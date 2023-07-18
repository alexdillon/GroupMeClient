using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using GroupMeClient.Core.Controls;

namespace GroupMeClient.AvaloniaUI.Extensions
{
    /// <summary>
    /// FileDragDropHelper.
    /// </summary>
    public class FileDragDropHelper
    {
        /// <summary>
        /// Gets a property indicating if FileDragDrop is supported.
        /// </summary>
        public static readonly AvaloniaProperty<bool> IsFileDragDropEnabledProperty =
            AvaloniaProperty.RegisterAttached<Control, bool>(
                "IsFileDragDropEnabled",
                typeof(FileDragDropHelper),
                defaultValue: false);

        /// <summary>
        /// Gets a property containing the File Drag Drop handler target.
        /// </summary>
        public static readonly AvaloniaProperty<IDragDropPasteTarget> FileDragDropTargetProperty =
            AvaloniaProperty.RegisterAttached<Control, IDragDropPasteTarget>(
                "FileDragDropTarget",
                typeof(FileDragDropHelper),
                defaultValue: null);

        static FileDragDropHelper()
        {
            IsFileDragDropEnabledProperty.Changed.Subscribe(x => HandleIsFileDropEnabledChanged(x.Sender, x.NewValue.Value));
        }

        /// <summary>
        /// Gets a value indicating whether File Drag Drop is supported.
        /// </summary>
        /// <param name="obj">The dependency object to retreive the property from.</param>
        /// <returns>A boolean indicating whether enabled.</returns>
        public static bool GetIsFileDragDropEnabled(AvaloniaObject obj)
        {
            return (bool)obj.GetValue(IsFileDragDropEnabledProperty);
        }

        /// <summary>
        /// Sets a value indicating whether File Drag Drop is supported.
        /// </summary>
        /// <param name="obj">The dependency object to retreive the property from.</param>
        /// <param name="value">Whether drag drop is supported.</param>
        public static void SetIsFileDragDropEnabled(AvaloniaObject obj, bool value)
        {
            obj.SetValue(IsFileDragDropEnabledProperty, value);
        }

        /// <summary>
        /// Gets a value containing the Drag Drop target.
        /// </summary>
        /// <param name="obj">The dependency object to retreive the property from.</param>
        /// <returns>The drag drop target.</returns>
        public static bool GetFileDragDropTarget(AvaloniaObject obj)
        {
            return (bool)obj.GetValue(FileDragDropTargetProperty);
        }

        /// <summary>
        /// Sets the drag drop target.
        /// </summary>
        /// <param name="obj">The dependency object to retreive the property from.</param>
        /// <param name="value">The target to assign.</param>
        public static void SetFileDragDropTarget(AvaloniaObject obj, bool value)
        {
            obj.SetValue(FileDragDropTargetProperty, value);
        }

        /// <summary>
        /// <see cref="CommandProperty"/> changed event handler.
        /// </summary>
        private static void HandleIsFileDropEnabledChanged(AvaloniaObject element, bool isEnabled)
        {
            if (element is Control control)
            {
                if (isEnabled)
                {
                    // Enable handler
                    control.AddHandler(DragDrop.DropEvent, OnDrop);
                }
                else
                {
                    // Remove handler
                    control.RemoveHandler(DragDrop.DropEvent, OnDrop);
                }
            }

            /*CommandManager.AddPreviewExecutedHandler(control, OnPreviewExecuted);
              CommandManager.AddPreviewCanExecuteHandler(control, OnPreviewCanExecute);*/
        }

        private static void OnDrop(object sender, DragEventArgs dragEventArgs)
        {
            if (!(sender is AvaloniaObject d))
            {
                return;
            }

            var target = d.GetValue(FileDragDropTargetProperty);
            if (target is IDragDropPasteTarget fileTarget)
            {
                if (dragEventArgs.Data.Contains(DataFormats.Files))
                {
                    fileTarget.OnFileDrop(dragEventArgs.Data
                        .GetFiles()
                        .Select(f => f.TryGetLocalPath())
                        .Where(p => !string.IsNullOrEmpty(p))
                        .ToArray());
                }
            }
            else
            {
                throw new Exception($"FileDragDropTarget object must be of type {nameof(IDragDropPasteTarget)}");
            }
        }

        // TODO: Support pasting images into text boxes.
        /*
        private static void OnPreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                e.CanExecute = true;
                e.Handled = true;
            }
        }

        private static void OnPreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                if (Clipboard.ContainsImage())
                {
                    if (!(sender is DependencyObject d))
                    {
                        return;
                    }

                    var target = d.GetValue(FileDragDropTargetProperty);

                    if (target is IDragDropTarget fileTarget)
                    {
                        var image = Clipboard.GetImage();
                        var imageBytes = Utilities.ImageUtils.BitmapSourceToBytes(image);

                        fileTarget.OnImageDrop(imageBytes);
                    }
                    else
                    {
                        throw new Exception("FileDragDropTarget object must be of type IFileDragDropTarget");
                    }

                    e.Handled = true;
                }
            }
        }
        */
    }
}
