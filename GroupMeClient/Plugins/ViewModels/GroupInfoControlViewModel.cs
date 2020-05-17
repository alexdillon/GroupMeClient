using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GroupMeClient.ViewModels.Controls;
using GroupMeClientApi.Models;
using GroupMeClientPlugin;
using GroupMeClientPlugin.GroupChat;
using Microsoft.Win32;

namespace GroupMeClient.Plugins.ViewModels
{
    /// <summary>
    /// <see cref="GroupInfoControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.GroupInfoControlViewModel"/> control.
    /// </summary>
    public class GroupInfoControlViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupInfoControlViewModel"/> class.
        /// </summary>
        /// <param name="group">The <see cref="Group"/> to display information for.</param>
        public GroupInfoControlViewModel(Group group)
        {
            this.Group = group;
            this.UpdateDisplay();

            this.EditNicknameCommand = new RelayCommand(async () => await this.ToggleName());
            this.ChangeAvatarCommand = new RelayCommand(async () => await this.ChangeAvatar());
            this.ResetAvatarToProfileCommand = new RelayCommand(async () => await this.ResetAvatarToProfile());
        }

        /// <summary>
        /// Gets the command to execute to begin or end editing a user's nickname.
        /// </summary>
        public ICommand EditNicknameCommand { get; }

        /// <summary>
        /// Gets the command to execute change a user's avatar.
        /// </summary>
        public ICommand ChangeAvatarCommand { get; }

        /// <summary>
        /// Gets the command to execute reset a user's avatar to their profile image.
        /// </summary>
        public ICommand ResetAvatarToProfileCommand { get; }

        /// <summary>
        /// Gets or sets  the avatar for this <see cref="Group"/>.
        /// </summary>
        public AvatarControlViewModel GroupAvatar { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a custom avatar is set.
        /// Custom avatars allow setting a unique avatar for a <see cref="Member"/>
        /// in a <see cref="Group"/>, as opposed to using their global profile avatar.
        /// </summary>
        public bool IsCustomAvatarSet { get; set; }

        /// <summary>
        /// Gets or sets the name of this <see cref="Group"/>.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the description for this <see cref="Group"/>.
        /// </summary>
        public string GroupDescription { get; set; }

        /// <summary>
        /// Gets or sets the current <see cref="Member"/>'s nickname in this <see cref="Group"/>.
        /// </summary>
        public string MemberNickname { get; set; }

        /// <summary>
        /// Gets or sets the current <see cref="Member"/>'s avatar in this <see cref="Group"/>.
        /// </summary>
        public AvatarControlViewModel MemberAvatar { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the nickname is currently being edited.
        /// </summary>
        public bool IsEditingNickname { get; set; }

        /// <summary>
        /// Gets a listing of members in this <see cref="Group"/>.
        /// </summary>
        public IEnumerable<GroupMember> GroupMembers
        {
            get
            {
                foreach (var member in this.Group.Members.ToList())
                {
                    yield return new GroupMember
                    {
                        MemberInfo = member,
                        Avatar = new AvatarControlViewModel(member, this.Group.Client.ImageDownloader),
                    };
                }
            }
        }

        private Group Group { get; set; }

        private async Task ReloadGroupInfo()
        {
            var groups = await this.Group.Client.GetGroupsAsync();
            this.Group = groups.First(g => g.Id == this.Group.Id);
        }

        private void UpdateDisplay()
        {
            this.GroupAvatar = new AvatarControlViewModel(this.Group, this.Group.Client.ImageDownloader, true);
            this.GroupName = this.Group.Name;
            this.GroupDescription = this.Group.Description;

            var me = this.Group.WhoAmI();
            var meGroupMember = this.Group.Members.First(m => m.UserId == me.UserId);
            this.MemberNickname = meGroupMember.Nickname;
            this.MemberAvatar = new AvatarControlViewModel(meGroupMember, this.Group.Client.ImageDownloader, true);
            this.IsCustomAvatarSet = !(meGroupMember.ImageOrAvatarUrl == me.ImageOrAvatarUrl);
        }

        private async Task ToggleName()
        {
            if (this.IsEditingNickname)
            {
                _ = await this.Group.UpdateNickname(this.MemberNickname);
                await this.ReloadGroupInfo();
                this.UpdateDisplay();
            }

            this.IsEditingNickname = !this.IsEditingNickname;
            this.RaisePropertyChanged(string.Empty);
        }

        private async Task<bool> ResetAvatarToProfile()
        {
            var result = await this.Group.UpdateMemberAvatar(null);
            await this.ReloadGroupInfo();
            this.UpdateDisplay();
            this.RaisePropertyChanged(string.Empty);

            return result;
        }

        private async Task<bool> ChangeAvatar()
        {
            var supportedImages = GroupMeClientApi.Models.Attachments.ImageAttachment.SupportedExtensions.ToList();

            var imageExtensions = string.Join(";", supportedImages.Select(x => "*" + x));
            var imageExtensionsDisplay = string.Join(", ", supportedImages.Select(x => "*" + x));

            var imageFilter = $"Images ({imageExtensionsDisplay})|{imageExtensions}";

            var openFileDialog = new OpenFileDialog
            {
                Filter = imageFilter,
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var extension = Path.GetExtension(openFileDialog.FileName);
                if (supportedImages.Contains(extension))
                {
                    var imageData = File.ReadAllBytes(openFileDialog.FileName);
                    var result = await this.Group.UpdateMemberAvatar(imageData);

                    await this.ReloadGroupInfo();
                    this.UpdateDisplay();
                    this.RaisePropertyChanged(string.Empty);

                    return result;
                }
            }

            return false;
        }

        /// <summary>
        /// <see cref="GroupMember"/> contains information about a group member and their avatar.
        /// </summary>
        public class GroupMember
        {
            /// <summary>
            /// Gets or sets the member information.
            /// </summary>
            public Member MemberInfo { get; set; }

            /// <summary>
            /// Gets or sets the displayable avatar for this member.
            /// </summary>
            public AvatarControlViewModel Avatar { get; set; }
        }

        /// <summary>
        /// <see cref="GroupInfoPseudoPlugin"/> defines a plugin-style object that can be used to integrate
        /// Group Information functionality into a <see cref="GroupContentsControlViewModel"/>.
        /// </summary>
        public class GroupInfoPseudoPlugin : PluginBase, IGroupChatPlugin
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GroupInfoPseudoPlugin"/> class.
            /// </summary>
            /// <param name="group">The <see cref="Group"/> to display information for.</param>
            public GroupInfoPseudoPlugin(Group group)
            {
                this.Group = group;
            }

            /// <inheritdoc/>
            public override string PluginDisplayName => "Group Information";

            /// <inheritdoc/>
            public override string PluginVersion => ThisAssembly.SimpleVersion;

            /// <inheritdoc/>
            public string PluginName => this.PluginDisplayName;

            /// <inheritdoc/>
            public override Version ApiVersion => new Version(2, 0, 0);

            private Group Group { get; }

            /// <inheritdoc/>
            public Task Activated(IMessageContainer groupOrChat, CacheSession cacheSession, IPluginUIIntegration integration, Action<CacheSession> cleanup)
            {
                var request = new Messaging.DialogRequestMessage(new GroupInfoControlViewModel(this.Group));
                Messenger.Default.Send(request);

                cleanup(cacheSession);
                return Task.CompletedTask;
            }
        }
    }
}
