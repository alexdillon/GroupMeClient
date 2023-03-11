using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GroupMeClient.Core.Settings;
using GroupMeClient.Core.Tasks;
using GroupMeClientApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GroupMeClient.Core.Caching
{
    /// <summary>
    /// <see cref="SuperIndexer"/> provides a background process than run and receives GroupMe updates from the <see cref="ViewModels.ChatsViewModel"/> to ensure
    /// the message cache remains automatically updated.
    /// </summary>
    public class SuperIndexer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SuperIndexer"/> class.
        /// </summary>
        /// <param name="cacheManager">A manager for the caching database.</param>
        /// <param name="taskManager">The manager to use for indexing tasks.</param>
        /// <param name="settingsManager">The settings manager for the application.</param>
        public SuperIndexer(CacheManager cacheManager, TaskManager taskManager, SettingsManager settingsManager)
        {
            this.CacheManager = cacheManager;
            this.TaskManager = taskManager;
            this.SettingsManager = settingsManager;
        }

        private CacheManager CacheManager { get; }

        private TaskManager TaskManager { get; }

        private SettingsManager SettingsManager { get; }

        private IEnumerable<IMessageContainer> GroupsAndChats { get; set; }

        private List<string> OutdatedGroupIdList { get; set; } = new List<string>();

        private ConcurrentDictionary<string, List<Message>> GroupUpdates { get; } = new ConcurrentDictionary<string, List<Message>>();

        private ManualResetEvent WaitingOnGroupAndChatListings { get; } = new ManualResetEvent(true);

        private DateTime LastMetadataUpdate { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Begins an asychronous transaction to execute a background scan with an up-to-date
        /// listing of all the <see cref="Group"/> and <see cref="Chat"/> returned by the GroupMe API.
        /// Any applicable Group Updates should be submitted, and once all foreground loading is completed,
        /// the transaction should be closed with <see cref="EndTransaction"/>. To allow for asyncronous loading,
        /// <see cref="SubmitGroupUpdate(IMessageContainer, IEnumerable{Message})"/> and <see cref="EndTransaction"/>
        /// do not have to be called in order.
        /// </summary>
        /// <param name="groupsAndChats">An up to date listing of all <see cref="Group"/> and <see cref="Chat"/> from GroupMe, if available.</param>
        public void BeginAsyncTransaction(IEnumerable<IMessageContainer> groupsAndChats = null)
        {
            this.OutdatedGroupIdList = new List<string>();
            this.WaitingOnGroupAndChatListings.Reset();

            if (groupsAndChats != null)
            {
                Debug.WriteLine("Updated ChatsList in BeginAsyncTransaction()");
                this.GroupsAndChats = groupsAndChats;
                this.WaitingOnGroupAndChatListings.Set();
            }
        }

        /// <summary>
        /// Submits the most recent set of messages from a group or chat that has been loaded in the user interface
        /// to the <see cref="SuperIndexer"/> for processing and indexing if necessary.
        /// </summary>
        /// <param name="messageContainer">The <see cref="Group"/> or <see cref="Chat"/> that is being updated.</param>
        /// <param name="mostRecentMessages">A collection of the most recent messages, returned from GroupMe.</param>
        public void SubmitGroupUpdate(IMessageContainer messageContainer, IEnumerable<Message> mostRecentMessages)
        {
            var messages = this.GroupUpdates.GetOrAdd(messageContainer.Id, new List<Message>());
            messages.AddRange(mostRecentMessages);
            Debug.WriteLine("Submitted updates for chat " + messageContainer.Name);
        }

        /// <summary>
        /// Ends a transaction of updates to the <see cref="SuperIndexer"/> and indicates that
        /// all foreground loading of GroupMe updates has completed. The <see cref="SuperIndexer"/>
        /// will continue to process necessary delta updates in the background.
        /// </summary>
        public void EndTransaction()
        {
            Task.Run(async () =>
            {
                Debug.WriteLine("Ending Super Indexer Transaction...");

                var result = this.WaitingOnGroupAndChatListings.WaitOne(TimeSpan.FromSeconds(10));

                if (!result)
                {
                    Debug.WriteLine("No group list available, loading new...");

                    var client = this.GroupsAndChats.FirstOrDefault()?.Client;
                    if (client == null)
                    {
                        return;
                    }

                    await client.GetGroupsAsync();
                    await client.GetChatsAsync();
                    this.GroupsAndChats = Enumerable.Concat<IMessageContainer>(client.Groups(), client.Chats());
                }

                this.OutdatedGroupIdList = this.CheckForOutdatedCache(this.GroupsAndChats);

                Debug.WriteLine("Dirty groups computed, count " + this.OutdatedGroupIdList.Count);

                using (var context = this.CacheManager.OpenNewContext())
                {
                    // Update Group and Chat metadata in cache
                    if (DateTime.Now.Subtract(this.LastMetadataUpdate) > this.SettingsManager.CoreSettings.MetadataCacheInterval)
                    {
                        this.LastMetadataUpdate = DateTime.Now;

                        // Remove old metadata. Doing a removal and addition in the same cycle was causing
                        // OtherUserId foreign key for Chats to be null. Doing true updates with cascading deletes
                        // should be possible, but this can be done easily in SQLite without any further migrations (GMDC 33.0.3)
                        foreach (var metaData in this.GroupsAndChats)
                        {
                            if (metaData is Group groupMetadata)
                            {
                                var existing = context.GroupMetadata
                                    .Include(g => g.Members)
                                    .FirstOrDefault(g => g.Id == groupMetadata.Id);
                                if (existing != null)
                                {
                                    foreach (var member in existing.Members)
                                    {
                                        context.Remove(member);
                                    }

                                    context.GroupMetadata.Remove(existing);
                                }
                            }
                            else if (metaData is Chat chatMetadata)
                            {
                                var existingChat = context.ChatMetadata.FirstOrDefault(c => c.Id == metaData.Id);
                                if (existingChat != null)
                                {
                                    context.Remove(existingChat);
                                }

                                var existingMember = context.Find<Member>(chatMetadata.OtherUser.Id);
                                if (existingMember != null)
                                {
                                    context.Remove(existingMember);
                                }
                            }
                        }

                        context.SaveChanges();

                        foreach (var addMetaData in this.GroupsAndChats)
                        {
                            context.Add(addMetaData);
                        }

                        context.SaveChanges();
                    }

                    // Process updates for each group and chat
                    var fullyUpdatedGroupIds = new List<string>();
                    foreach (var id in this.GroupUpdates.Keys)
                    {
                        var messages = this.GroupUpdates[id];
                        var groupState = context.IndexStatus.Find(id);
                        if (groupState == null)
                        {
                            // No cache status exists for this group. Force a full re-index.
                        }
                        else if (this.OutdatedGroupIdList.Contains(id))
                        {
                            var availableMessageIds = messages.Select(m => long.Parse(m.Id)).ToList();
                            var messageContainer = this.GroupsAndChats.FirstOrDefault(c => c.Id == id);

                            long.TryParse(groupState.LastIndexedId, out var lastIndexId);
                            if (availableMessageIds.Contains(lastIndexId))
                            {
                                // All new messages have already been loaded and are ready to index.
                                var newMessages = new List<Message>();
                                var newLastIndexId = lastIndexId;
                                foreach (var msg in messages)
                                {
                                    if (long.TryParse(msg.Id, out var messageId) && messageId > lastIndexId)
                                    {
                                        newMessages.Add(msg);

                                        if (messageId > newLastIndexId)
                                        {
                                            newLastIndexId = messageId;
                                        }
                                    }
                                }

                                context.AddMessages(newMessages);
                                groupState.LastIndexedId = newLastIndexId.ToString();
                                context.SaveChanges();
                                fullyUpdatedGroupIds.Add(id);
                            }
                        }
                    }

                    Debug.WriteLine("In place deltas applied, resolved " + fullyUpdatedGroupIds.Count);

                    // Pass 2, go through all originally outdated chats and run the complete re-index task on them
                    // if they couldn't be automatically updated with available messages.
                    foreach (var id in this.OutdatedGroupIdList)
                    {
                        if (!fullyUpdatedGroupIds.Contains(id))
                        {
                            var container = this.GroupsAndChats.FirstOrDefault(c => c.Id == id);
                            var cts = new CancellationTokenSource();

                            Debug.WriteLine("Full index scan required for " + container.Name);

                            // Don't start multiple overlapping indexing tasks.
                            var existingScan = this.TaskManager.RunningTasks.FirstOrDefault(t => t.Tag == id);
                            if (existingScan == null)
                            {
                                this.TaskManager.AddTask(
                                    $"Indexing {container.Name}",
                                    id,
                                    this.IndexGroup(container, cts),
                                    cts);
                            }
                        }
                    }
                }

                this.GroupUpdates.Clear();
                this.WaitingOnGroupAndChatListings.Reset();
            });
        }

        private List<string> CheckForOutdatedCache(IEnumerable<IMessageContainer> groupsAndChats)
        {
            if (groupsAndChats == null)
            {
                return new List<string>();
            }

            var outdatedGroupsAndChatsIds = new List<string>();
            using (var context = this.CacheManager.OpenNewContext())
            {
                foreach (var messageContainer in groupsAndChats)
                {
                    var groupState = context.IndexStatus.Find(messageContainer.Id);
                    if (groupState == null)
                    {
                        // No cache status exists for this group, so it must be out of date.
                        outdatedGroupsAndChatsIds.Add(messageContainer.Id);
                    }
                    else
                    {
                        // Compare the last indexed ID in the DB with the ID of the most recent preview message.
                        long.TryParse(groupState.LastIndexedId, out var lastIndexId);
                        long.TryParse(messageContainer.LatestMessage.Id, out var newestMessageId);
                        if (lastIndexId < newestMessageId)
                        {
                            // Outdated
                            outdatedGroupsAndChatsIds.Add(messageContainer.Id);
                        }
                    }
                }
            }

            return outdatedGroupsAndChatsIds;
        }

        private async Task IndexGroup(IMessageContainer container, CancellationTokenSource cancellationTokenSource)
        {
            using (var context = this.CacheManager.OpenNewContext())
            {
                var groupState = context.IndexStatus.Find(container.Id);

                if (groupState == null)
                {
                    groupState = new Models.GroupIndexStatus()
                    {
                        Id = container.Id,
                    };
                    context.IndexStatus.Add(groupState);
                }

                async Task<ICollection<Message>> InitialDownloadAction()
                {
                    return await container.GetMessagesAsync();
                }

                var newestMessages = await Utilities.ReliabilityStateMachine.TryUntilSuccess(InitialDownloadAction, cancellationTokenSource.Token);

                context.AddMessages(newestMessages);

                long.TryParse(groupState.LastIndexedId, out var lastIndexId);
                long.TryParse(newestMessages.Last().Id, out var retreiveFrom);

                while (lastIndexId < retreiveFrom && !cancellationTokenSource.IsCancellationRequested)
                {
                    // not up-to-date, we need to retreive the delta
                    async Task<ICollection<Message>> DownloadDeltaAction()
                    {
                        return await container.GetMaxMessagesAsync(GroupMeClientApi.MessageRetreiveMode.BeforeId, retreiveFrom.ToString());
                    }

                    var results = await Utilities.ReliabilityStateMachine.TryUntilSuccess(DownloadDeltaAction, cancellationTokenSource.Token);

                    context.AddMessages(results);

                    if (results.Count == 0)
                    {
                        // we've hit the top.
                        break;
                    }

                    long.TryParse(results.Last().Id, out var latestRetreivedOldestId);
                    retreiveFrom = latestRetreivedOldestId;
                }

                groupState.LastIndexedId = newestMessages.First().Id; // everything is downloaded
                await context.SaveChangesAsync(cancellationTokenSource.Token);
            }
        }
    }
}
