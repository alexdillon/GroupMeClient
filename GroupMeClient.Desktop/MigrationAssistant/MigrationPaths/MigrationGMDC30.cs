using System.Data;
using System.IO;
using GroupMeClient.Core;
using GroupMeClient.Core.Caching;
using GroupMeClient.Core.Caching.Models;
using GroupMeClient.Core.Settings;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace GroupMeClient.Desktop.MigrationAssistant
{
    public class MigrationGMDC30
    {
        public const int MigrationVersion = 1;

        public bool DoMigration(Startup.StartupParameters parameters)
        {
            // In GMDC 30:
            // - The Migration Version field in Core Settings is 1.0
            // - The plugin file hasn't changed, but the namespace for GitHub Repos has and must be updated
            // - Last Read State has been removed from settings.json and moved into persist.db
            // - Starred and Hidden messages have been moved from cache.db to persist.db

            this.FixPluginFile(Path.Combine(parameters.PluginPath, "plugin.json"));
            this.FixReadStatus(parameters.PersistFilePath, parameters.SettingsFilePath);
            this.FixStarredHiddenMessages(parameters.PersistFilePath, parameters.CacheFilePath);
            this.SetMigrationVersion(parameters.SettingsFilePath);

            return true;
        }

        private void FixPluginFile(string pluginFilePath)
        {
            if (File.Exists(pluginFilePath))
            {
                // Just fix the namespace for GitHubRepos, the only known type in GMDC <30 to be in the new namespace.
                var contents = File.ReadAllText(pluginFilePath);
                contents = contents.Replace(
                    "\"$type\": \"GroupMeClient.Plugins.Repositories.GitHubRepository, GroupMeClient\"",
                    "\"$type\": \"GroupMeClient.Core.Plugins.Repositories.GitHubRepository, GroupMeClient.Core\"");

                File.WriteAllText(pluginFilePath, contents);
            }
        }

        private void FixReadStatus(string persistPath, string settingsFilePath)
        {
            var persistManager = new PersistManager(persistPath);
            using (var persistContext = persistManager.OpenNewContext())
            {
                try
                {
                    JObject settings = JObject.Parse(File.ReadAllText(settingsFilePath));
                    var states = settings["ChatsSettings"]["GroupChatStates"];
                    if (states != null)
                    {
                        foreach (var state in states)
                        {
                            try
                            {
                                var key = state["GroupOrChatId"].Value<string>();

                                if (persistContext.GroupChatStates.Find(key) == null)
                                {
                                    persistContext.GroupChatStates.Add(new GroupOrChatState()
                                    {
                                        GroupOrChatId = key,
                                        LastTotalMessageCount = state["LastTotalMessageCount"].Value<int>(),
                                    });
                                }
                            }
                            catch (System.Exception)
                            {
                                // Not critial data - just throw it away
                            }
                        }
                    }
                    
                    persistContext.SaveChanges();
                }
                catch (System.Exception)
                {
                }
            }
        }

        private void FixStarredHiddenMessages(string persistPath, string cachePath)
        {
            var cacheManager = new CacheManager(cachePath, null);
            var persistManager = new PersistManager(persistPath);
        
            using (var persistContext = persistManager.OpenNewContext())
            {
                using (var cacheContext = cacheManager.OpenUnmigratedContext())
                {
                    var conn = cacheContext.Database.GetDbConnection();
                    if (conn.State.Equals(ConnectionState.Closed))
                    {
                        conn.Open();
                    }

                    // Do Starred Messages
                    using (var command = conn.CreateCommand())
                    {
                        command.CommandText = @"SELECT name FROM sqlite_master WHERE type='table' AND name='StarredMessages';";
                        var exists = command.ExecuteScalar() != null;

                        if (exists)
                        {
                            using (var getStars = conn.CreateCommand())
                            {
                                getStars.CommandText = "SELECT * FROM StarredMessages";
                                var reader = getStars.ExecuteReader();
                                while (reader.Read())
                                {
                                    var starMessage = new StarredMessage() { ConversationId = (string)reader["ConversationId"], MessageId = (string)reader["MessageId"] };
                                    persistContext.StarredMessages.Add(starMessage);
                                }
                            }
                        }
                    }

                    // Do Hidden Messages
                    using (var command = conn.CreateCommand())
                    {
                        command.CommandText = @"SELECT name FROM sqlite_master WHERE type='table' AND name='HiddenMessages';";
                        var exists = command.ExecuteScalar() != null;

                        if (exists)
                        {
                            using (var getHidden = conn.CreateCommand())
                            {
                                getHidden.CommandText = "SELECT * FROM HiddenMessages";
                                var reader = getHidden.ExecuteReader();
                                while (reader.Read())
                                {
                                    var hiddenMessage = new HiddenMessage() { ConversationId = (string)reader["ConversationId"], MessageId = (string)reader["MessageId"] };
                                    persistContext.HiddenMessages.Add(hiddenMessage);
                                }
                            }
                        }
                    }
                }

                persistContext.SaveChanges();
            }
        }

        private void SetMigrationVersion(string settingsFilePath)
        {
            var settingsManager = new SettingsManager(settingsFilePath);
            settingsManager.LoadSettings();
            settingsManager.CoreSettings.MigrationVersion = MigrationVersion;
            settingsManager.SaveSettings();
        }
    }
}
