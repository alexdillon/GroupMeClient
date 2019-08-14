using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;

namespace GroupMeClient.Extensions
{
    /// <summary>
    /// <see cref="TaskbarItemOverlayExtensions"/> provides support for dynamically creating taskbar badges and binding them to
    /// a <see cref="TaskbarItemInfo"/>.
    /// </summary>
    /// <remarks>
    /// Adapted from https://thomasfreudenberg.com/archive/2010/08/15/creating-dynamic-windows-7-taskbar-overlay-icons-the-mvvm-way/.
    /// </remarks>
    public class TaskbarItemOverlayExtensions
    {
        /// <summary>
        /// The Dependency Property for the Content property for <see cref="TaskbarItemInfo"/>.
        /// </summary>
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.RegisterAttached(
                "Content",
                typeof(object),
                typeof(TaskbarItemOverlayExtensions),
                new PropertyMetadata(OnPropertyChanged));

        /// <summary>
        /// The Dependency Property for the Template property for <see cref="TaskbarItemInfo"/>.
        /// </summary>
        public static readonly DependencyProperty TemplateProperty =
            DependencyProperty.RegisterAttached(
            "Template",
            typeof(DataTemplate),
            typeof(TaskbarItemOverlayExtensions),
            new PropertyMetadata(OnPropertyChanged));

        /// <summary>
        /// Gets the content property for a specified <see cref="TaskbarItemInfo"/>.
        /// </summary>
        /// <param name="dependencyObject">The dependency object to retrieve the property from.</param>
        /// <returns>The content property.</returns>
        public static object GetContent(DependencyObject dependencyObject)
        {
            return dependencyObject.GetValue(ContentProperty);
        }

        /// <summary>
        /// Sets the content property for a specified <see cref="TaskbarItemInfo"/>.
        /// </summary>
        /// <param name="dependencyObject">The dependency object to assign the property on.</param>
        /// <param name="content">The content property value to assign.</param>
        public static void SetContent(DependencyObject dependencyObject, object content)
        {
            dependencyObject.SetValue(ContentProperty, content);
        }

        /// <summary>
        /// Gets the template property for a specified <see cref="TaskbarItemInfo"/>.
        /// </summary>
        /// <param name="dependencyObject">The dependency object to retrieve the property from.</param>
        /// <returns>The template property.</returns>
        public static DataTemplate GetTemplate(DependencyObject dependencyObject)
        {
            return (DataTemplate)dependencyObject.GetValue(TemplateProperty);
        }

        /// <summary>
        /// Sets the template property for a specified <see cref="TaskbarItemInfo"/>.
        /// </summary>
        /// <param name="dependencyObject">The dependency object to assign the property on.</param>
        /// <param name="template">The template property value to assign.</param>
        public static void SetTemplate(DependencyObject dependencyObject, DataTemplate template)
        {
            dependencyObject.SetValue(TemplateProperty, template);
        }

        private static void OnPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var taskbarItemInfo = dependencyObject as TaskbarItemInfo;
            var content = GetContent(taskbarItemInfo);
            var template = GetTemplate(taskbarItemInfo);

            if (template == null ||
                content == null ||
                (int.TryParse(content.ToString(), out var contentInt) && contentInt == 0))
            {
                taskbarItemInfo.Overlay = null;
                return;
            }

            const int ICON_WIDTH = 16;
            const int ICON_HEIGHT = 16;

            var bmp =
                new RenderTargetBitmap(ICON_WIDTH, ICON_HEIGHT, 96, 96, PixelFormats.Default);
            var root = new ContentControl
            {
                ContentTemplate = template,
                Content = content,
            };
            root.Arrange(new Rect(0, 0, ICON_WIDTH, ICON_HEIGHT));
            bmp.Render(root);

            taskbarItemInfo.Overlay = bmp;
        }
    }
}
