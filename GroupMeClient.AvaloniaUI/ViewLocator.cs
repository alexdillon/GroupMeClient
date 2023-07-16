using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace GroupMeClient.AvaloniaUI
{
    /// <summary>
    /// <see cref="ViewLocator"/> provides a universal Data Template to map between ViewModels and Views.
    /// </summary>
    public class ViewLocator : IDataTemplate
    {
        /// <inheritdoc/>
        Control ITemplate<object, Control>.Build(object data)
        {
            var originalName = data.GetType().FullName;

            // Change from GroupMe.Core.ViewModels.xViewModel to GroupMeClient.AvaloniaUI.Views.xView
            var viewName = originalName.Replace("GroupMeClient.Core.ViewModels", "GroupMeClient.AvaloniaUI.Views").Replace("ViewModel", "View");
            var type = Type.GetType(viewName);
            if (type != null && viewName != originalName)
            {
                var control = (Control)Activator.CreateInstance(type);
                return control;
            }

            // Try it without "View" (ex. GroupControlsControlViewModel -> GroupContentsControl)
            type = Type.GetType(viewName.Substring(0, viewName.LastIndexOf("View")));
            if (type != null && viewName != originalName)
            {
                var control = (Control)Activator.CreateInstance(type);
                return control;
            }

            // Handle things that are already in the GMDCA namespace
            viewName = data.GetType().FullName.Replace("ViewModel", "View");
            type = Type.GetType(viewName);
            if (type != null && viewName != originalName)
            {
                var control = (Control)Activator.CreateInstance(type);
                return control;
            }

            return new TextBlock { Text = "Not Found: " + viewName };
        }

        /// <inheritdoc/>
        public bool Match(object data)
        {
            return data is ObservableObject;
        }
    }
}