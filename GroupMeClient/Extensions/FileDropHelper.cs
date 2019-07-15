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
            var control = d as Control;
            if (control != null)
            {
                control.Drop += OnDrop;

                CommandManager.AddPreviewExecutedHandler(control, onPreviewExecuted);
                CommandManager.AddPreviewCanExecuteHandler(control, onPreviewCanExecute);
            }
        }

        private static void OnDrop(object _sender, DragEventArgs _dragEventArgs)
        {
            DependencyObject d = _sender as DependencyObject;
            if (d == null) return;
            Object target = d.GetValue(FileDragDropTargetProperty);
            IDragDropTarget fileTarget = target as IDragDropTarget;
            if (fileTarget != null)
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

        private static void onPreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                e.CanExecute = true;
                e.Handled = true;
            }
        }

        private static void onPreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                if (Clipboard.ContainsImage())
                {
                    DependencyObject d = sender as DependencyObject;
                    if (d == null) return;
                    Object target = d.GetValue(FileDragDropTargetProperty);
                    IDragDropTarget fileTarget = target as IDragDropTarget;

                    if (fileTarget != null)
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
