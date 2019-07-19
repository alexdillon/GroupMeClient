using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GroupMeClient.Extensions
{
    /// <summary>
    /// <see cref="IDragDropTarget"/> enables receiving updates when data is dropped onto a control.
    /// </summary>
    /// <remarks>
    /// Adapted from https://stackoverflow.com/a/37608994.
    /// </remarks>
    public interface IDragDropTarget
    {
        /// <summary>
        /// Executed when a file has been dragged onto the target.
        /// </summary>
        /// <param name="filepaths">The file name(s) dropped.</param>
        void OnFileDrop(string[] filepaths);

        /// <summary>
        /// Executed when an image has been dragged onto the target.
        /// </summary>
        /// <param name="image">The raw image data that was dropped.</param>
        void OnImageDrop(byte[] image);
    }

    /// <summary>
    /// FileDragDropHelper.
    /// </summary>
    public class FileDragDropHelper
    {
        public static readonly DependencyProperty IsFileDragDropEnabledProperty =
              DependencyProperty.RegisterAttached("IsFileDragDropEnabled", typeof(bool), typeof(FileDragDropHelper), new PropertyMetadata(OnFileDragDropEnabled));

        public static readonly DependencyProperty FileDragDropTargetProperty =
                DependencyProperty.RegisterAttached("FileDragDropTarget", typeof(object), typeof(FileDragDropHelper), null);

        public static bool GetIsFileDragDropEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFileDragDropEnabledProperty);
        }

        public static void SetIsFileDragDropEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFileDragDropEnabledProperty, value);
        }

        public static bool GetFileDragDropTarget(DependencyObject obj)
        {
            return (bool)obj.GetValue(FileDragDropTargetProperty);
        }

        public static void SetFileDragDropTarget(DependencyObject obj, bool value)
        {
            obj.SetValue(FileDragDropTargetProperty, value);
        }

        private static void OnFileDragDropEnabled(DependencyObject d, DependencyPropertyChangedEventArgs e)
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

            var target = d.GetValue(FileDragDropTargetProperty);
            if (target is IDragDropTarget fileTarget)
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
                        var imageBytes = ImageUtils.BitmapSourceToBytes(image);

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
    }
}
