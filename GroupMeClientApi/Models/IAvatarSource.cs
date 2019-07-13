using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GroupMeClientApi.Models
{
    /// <summary>
    /// <see cref="IAvatarSource"/> represents a generic object that has an avatar or profile picture.
    /// </summary>
    public interface IAvatarSource
    {
        /// <summary>
        /// Gets the avatar URL for this <see cref="IAvatarSource"/>.
        string ImageOrAvatarUrl { get; }

        /// <summary>
        /// Gets a value indicating whether the avatar image should be rounded for this <see cref="IAvatarSource"/>.
        /// </summary>
        bool IsRoundedAvatar { get; }
    }
}
