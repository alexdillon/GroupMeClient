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
    /// Adapted from https://stackoverflow.com/a/23561679.
    /// Adapted from https://stackoverflow.com/a/29500540.
    /// </remarks>
    public static class ScrollViewerEx
    {
        /// <summary>
        /// Gets a property indicating if Auto Scroll is enabled.
        /// </summary>
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached(
                "AutoScrollToEnd",
                typeof(bool),
                typeof(ScrollViewerEx),
                new PropertyMetadata(false, HookupAutoScrollToEnd));

        /// <summary>
        /// Gets a property containing the Auto Scroll handler.
        /// </summary>
        public static readonly DependencyProperty AutoScrollHandlerProperty =
            DependencyProperty.RegisterAttached(
                "AutoScrollToEndHandler",
                typeof(ScrollViewerAutoScrollToEndHandler),
                typeof(ScrollViewerEx));

        /// <summary>
        /// Gets a property indicating if Scroll To Top notifications are enabled.
        /// </summary>
        public static readonly DependencyProperty ScrollToTopProperty =
            DependencyProperty.RegisterAttached(
                "ScrollToTop",
                typeof(ICommand),
                typeof(ScrollViewerEx),
                new FrameworkPropertyMetadata(null, OnScrollToTopPropertyChanged));

        /// <summary>
        /// Gets a property indicating if IsInViewPort is enabled.
        /// </summary>
        public static readonly DependencyProperty IsInViewportProperty =
          DependencyProperty.RegisterAttached(
              "IsInViewport",
              typeof(bool),
              typeof(ScrollViewerEx));

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
            {
                instance.SetValue(AutoScrollHandlerProperty, new ScrollViewerAutoScrollToEndHandler(instance));
            }
        }

        public static ICommand GetScrollToTop(DependencyObject ob)
        {
            return (ICommand)ob.GetValue(ScrollToTopProperty);
        }

        public static void SetScrollToTop(DependencyObject ob, ICommand value)
        {
            ob.SetValue(ScrollToTopProperty, value);
        }

        public static bool GetIsInViewport(UIElement element)
        {
            return (bool)element.GetValue(IsInViewportProperty);
        }

        public static void SetIsInViewport(UIElement element, bool value)
        {
            element.SetValue(IsInViewportProperty, value);
        }

        private static void HookupAutoScrollToEnd(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ScrollViewer scrollViewer))
            {
                return;
            }

            SetAutoScrollToEnd(scrollViewer, (bool)e.NewValue);
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
                {
                    return;
                }

                command.Execute(sender);
            }
        }

        private static void OnScrollViewerUnloaded(object sender, RoutedEventArgs e)
        {
            (sender as ScrollViewer).Unloaded -= OnScrollViewerUnloaded;
            (sender as ScrollViewer).ScrollChanged -= OnScrollViewerScrollChanged;
        }
    }

    public class ScrollViewerAutoScrollToEndHandler : DependencyObject, IDisposable
    {
        private readonly ScrollViewer scrollViewer;
        private bool doScroll = false;

        public ScrollViewerAutoScrollToEndHandler(ScrollViewer scrollViewer)
        {
            this.scrollViewer = scrollViewer ?? throw new ArgumentNullException("scrollViewer");
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

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
        }

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
        #endregion
    }
}