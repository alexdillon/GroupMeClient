using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GroupMeClient.Extensions
{
    /// <summary>
    /// Provides MVVM support for scrolling to the bottom of a list or allowing
    /// a user to lock the current scroll position.
    /// </summary>
    /// <remarks>
    /// Adapted from https://stackoverflow.com/a/23561679
    /// Adapted from https://stackoverflow.com/a/29500540
    /// </remarks>
    public static class ScrollViewerEx
    {
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached("AutoScrollToEnd",
                typeof(bool), typeof(ScrollViewerEx),
                new PropertyMetadata(false, HookupAutoScrollToEnd));

        public static readonly DependencyProperty AutoScrollHandlerProperty =
            DependencyProperty.RegisterAttached("AutoScrollToEndHandler",
                typeof(ScrollViewerAutoScrollToEndHandler), typeof(ScrollViewerEx));

        private static void HookupAutoScrollToEnd(DependencyObject d,
                DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ScrollViewer scrollViewer)) return;

            SetAutoScrollToEnd(scrollViewer, (bool)e.NewValue);
        }

        public static bool GetAutoScrollToEnd(ScrollViewer instance)
        {
            return (bool)instance.GetValue(AutoScrollProperty);
        }

        public static void SetAutoScrollToEnd(ScrollViewer instance, bool value)
        {
            var oldHandler = (ScrollViewerAutoScrollToEndHandler)instance.GetValue(AutoScrollHandlerProperty);
            if (oldHandler != null)
            {
                oldHandler.Dispose();
                instance.SetValue(AutoScrollHandlerProperty, null);
            }
            instance.SetValue(AutoScrollProperty, value);
            if (value)
                instance.SetValue(AutoScrollHandlerProperty, new ScrollViewerAutoScrollToEndHandler(instance));
        }

        public static readonly DependencyProperty ScrollToTopProperty =
            DependencyProperty.RegisterAttached("ScrollToTop", 
                typeof(ICommand), 
                typeof(ScrollViewerEx), 
                new FrameworkPropertyMetadata(null, OnScrollToTopPropertyChanged));

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
            var scrollViewer = obj as ScrollViewer;

            scrollViewer.Loaded += OnScrollViewerLoaded;

        }

        private static void OnScrollViewerLoaded(object sender, RoutedEventArgs e)
        {
            (sender as ScrollViewer).Loaded -= OnScrollViewerLoaded;

            (sender as ScrollViewer).Unloaded += OnScrollViewerUnloaded;
            (sender as ScrollViewer).ScrollChanged += OnScrollViewerScrollChanged;
        }

        private static void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;

            // Check to see if scrolled to top
            if (scrollViewer.VerticalOffset == 0)
            {
                var command = GetScrollToTop(sender as ScrollViewer);
                if (command == null || !command.CanExecute(null))
                    return;

                command.Execute(sender);
            }
        }

        private static void OnScrollViewerUnloaded(object sender, RoutedEventArgs e)
        {
            (sender as ScrollViewer).Unloaded -= OnScrollViewerUnloaded;
            (sender as ScrollViewer).ScrollChanged -= OnScrollViewerScrollChanged;
        }

        public static readonly DependencyProperty IsInViewportProperty =
            DependencyProperty.RegisterAttached("IsInViewport", typeof(bool), typeof(ScrollViewerEx));

        public static bool GetIsInViewport(UIElement element)
        {
            return (bool)element.GetValue(IsInViewportProperty);
        }

        public static void SetIsInViewport(UIElement element, bool value)
        {
            element.SetValue(IsInViewportProperty, value);
        }
    }

    public class ScrollViewerAutoScrollToEndHandler : DependencyObject, IDisposable
    {
        readonly ScrollViewer m_scrollViewer;
        bool m_doScroll = false;

        public ScrollViewerAutoScrollToEndHandler(ScrollViewer scrollViewer)
        {
            m_scrollViewer = scrollViewer ?? throw new ArgumentNullException("scrollViewer");
            m_scrollViewer.ScrollToEnd();
            m_scrollViewer.ScrollChanged += ScrollChanged;
        }

        private void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // User scroll event : set or unset autoscroll mode
            if (e.ExtentHeightChange == 0)
            { m_doScroll = m_scrollViewer.VerticalOffset == m_scrollViewer.ScrollableHeight; }

            // Content scroll event : autoscroll eventually
            if (m_doScroll && e.ExtentHeightChange != 0)
            { m_scrollViewer.ScrollToVerticalOffset(m_scrollViewer.ExtentHeight); }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    m_scrollViewer.ScrollChanged -= ScrollChanged;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}