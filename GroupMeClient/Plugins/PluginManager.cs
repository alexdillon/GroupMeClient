using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        public ICollection<IGroupChatPlugin> GroupChatPlugins { get; } = new List<IGroupChatPlugin>();

        /// <summary>
        /// Gets the available <see cref="IMessageComposePlugin"/> plugins.
        /// </summary>
        public ICollection<IMessageComposePlugin> MessageComposePlugins { get; } = new List<IMessageComposePlugin>();

        /// <summary>
        /// Loads and registers all available plugins.
        /// </summary>
        /// <param name="pluginsPath">The folder to load plugins from.</param>
        public void LoadPlugins(string pluginsPath)
        {
            this.GroupChatPlugins.Clear();
            this.MessageComposePlugins.Clear();

            // Load plugins from the plugins folder
            if (Directory.Exists(pluginsPath))
            {
                string[] dllFileNames = Directory.GetFiles(pluginsPath, "*.dll");

                ICollection<Assembly> assemblies = new List<Assembly>(dllFileNames.Length);
                foreach (string dllFile in dllFileNames)
                {
                    AssemblyName an = AssemblyName.GetAssemblyName(dllFile);
                    Assembly assembly = Assembly.Load(an);
                    assemblies.Add(assembly);
                }

                Type pluginType = typeof(PluginBase);
                ICollection<Type> pluginTypes = new List<Type>();
                foreach (Assembly assembly in assemblies)
                {
                    if (assembly != null)
                    {
                        try
                        {
                            Type[] types = assembly.GetTypes();
                            foreach (Type type in types)
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

                foreach (Type type in pluginTypes)
                {
                    var plugin = (PluginBase)Activator.CreateInstance(type);

                    if (plugin is IMessageComposePlugin messageComposePlugin)
                    {
                        this.MessageComposePlugins.Add(messageComposePlugin);
                    }
                    else if (plugin is IGroupChatPlugin groupChatPlugin)
                    {
                        this.GroupChatPlugins.Add(groupChatPlugin);
                    }
                }
            }

            // Load plugins that ship directly in GMDC
            this.GroupChatPlugins.Add(new ViewModels.ImageGalleryWindowViewModel.ImageGalleryPlugin());
        }
    }
}
