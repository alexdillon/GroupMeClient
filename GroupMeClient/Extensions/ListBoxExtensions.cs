using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace GroupMeClient.WpfUI.Extensions
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
                new FrameworkPropertyMetadata(null, OnScrollToTopBottomPropertyChanged));

        /// <summary>
        /// Gets a property indicating if Scroll To Bottom notifications are enabled.
        /// </summary>
        public static readonly DependencyProperty ScrollToBottomProperty =
            DependencyProperty.RegisterAttached(
                "ScrollToBottom",
                typeof(ICommand),
                typeof(ListBoxExtensions),
                new FrameworkPropertyMetadata(null, OnScrollToTopBottomPropertyChanged));

        /// <summary>
        /// Gets a property indicating if Auto Scroll is enabled.
        /// </summary>
        public static readonly DependencyProperty TopLoadingSnap =
            DependencyProperty.RegisterAttached(
                "TopLoadingSnap",
                typeof(int),
                typeof(ListBoxExtensions),
                new PropertyMetadata(0));

        /// <summary>
        /// Gets a property indicating if Auto Scroll is enabled.
        /// </summary>
        public static readonly DependencyProperty BottomLoadingSnap =
            DependencyProperty.RegisterAttached(
                "BottomLoadingSnap",
                typeof(int),
                typeof(ListBoxExtensions),
                new PropertyMetadata(0));

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
        /// Gets a value indicating whether notifications when the control is scrolled to the bottom are enabled.
        /// </summary>
        /// <param name="ob">The dependency object to retreive the property from.</param>
        /// <returns>A boolean indicating whether enabled.</returns>
        public static ICommand GetScrollToBottom(DependencyObject ob)
        {
            return (ICommand)ob.GetValue(ScrollToBottomProperty);
        }

        /// <summary>
        /// Sets a value indicating whether Scroll to Bottom notications are enabled.
        /// </summary>
        /// <param name="ob">The dependency object to retreive the property from.</param>
        /// <param name="value">Whether scroll to top notifications are enabled. </param>
        public static void SetScrollToBottom(DependencyObject ob, ICommand value)
        {
            ob.SetValue(ScrollToBottomProperty, value);
        }

        /// <summary>
        /// Gets the snap position when items are being loaded at the top.
        /// </summary>
        /// <param name="instance">The dependency object to retreive the property from.</param>
        /// <returns>The snap position when items are being loaded at the top.</returns>
        public static int GetTopLoadingSnap(ListBox instance)
        {
            return (int)instance.GetValue(TopLoadingSnap);
        }

        /// <summary>
        /// Sets the snap position when items are being loaded at the top.
        /// </summary>
        /// <param name="ob">The dependency object to apply the property to.</param>
        /// <param name="value">The snap position when items are being loaded at the top.</param>
        public static void SetTopLoadingSnap(DependencyObject ob, int value)
        {
            ob.SetValue(TopLoadingSnap, value);
        }

        /// <summary>
        /// Gets the snap position when items are being loaded at the Bottom.
        /// </summary>
        /// <param name="instance">The dependency object to retreive the property from.</param>
        /// <returns>The snap position when items are being loaded at the Bottom.</returns>
        public static int GetBottomLoadingSnap(ListBox instance)
        {
            return (int)instance.GetValue(BottomLoadingSnap);
        }

        /// <summary>
        /// Sets the snap position when items are being loaded at the Bottom.
        /// </summary>
        /// <param name="ob">The dependency object to apply the property to.</param>
        /// <param name="value">The snap position when items are being loaded at the Bottom.</param>
        public static void SetBottomLoadingSnap(DependencyObject ob, int value)
        {
            ob.SetValue(BottomLoadingSnap, value);
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

        private static void OnScrollToTopBottomPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var listBox = obj as ListBox;

            listBox.Loaded -= OnListBoxLoaded;
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

            var topSnap = GetTopLoadingSnap(listBox);
            var bottomSnap = GetBottomLoadingSnap(listBox);

            if (bottomSnap > 0)
            {
                scrollViewer.UpdateLayout();
                scrollViewer.ScrollToVerticalOffset(bottomSnap);
                scrollViewer.UpdateLayout();
                SetBottomLoadingSnap(listBox, 0);
                return;
            }
            else if (topSnap != 0)
            {
                // Calculate the offset where the last message the user was looking at is
                // Scroll back to there so new messages appear on top, above screen
                scrollViewer.UpdateLayout();

                double newHeight = scrollViewer?.ExtentHeight ?? 0.0;
                double difference = newHeight - topSnap;

                scrollViewer.ScrollToVerticalOffset(difference);
                SetTopLoadingSnap(listBox, 0);
                return;
            }

            // Check to see if scrolled to top
            if (scrollViewer.VerticalOffset == 0)
            {
                var command = GetScrollToTop(listBox);
                if (command != null && command.CanExecute(null))
                {
                    // Save the original position to allow us to return to it when the UI update is completed
                    double originalHeight = scrollViewer?.ExtentHeight ?? 0.0;
                    double originalOffset = scrollViewer?.VerticalOffset ?? 0.0;

                    // Run the At-Top handler
                    scrollViewer.CanContentScroll = false;
                    SetTopLoadingSnap(listBox, (int)originalHeight);
                    command.Execute(scrollViewer);
                    scrollViewer.CanContentScroll = true;

                    //// Restore the original position after the insert has been completed
                    //if (originalHeight != 0)
                    //{
                    //    // Calculate the offset where the last message the user was looking at is
                    //    // Scroll back to there so new messages appear on top, above screen
                    //    scrollViewer.UpdateLayout();
                    //    double newHeight = scrollViewer?.ExtentHeight ?? 0.0;
                    //    double difference = newHeight - originalHeight;

                    //    scrollViewer.ScrollToVerticalOffset(difference);
                    //}
                }
            }
            else if ((int)scrollViewer.VerticalOffset == (int)scrollViewer.ScrollableHeight)
            {
                if (e.VerticalChange < 1000)
                {
                    var command = GetScrollToBottom(listBox);
                    if (command != null && command.CanExecute(null))
                    {
                        // Save the original position to allow us to return to it when the UI update is completed
                        double originalHeight = scrollViewer?.ExtentHeight ?? 0.0;
                        double originalOffset = scrollViewer?.VerticalOffset ?? 0.0;

                        // Run the At-Bottom handler
                        scrollViewer.CanContentScroll = false;
                        SetBottomLoadingSnap(listBox, (int)originalOffset);
                        command.Execute(scrollViewer);
                        scrollViewer.CanContentScroll = true;
                        //scrollViewer.ScrollToVerticalOffset(originalOffset - 1);
                        //SetIsScrollFrozen(listBox, false);
                    }
                }
            }
        }

        /// <summary>
        /// Handler class to maintain user scroll information for a scrollable <see cref="ListBox"/>.
        /// </summary>
        public class ListBoxAutoScrollToEndHandler : DependencyObject, IDisposable
        {
            private readonly ScrollViewer scrollViewer;
            private bool doScroll = true;
            private bool disposedValue = false; // To detect redundant calls, for IDisposable

            /// <summary>
            /// Initializes a new instance of the <see cref="ListBoxAutoScrollToEndHandler"/> class.
            /// </summary>
            /// <param name="listBox">The ListBox to bind to.</param>
            public ListBoxAutoScrollToEndHandler(ListBox listBox)
            {
                this.scrollViewer = FindSimpleVisualChild<ScrollViewer>(listBox);
                this.scrollViewer.ScrollToEnd();
                this.scrollViewer.ScrollChanged += this.ScrollChanged;
                this.scrollViewer.SizeChanged += this.SizeChanged;

                // Use a One-Shot event handler to force the list to snap to the bottom when generation is finished
                // This can safely ignore doScroll - it tends to be extraneously set to false by out-of-order scroll
                // events before the control has actually finished loading any content.
                EventHandler eventHandler = null;
                eventHandler = (s, e) =>
                    {
                        if (listBox.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                        {
                            var scrollViewer = FindSimpleVisualChild<ScrollViewer>(listBox);
                            scrollViewer.ScrollToBottom();
                            listBox.ItemContainerGenerator.StatusChanged -= eventHandler;
                        }
                    };
                listBox.ItemContainerGenerator.StatusChanged += eventHandler;
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                // This code added to correctly implement the disposable pattern.
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                this.Dispose(true);
            }

            private void ScrollChanged(object sender, ScrollChangedEventArgs e)
            {
                // User scroll event : set or unset autoscroll mode
                if (e.ExtentHeightChange == 0)
                {
                    // Trucate to integers before comparing to prevent round-off errors when high-DPI scaling is used.
                    this.doScroll = (int)this.scrollViewer.VerticalOffset == (int)this.scrollViewer.ScrollableHeight;
                }

                // Content scroll event : autoscroll eventually
                if (this.doScroll && e.ExtentHeightChange != 0)
                {
                    this.scrollViewer.ScrollToVerticalOffset(this.scrollViewer.ExtentHeight);
                }
            }

            private void SizeChanged(object sender, SizeChangedEventArgs e)
            {
                // If autoscroll is enabled and scrolled to the bottom, keep the bottom tracked if the size changes.
                if (this.doScroll)
                {
                    this.scrollViewer.ScrollToVerticalOffset(this.scrollViewer.ExtentHeight);
                }
            }

            private void Dispose(bool disposing)
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
        }
    }
}