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
        /// <summary>
        /// Gets a property indicating if Auto Scroll is enabled.
        /// </summary>
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached(
                "AutoScrollToEnd",
                typeof(bool),
                typeof(ListBoxExtensions),
                new PropertyMetadata(false, HookupAutoScrollToEnd));

        /// <summary>
        /// Gets a property containing the Auto Scroll handler.
        /// </summary>
        public static readonly DependencyProperty AutoScrollHandlerProperty =
            DependencyProperty.RegisterAttached(
                "AutoScrollToEndHandler",
                typeof(ListBoxAutoScrollToEndHandler),
                typeof(ListBoxExtensions));

        /// <summary>
        /// Gets a property indicating if Scroll To Top notifications are enabled.
        /// </summary>
        public static readonly DependencyProperty ScrollToTopProperty =
            DependencyProperty.RegisterAttached(
                "ScrollToTop",
                typeof(ICommand),
                typeof(ListBoxExtensions),
                new FrameworkPropertyMetadata(null, OnScrollToTopPropertyChanged));

        /// <summary>
        /// Gets a value indicating whether Auto Scrolling in enabled.
        /// </summary>
        /// <param name="instance">The dependency object to retreive the property from.</param>
        /// <returns>A boolean indicating whether enabled.</returns>
        public static bool GetAutoScrollToEnd(ListBox instance)
        {
            return (bool)instance.GetValue(AutoScrollProperty);
        }

        /// <summary>
        /// Sets a value indicating whether Auto Scrolling is enabled.
        /// </summary>
        /// <param name="instance">The dependency object to retreive the property from.</param>
        /// <param name="value">Whether scroll to end is enabled. </param>
        public static void SetAutoScrollToEnd(ListBox instance, bool value)
        {
            if (value)
            {
                instance.Loaded += Loaded;
            }
        }

        /// <summary>
        /// Gets a value indicating whether notifications when the control is scrolled to the top are enabled.
        /// </summary>
        /// <param name="ob">The dependency object to retreive the property from.</param>
        /// <returns>A boolean indicating whether enabled.</returns>
        public static ICommand GetScrollToTop(DependencyObject ob)
        {
            return (ICommand)ob.GetValue(ScrollToTopProperty);
        }

        /// <summary>
        /// Sets a value indicating whether Scroll to Top notications are enabled.
        /// </summary>
        /// <param name="ob">The dependency object to retreive the property from.</param>
        /// <param name="value">Whether scroll to top notifications are enabled. </param>
        public static void SetScrollToTop(DependencyObject ob, ICommand value)
        {
            ob.SetValue(ScrollToTopProperty, value);
        }

        /// <summary>
        /// Retreives a child element of a control.
        /// </summary>
        /// <typeparam name="T">The type to search for.</typeparam>
        /// <param name="element">The control to search in.</param>
        /// <returns>The child control if found.</returns>
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

        private static void Loaded(object sender, EventArgs e)
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

        /// <inheritdoc/>
        public void Dispose()
        {
            // This code added to correctly implement the disposable pattern.
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