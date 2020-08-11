using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GroupMeClient.Core.Plugins
{
    /// <summary>
    /// <see cref="Repository"/> defines an online repository used to host plugins.
    /// </summary>
    public abstract class Repository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Repository"/> class.
        /// </summary>
        /// <param name="url">The URL of the repository.</param>
        public Repository(string url)
        {
            this.Url = url;
        }

        /// <summary>
        /// Gets the URL of the repository.
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Connects to the repository and downloads an up-to-date listing of all available plugins.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of <see cref="AvailablePlugin"/>.</returns>
        public abstract Task<List<AvailablePlugin>> GetAvailablePlugins();

        /// <summary>
        /// <see cref="AvailablePlugin"/> defines a specific plugin that is hosted in a <see cref="Repository"/>.
        /// </summary>
        public class AvailablePlugin
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AvailablePlugin"/> class.
            /// </summary>
            /// <param name="name">The name of the plugin.</param>
            /// <param name="binaryUrl">The URL to the downloadable binary for this plugin.</param>
            /// <param name="sha1Hash">The hash of the plugin package.</param>
            /// <param name="version">The name of the <see cref="Repository"/> this plugin is hosted in.</param>
            /// <param name="source">The newest available version of the plugin.</param>
            /// <param name="parentRepo">The parent <see cref="Repository"/>.</param>
            public AvailablePlugin(string name, string binaryUrl, string sha1Hash, Version version, string source, Repository parentRepo)
            {
                this.Name = name;
                this.BinaryUrl = binaryUrl;
                this.Sha1Hash = sha1Hash;
                this.Version = version;
                this.Source = source;
                this.ParentRepo = parentRepo;
            }

            /// <summary>
            /// Gets the name of the plugin.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Gets the URL to download the plugin binary.
            /// </summary>
            public string BinaryUrl { get; }

            /// <summary>
            /// Gets the SHA1 hash of the downloadable package for this plugin.
            /// </summary>
            public string Sha1Hash { get; }

            /// <summary>
            /// Gets the latest available version of the plugin.
            /// </summary>
            public Version Version { get; }

            /// <summary>
            /// Gets the a string identifying the <see cref="Repository"/> this <see cref="AvailablePlugin"/> is hosted in.
            /// </summary>
            public string Source { get; }

            /// <summary>
            /// Gets the parent <see cref="Repository"/>.
            /// </summary>
            public Repository ParentRepo { get; }
        }
    }
}
