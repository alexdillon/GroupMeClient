using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GroupMeClientApi.Models;

namespace GroupMeClient.ViewModels.Controls
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

            this.GroupAvatar = new AvatarControlViewModel(this.Group, this.Group.Client.ImageDownloader, true);
            this.GroupName = this.Group.Name;
            this.GroupDescription = this.Group.Description;
        }

        /// <summary>
        /// Gets the avatar for this <see cref="Group"/>.
        /// </summary>
        public AvatarControlViewModel GroupAvatar { get; }

        /// <summary>
        /// Gets the name of this <see cref="Group"/>.
        /// </summary>
        public string GroupName { get; }

        /// <summary>
        /// Gets the description for this <see cref="Group"/>.
        /// </summary>
        public string GroupDescription { get; }

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

        private Group Group { get; }

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
        public class GroupInfoPseudoPlugin : GroupMeClientPlugin.PluginBase, GroupMeClientPlugin.GroupChat.IGroupChatPlugin
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GroupInfoPseudoPlugin"/> class.
            /// </summary>
            /// <param name="group">The <see cref="Group"/> to display information for.</param>
            public GroupInfoPseudoPlugin(Group group)
            {
                this.Group = group;
                this.GroupInfoControlViewModel = new GroupInfoControlViewModel(this.Group);
            }

            /// <inheritdoc/>
            public override string PluginDisplayName => "Group Information";

            /// <inheritdoc/>
            public override string PluginVersion => ThisAssembly.SimpleVersion;

            /// <inheritdoc/>
            public string PluginName => this.PluginDisplayName;

            private Group Group { get; }

            private GroupInfoControlViewModel GroupInfoControlViewModel { get; }

            /// <inheritdoc/>
            public Task Activated(IMessageContainer groupOrChat)
            {
                var request = new Messaging.DialogRequestMessage(this.GroupInfoControlViewModel);
                Messenger.Default.Send(request);

                return Task.CompletedTask;
            }
        }
    }
}
