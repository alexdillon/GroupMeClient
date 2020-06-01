using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using GroupMeClientPlugin;
using GroupMeClientPlugin.GroupChat;
using GroupMeClientPlugin.MessageCompose;

namespace GroupMeClient.Plugins
{
    /// <summary>
    /// <see cref="PluginManager"/> provides functionality to dynamically load <see cref="IPluginBase"/>-based
    /// plugins and register them to extend Client functionality.
    /// </summary>
    /// <remarks>
    /// Based on https://code.msdn.microsoft.com/windowsdesktop/Creating-a-simple-plugin-b6174b62.
    /// </remarks>
    public sealed class PluginManager
    {
        private static readonly Lazy<PluginManager> LazyPluginManager = new Lazy<PluginManager>(() => new PluginManager());

        private PluginManager()
        {
        }

        /// <summary>
        /// Gets the instance of the <see cref="PluginManager"/> for the current application.
        /// </summary>
        public static PluginManager Instance => LazyPluginManager.Value;

        /// <summary>
        /// Gets the available <see cref="IGroupChatPlugin"/> plugins.
        /// </summary>
        public IEnumerable<IGroupChatPlugin> GroupChatPlugins
        {
            get
            {
                return Enumerable.Concat(this.GroupChatPluginsAutoInstalled, this.GroupChatPluginsBuiltIn).Concat(this.GroupChatPluginsManuallyInstalled);
            }
        }

        /// <summary>
        /// Gets the available <see cref="IGroupChatPlugin"/> plugins that are built-in.
        /// </summary>
        public ICollection<IGroupChatPlugin> GroupChatPluginsBuiltIn { get; } = new List<IGroupChatPlugin>();

        /// <summary>
        /// Gets the available <see cref="IGroupChatPlugin"/> plugins that are manually installed.
        /// </summary>
        public ICollection<IGroupChatPlugin> GroupChatPluginsManuallyInstalled { get; } = new List<IGroupChatPlugin>();

        /// <summary>
        /// Gets the available <see cref="IGroupChatPlugin"/> plugins that are automatically installed from a repo.
        /// </summary>
        public ICollection<IGroupChatPlugin> GroupChatPluginsAutoInstalled { get; } = new List<IGroupChatPlugin>();

        /// <summary>
        /// Gets the available <see cref="IMessageComposePlugin"/> plugins.
        /// </summary>
        public IEnumerable<IMessageComposePlugin> MessageComposePlugins
        {
            get
            {
                return Enumerable.Concat(this.MessageComposePluginsAutoInstalled, this.MessageComposePluginsBuiltIn).Concat(this.MessageComposePluginsManuallyInstalled);
            }
        }

        /// <summary>
        /// Gets the available <see cref="IMessageComposePlugin"/> plugins.
        /// </summary>
        public ICollection<IMessageComposePlugin> MessageComposePluginsBuiltIn { get; } = new List<IMessageComposePlugin>();

        /// <summary>
        /// Gets the available <see cref="IMessageComposePlugin"/> plugins.
        /// </summary>
        public ICollection<IMessageComposePlugin> MessageComposePluginsManuallyInstalled { get; } = new List<IMessageComposePlugin>();

        /// <summary>
        /// Gets the available <see cref="IMessageComposePlugin"/> plugins.
        /// </summary>
        public ICollection<IMessageComposePlugin> MessageComposePluginsAutoInstalled { get; } = new List<IMessageComposePlugin>();

        /// <summary>
        /// Loads and registers all available plugins.
        /// </summary>
        /// <param name="pluginsPath">The folder to load plugins from.</param>
        public void LoadPlugins(string pluginsPath)
        {
            this.GroupChatPluginsAutoInstalled.Clear();
            this.GroupChatPluginsBuiltIn.Clear();
            this.GroupChatPluginsManuallyInstalled.Clear();

            this.MessageComposePluginsAutoInstalled.Clear();
            this.MessageComposePluginsBuiltIn.Clear();
            this.MessageComposePluginsManuallyInstalled.Clear();

            Directory.CreateDirectory(pluginsPath);

            if (Directory.Exists(pluginsPath))
            {
                var dllFileNames = Directory.GetFiles(pluginsPath, "*.dll");

                foreach (var file in dllFileNames)
                {
                    if (Path.GetFileNameWithoutExtension(file).EndsWith(PluginInstaller.StagingSuffix))
                    {
                        var originalPluginName = Path.GetFileName(file).Replace(PluginInstaller.StagingSuffix, string.Empty);
                        var originalPluginFullPath = Path.Combine(pluginsPath, originalPluginName);
                        if (File.Exists(originalPluginFullPath))
                        {
                            File.Delete(originalPluginFullPath);
                        }

                        // If a zero-byte staging file is used, don't keep it.
                        if (new FileInfo(file).Length > 0)
                        {
                            File.Move(file, originalPluginFullPath);
                        }
                    }
                }

                // Get a new listing after staged installations are completed.
                dllFileNames = Directory.GetFiles(pluginsPath, "*.dll");

                var assemblies = new List<Assembly>(dllFileNames.Length);
                foreach (string dllFile in dllFileNames)
                {
                    var an = AssemblyName.GetAssemblyName(dllFile);
                    var assembly = Assembly.Load(an);
                    assemblies.Add(assembly);
                }

                var pluginType = typeof(PluginBase);
                var pluginTypes = new List<Type>();
                foreach (var assembly in assemblies)
                {
                    if (assembly != null)
                    {
                        try
                        {
                            var types = assembly.GetTypes();
                            foreach (var type in types)
                            {
                                if (type.IsInterface || type.IsAbstract)
                                {
                                    continue;
                                }
                                else
                                {
                                    if (type.IsSubclassOf(pluginType))
                                    {
                                        pluginTypes.Add(type);
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            Debug.WriteLine($"Failed to load plugin {assembly.FullName}");
                        }
                    }
                }

                foreach (var type in pluginTypes)
                {
                    var plugin = (PluginBase)Activator.CreateInstance(type);

                    if (plugin is IMessageComposePlugin messageComposePlugin)
                    {
                        this.MessageComposePluginsManuallyInstalled.Add(messageComposePlugin);
                    }
                    else if (plugin is IGroupChatPlugin groupChatPlugin)
                    {
                        this.GroupChatPluginsManuallyInstalled.Add(groupChatPlugin);
                    }
                }
            }

            // Load plugins that ship directly in GMDC
            this.GroupChatPluginsBuiltIn.Add(new ViewModels.ImageGalleryWindowViewModel.ImageGalleryPlugin());
        }
    }
}
