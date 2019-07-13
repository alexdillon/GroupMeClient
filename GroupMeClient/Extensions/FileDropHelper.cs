using System;
using System.Windows;
using System.Windows.Controls;

namespace GroupMeClient.Extensions
{
    /// <summary>
    /// IFileDragDropTarget Interface
    /// </summary>
    /// <remarks>
    /// Adapted from https://stackoverflow.com/a/37608994
    /// </remarks>
    public interface IFileDragDropTarget
    {
        void OnFileDrop(string[] filepaths);
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
            if (control != null) control.Drop += OnDrop;
        }

        private static void OnDrop(object _sender, DragEventArgs _dragEventArgs)
        {
            DependencyObject d = _sender as DependencyObject;
            if (d == null) return;
            Object target = d.GetValue(FileDragDropTargetProperty);
            IFileDragDropTarget fileTarget = target as IFileDragDropTarget;
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
    }
}
