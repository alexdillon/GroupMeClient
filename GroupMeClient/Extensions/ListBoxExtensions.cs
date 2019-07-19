using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GroupMeClient.Extensions
{
    /// <summary>
    /// Provides MVVM support for scrolling to the bottom of a list or allowing
    /// a user to lock the current scroll position.
    /// </summary>
    /// <remarks>
    /// Adapted from https://stackoverflow.com/a/23561679.
    /// </remarks>
    public static class ListBoxExtensions
    {
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached(
                "AutoScrollToEnd",
                typeof(bool),
                typeof(ListBoxExtensions),
                new PropertyMetadata(false, HookupAutoScrollToEnd));

        public static readonly DependencyProperty AutoScrollHandlerProperty =
            DependencyProperty.RegisterAttached(
                "AutoScrollToEndHandler",
                typeof(ListBoxAutoScrollToEndHandler),
                typeof(ListBoxExtensions));

        public static readonly DependencyProperty ScrollToTopProperty =
            DependencyProperty.RegisterAttached(
                "ScrollToTop",
                typeof(ICommand),
                typeof(ListBoxExtensions),
                new FrameworkPropertyMetadata(null, OnScrollToTopPropertyChanged));

        public static void Loaded(object sender, EventArgs e)
        {
            var instance = sender as ListBox;

            var oldHandler = (ListBoxAutoScrollToEndHandler)instance.GetValue(AutoScrollHandlerProperty);
            if (oldHandler != null)
            {
                oldHandler.Dispose();
                instance.SetValue(AutoScrollHandlerProperty, null);
            }

            instance.SetValue(AutoScrollProperty, true);
            instance.SetValue(AutoScrollHandlerProperty, new ListBoxAutoScrollToEndHandler(instance));
        }

        private static void HookupAutoScrollToEnd(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ListBox listBox))
            {
                return;
            }

            SetAutoScrollToEnd(listBox, (bool)e.NewValue);
        }

        public static bool GetAutoScrollToEnd(ListBox instance)
        {
            return (bool)instance.GetValue(AutoScrollProperty);
        }

        public static void SetAutoScrollToEnd(ListBox instance, bool value)
        {
            instance.Loaded += Loaded;
        }

        public static ICommand GetScrollToTop(DependencyObject ob)
        {
            return (ICommand)ob.GetValue(ScrollToTopProperty);
        }

        public static void SetScrollToTop(DependencyObject ob, ICommand value)
        {
            ob.SetValue(ScrollToTopProperty, value);
        }

        private static void OnScrollToTopPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var listBox = obj as ListBox;

            listBox.Loaded += OnListBoxLoaded;
        }

        private static void OnListBoxLoaded(object sender, RoutedEventArgs e)
        {
            (sender as ListBox).Loaded -= OnListBoxLoaded;

            var scrollViewer = FindSimpleVisualChild<ScrollViewer>(sender as ListBox);

            scrollViewer.ScrollChanged += OnListBoxScrollChanged;
        }

        private static void OnListBoxScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            var listBox = scrollViewer.TemplatedParent as ListBox;

            // Check to see if scrolled to top
            if (scrollViewer.VerticalOffset == 0)
            {
                var command = GetScrollToTop(listBox);
                if (command == null || !command.CanExecute(null))
                {
                    return;
                }

                command.Execute(scrollViewer);
            }
        }

        public static T FindSimpleVisualChild<T>(DependencyObject element)
            where T : class
        {
            while (element != null)
            {
                if (element is T)
                {
                    return element as T;
                }

                element = VisualTreeHelper.GetChild(element, 0);
            }

            return null;
        }
    }

    public class ListBoxAutoScrollToEndHandler : DependencyObject, IDisposable
    {
        private readonly ScrollViewer scrollViewer;
        private bool doScroll = true;

        public ListBoxAutoScrollToEndHandler(ListBox listBox)
        {
            this.scrollViewer = ListBoxExtensions.FindSimpleVisualChild<ScrollViewer>(listBox);
            this.scrollViewer.ScrollToEnd();
            this.scrollViewer.ScrollChanged += this.ScrollChanged;
        }

        private void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // User scroll event : set or unset autoscroll mode
            if (e.ExtentHeightChange == 0)
            {
                this.doScroll = this.scrollViewer.VerticalOffset == this.scrollViewer.ScrollableHeight;
            }

            // Content scroll event : autoscroll eventually
            if (this.doScroll && e.ExtentHeightChange != 0)
            {
                this.scrollViewer.ScrollToVerticalOffset(this.scrollViewer.ExtentHeight);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.scrollViewer.ScrollChanged -= this.ScrollChanged;
                }

                this.disposedValue = true;
            }
        }

        public void Dispose()
        {
            // This code added to correctly implement the disposable pattern.
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
        }
        #endregion
    }
}