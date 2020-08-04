using System;
using System.Collections.Generic;

namespace GroupMeClient.Core.Plugins
{
    /// <summary>
    /// <see cref="PluginSettings"/> defines the settings needed for installing and updating 3rd party plugins.
    /// </summary>
    public class PluginSettings
    {
        /// <summary>
        /// Gets or sets a listing of the <see cref="Repository"/>s that have been added for installing Plugins.
        /// </summary>
        public List<Repository> Repositories { get; set; } = new List<Repository>();

        /// <summary>
        /// Gets or sets a list of all the <see cref="InstalledPlugin"/>s.
        /// </summary>
        public List<InstalledPlugin> InstalledPlugins { get; set; } = new List<InstalledPlugin>();

        /// <summary>
        /// <see cref="InstalledPlugin"/> saves information about a specific <see cref="GroupMeClientPlugin.PluginBase"/>
        /// that has been installed through a <see cref="Repository"/>.
        /// </summary>
        public class InstalledPlugin
        {
            /// <summary>
            /// Gets or sets the identifier for this <see cref="InstalledPlugin"/>.
            /// </summary>
            public string InstallationGuid { get; set; }

            /// <summary>
            /// Gets or sets the name of this Plugin that is used to identify it in it's home <see cref="Repository"/>.
            /// </summary>
            public string PluginName { get; set; }

            /// <summary>
            /// Gets or sets the version of the plugin that is installed.
            /// </summary>
            public Version Version { get; set; }

            /// <summary>
            /// Gets or sets the URL of this <see cref="InstalledPlugin"/>'s home <see cref="Repository"/>.
            /// </summary>
            public string RepositoryUrl { get; set; }
        }
    }
}
