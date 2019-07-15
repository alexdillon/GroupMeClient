using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GroupMeClient.Extensions
{
    /// <summary>
    /// IFileDragDropTarget Interface
    /// </summary>
    /// <remarks>
    /// Adapted from https://stackoverflow.com/a/37608994
    /// </remarks>
    public interface IDragDropTarget
    {
        void OnFileDrop(string[] filepaths);
        void OnImageDrop(byte[] image);
    }

    /// <summary>
    /// FileDragDropHelper
    /// </summary>
    public class FileDragDropHelper
    {
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

        public static readonly DependencyProperty IsFileDragDropEnabledProperty =
                DependencyProperty.RegisterAttached("IsFileDragDropEnabled", typeof(bool), typeof(FileDragDropHelper), new PropertyMetadata(OnFileDragDropEnabled));

        public static readonly DependencyProperty FileDragDropTargetProperty =
                DependencyProperty.RegisterAttached("FileDragDropTarget", typeof(object), typeof(FileDragDropHelper), null);

        private static void OnFileDragDropEnabled(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue) return;
            if (d is Control control)
            {
                control.Drop += OnDrop;

                CommandManager.AddPreviewExecutedHandler(control, OnPreviewExecuted);
                CommandManager.AddPreviewCanExecuteHandler(control, OnPreviewCanExecute);
            }
        }

        private static void OnDrop(object _sender, DragEventArgs _dragEventArgs)
        {
            if (!(_sender is DependencyObject d)) return;

            var target = d.GetValue(FileDragDropTargetProperty);
            if (target is IDragDropTarget fileTarget)
            {
                if (_dragEventArgs.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    fileTarget.OnFileDrop((string[])_dragEventArgs.Data.GetData(DataFormats.FileDrop));
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
                    if (!(sender is DependencyObject d)) return;

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
