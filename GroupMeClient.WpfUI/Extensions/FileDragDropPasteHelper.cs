using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GroupMeClient.Core.Controls;

namespace GroupMeClient.WpfUI.Extensions
{
    /// <summary>
    /// <see cref="FileDragDropPasteHelper"/> provides support for dragging and pasting files and images onto controls.
    /// </summary>
    public class FileDragDropPasteHelper
    {
        /// <summary>
        /// Gets a property indicating if FileDragDrop is supported.
        /// </summary>
        public static readonly DependencyProperty IsFileDragDropPasteEnabledProperty =
              DependencyProperty.RegisterAttached("IsFileDragDropPasteEnabled", typeof(bool), typeof(FileDragDropPasteHelper), new PropertyMetadata(OnFileDragDropPasteEnabled));

        /// <summary>
        /// Gets a property containing the File Drag Drop handler target.
        /// </summary>
        public static readonly DependencyProperty FileDragDropPasteTargetProperty =
                DependencyProperty.RegisterAttached("FileDragDropPasteTarget", typeof(object), typeof(FileDragDropPasteHelper), null);

        /// <summary>
        /// Gets a value indicating whether File Drag Drop and Enhanced Object Pasting are supported.
        /// </summary>
        /// <param name="obj">The dependency object to retreive the property from.</param>
        /// <returns>A boolean indicating whether enabled.</returns>
        public static bool GetIsFileDragDropPasteEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFileDragDropPasteEnabledProperty);
        }

        /// <summary>
        /// Sets a value indicating whether File Drag Drop and Enhanced Object Pasting are supported.
        /// </summary>
        /// <param name="obj">The dependency object to retreive the property from.</param>
        /// <param name="value">Whether drag drop and paste are supported.</param>
        public static void SetIsFileDragDropPasteEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFileDragDropPasteEnabledProperty, value);
        }

        /// <summary>
        /// Gets a value containing the Drag Drop Paste target.
        /// </summary>
        /// <param name="obj">The dependency object to retreive the property from.</param>
        /// <returns>The drag drop paste target.</returns>
        public static bool GetFileDragDropPasteTarget(DependencyObject obj)
        {
            return (bool)obj.GetValue(FileDragDropPasteTargetProperty);
        }

        /// <summary>
        /// Sets the drag drop paste target.
        /// </summary>
        /// <param name="obj">The dependency object to retreive the property from.</param>
        /// <param name="value">The target to assign.</param>
        public static void SetFileDragDropPasteTarget(DependencyObject obj, bool value)
        {
            obj.SetValue(FileDragDropPasteTargetProperty, value);
        }

        private static void OnFileDragDropPasteEnabled(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue)
            {
                return;
            }

            if (d is Control control)
            {
                control.Drop += OnDrop;

                CommandManager.AddPreviewExecutedHandler(control, OnPreviewExecuted);
                CommandManager.AddPreviewCanExecuteHandler(control, OnPreviewCanExecute);
            }
        }

        private static void OnDrop(object sender, DragEventArgs dragEventArgs)
        {
            if (!(sender is DependencyObject d))
            {
                return;
            }

            var target = d.GetValue(FileDragDropPasteTargetProperty);
            if (target is IDragDropPasteTarget fileTarget)
            {
                if (dragEventArgs.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    fileTarget.OnFileDrop((string[])dragEventArgs.Data.GetData(DataFormats.FileDrop));
                }
            }
            else
            {
                throw new Exception("FileDragDropTarget object must be of type IFileDragDropTarget");
            }
        }

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
                byte[] imageBytes = null;

                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    // If Shift key is held, paste as plain-text, not an image.
                    e.Handled = false;
                    return;
                }

                // First check to see if "PNG" data is on the clipboard to preserve transparency
                if (Clipboard.ContainsData("PNG"))
                {
                    var pngData = Clipboard.GetData("PNG");
                    if (pngData is MemoryStream pngDataMs)
                    {
                        imageBytes = pngDataMs.ToArray();
                    }
                    else if (pngData is byte[] bytes)
                    {
                        imageBytes = bytes;
                    }
                }
                else if (Clipboard.ContainsData("DeviceIndependentBitmap"))
                {
                    var dibData = Clipboard.GetData("DeviceIndependentBitmap");
                    if (dibData is MemoryStream dibDataMs)
                    {
                        var image = Utilities.ImageUtils.ImageFromClipboardDib(dibDataMs);
                        imageBytes = Utilities.ImageUtils.BitmapSourceToBytes(image as BitmapSource);
                    }
                }
                else if (Clipboard.ContainsImage())
                {
                    var image = Clipboard.GetImage();
                    imageBytes = Utilities.ImageUtils.BitmapSourceToBytes(image);
                }

                if (!(sender is DependencyObject d))
                {
                    return;
                }

                var target = d.GetValue(FileDragDropPasteTargetProperty);
                if (!(target is IDragDropPasteTarget fileTarget))
                {
                    throw new Exception("FileDragDropTarget object must be of type IFileDragDropTarget");
                }

                if (imageBytes != null)
                {
                    // Handle pasting of all image data
                    fileTarget.OnImageDrop(imageBytes);
                    e.Handled = true;
                }
                else if (Clipboard.ContainsFileDropList())
                {
                    // Handle pasting of all file data
                    fileTarget.OnFileDrop(Clipboard.GetFileDropList().Cast<string>().ToArray());
                }
            }
        }
    }
}
