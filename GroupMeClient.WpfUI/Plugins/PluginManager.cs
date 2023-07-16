using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using GroupMeClient.Core.Plugins;
using GroupMeClient.Core.Services;
using GroupMeClientPlugin;
using GroupMeClientPlugin.GroupChat;
using GroupMeClientPlugin.MessageCompose;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace GroupMeClient.WpfUI.Plugins
{
    /// <summary>
    /// <see cref="PluginManager"/> provides functionality to dynamically load <see cref="IPluginBase"/>-based
    /// plugins and register them to extend Client functionality.
    /// </summary>
    /// <remarks>
    /// Based on https://code.msdn.microsoft.com/windowsdesktop/Creating-a-simple-plugin-b6174b62.
    /// </remarks>
    public sealed class PluginManager : IPluginManagerService
    {
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
            Directory.CreateDirectory(Path.Combine(pluginsPath, PluginInstaller.PackagesDirectory));

            if (Directory.Exists(pluginsPath))
            {
                // Handle staged removals
                foreach (var file in Directory.GetFiles(Path.Combine(pluginsPath, PluginInstaller.PackagesDirectory), "*.rm"))
                {
                    if (Path.GetFileNameWithoutExtension(file).EndsWith(PluginInstaller.StagingSuffix))
                    {
                        var originalPluginName = Path.GetFileNameWithoutExtension(file).Replace(PluginInstaller.StagingSuffix, string.Empty);
                        var originalPluginFullPath = Path.Combine(pluginsPath, PluginInstaller.PackagesDirectory, originalPluginName);

                        if (new FileInfo(file).Length == 0)
                        {
                            // Delete the zero-byte staging stub
                            File.Delete(file);

                            // Delete the staged installation directory
                            if (Directory.Exists(originalPluginFullPath))
                            {
                                Directory.Delete(originalPluginFullPath, true);
                            }
                        }
                    }
                }

                // Handle staged upgrades
                foreach (var directory in Directory.GetDirectories(Path.Combine(pluginsPath, PluginInstaller.PackagesDirectory)))
                {
                    if (Path.GetFileNameWithoutExtension(directory).EndsWith(PluginInstaller.StagingSuffix))
                    {
                        var originalPluginName = Path.GetFileNameWithoutExtension(directory).Replace(PluginInstaller.StagingSuffix, string.Empty);
                        var originalPluginFullPath = Path.Combine(pluginsPath, PluginInstaller.PackagesDirectory, originalPluginName);

                        if (Directory.Exists(originalPluginFullPath))
                        {
                            Directory.Delete(originalPluginFullPath, true);
                        }

                        Directory.Move(directory, originalPluginFullPath);
                    }
                }

                var dllFileNames = Directory.GetFiles(pluginsPath, "*.dll", SearchOption.AllDirectories);

                var assemblies = new List<Assembly>(dllFileNames.Length);
                foreach (string dllFile in dllFileNames)
                {
                    assemblies.Add(LoadPlugin(dllFile));
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

                var pluginInstaller = Ioc.Default.GetService<PluginInstaller>();

                foreach (var type in pluginTypes)
                {
                    var hostedAssemblyName = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(type.Module.Assembly.Location));
                    var installedPlugin = pluginInstaller.InstalledPlugins.FirstOrDefault(p => p.InstallationGuid == hostedAssemblyName);

                    var plugin = (PluginBase)Activator.CreateInstance(type);

                    if (plugin is IMessageComposePlugin messageComposePlugin)
                    {
                        if (installedPlugin == null)
                        {
                            this.MessageComposePluginsManuallyInstalled.Add(messageComposePlugin);
                        }
                        else
                        {
                            this.MessageComposePluginsAutoInstalled.Add(messageComposePlugin);
                        }
                    }
                    else if (plugin is IGroupChatPlugin groupChatPlugin)
                    {
                        if (installedPlugin == null)
                        {
                            this.GroupChatPluginsManuallyInstalled.Add(groupChatPlugin);
                        }
                        else
                        {
                            this.GroupChatPluginsAutoInstalled.Add(groupChatPlugin);
                        }
                    }
                }
            }

            // Load plugins that ship directly in GMDC/Wpf
            this.GroupChatPluginsBuiltIn.Add(new ImageGalleryPlugin());
        }

        private static Assembly LoadPlugin(string fullPath)
        {
            PluginLoadContext loadContext = new PluginLoadContext(fullPath);
            return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(fullPath)));
        }
    }
}
