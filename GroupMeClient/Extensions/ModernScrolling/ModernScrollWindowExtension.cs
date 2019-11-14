using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using GroupMeClient.Utilities.DirectManipulation;

namespace GroupMeClient.Extensions.ModernScrolling
{
    public static class ModernScrollWindowExtension
    {
        /// <summary>
        /// Gets a dependency property indicating if Direct Manipulation Scrolling is enabled.
        /// </summary>
        public static readonly DependencyProperty DirectManipulationEnabledProperty =
            DependencyProperty.RegisterAttached(
                "EnableDirectManipulation",
                typeof(bool),
                typeof(ModernScrollWindowExtension),
                new PropertyMetadata(false, HookupEnableDirectManipulation));

        /// <summary>
        /// Gets a property containing the Direct Manipulation handler.
        /// </summary>
        public static readonly DependencyProperty DirectManipulationHandlerProperty =
            DependencyProperty.RegisterAttached(
                "DirectManipulationHandler",
                typeof(DirectManipulationHandler),
                typeof(ModernScrollWindowExtension));

        /// <summary>
        /// Gets a value indicating whether Direct Manipulation Scrolling is enabled.
        /// </summary>
        /// <param name="instance">The dependency object to retreive the property from.</param>
        /// <returns>A boolean indicating whether enabled.</returns>
        public static bool GetEnableDirectManipulation(Window instance)
        {
            return (bool)instance.GetValue(DirectManipulationEnabledProperty);
        }

        /// <summary>
        /// Sets a value indicating whether Direct Manipulation Scrolling is enabled.
        /// </summary>
        /// <param name="instance">The dependency object to retreive the property from.</param>
        /// <param name="value">Whether Direct Manipulation Scrolling is enabled. </param>
        public static void SetEnableDirectManipulation(Window instance, bool value)
        {
            if (value)
            {
                instance.Loaded += Loaded;
            }
        }

        private static void HookupEnableDirectManipulation(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Window window))
            {
                return;
            }

            SetEnableDirectManipulation(window, (bool)e.NewValue);
        }

        private static void Loaded(object sender, EventArgs e)
        {
            var instance = sender as Window;

            var oldHandler = (DirectManipulationHandler)instance.GetValue(DirectManipulationHandlerProperty);
            if (oldHandler != null)
            {
                oldHandler.Dispose();
                instance.SetValue(DirectManipulationHandlerProperty, null);
            }

            instance.SetValue(DirectManipulationEnabledProperty, true);
            instance.SetValue(DirectManipulationHandlerProperty, new DirectManipulationHandler(instance));
        }

        /// <summary>
        /// Handler class to maintain direct manipulation scroll information for a scrollable <see cref="Window"/>.
        /// </summary>
        public class DirectManipulationHandler : DependencyObject, IDisposable
        {
            private readonly Window window;
            private bool disposedValue = false; // To detect redundant calls, for IDisposable
            private readonly PointerBasedManipulationHandler manipulationHandler;

            /// <summary>
            /// Initializes a new instance of the <see cref="DirectManipulationHandler"/> class.
            /// </summary>
            /// <param name="instance">The <see cref="Window"/> to bind to.</param>
            public DirectManipulationHandler(Window instance)
            {
                this.window = instance;
               // this.window.Initialized += this.ScrollViewer_Initialized;
                this.manipulationHandler = new PointerBasedManipulationHandler();
                this.Window_Initialized(this, null);
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                // This code added to correctly implement the disposable pattern.
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                this.Dispose(true);
            }

            private void Window_Initialized(object sender, EventArgs e)
            {
                HwndSource source = (HwndSource)HwndSource.FromVisual(this.window);

                this.manipulationHandler.HwndSource = source;
                this.manipulationHandler.TranslationUpdated += this.ManipulationHandler_TranslationUpdated;

                // PresentationSource.AddSourceChangedHandler(this.window, this.HandleSourceUpdated);

                var group = new TransformGroup();
                var st = new ScaleTransform();
                group.Children.Add(st);
                var tt = new TranslateTransform();
                group.Children.Add(tt);
                this.window.RenderTransform = group;
                this.window.RenderTransformOrigin = new Point(0.0, 0.0);
            }

            private TranslateTransform GetTranslateTransform(UIElement element)
            {
                return (TranslateTransform)(element.RenderTransform as TransformGroup)
                  .Children.First(tr => tr is TranslateTransform);
            }

            private void HandleSourceUpdated(object sender, SourceChangedEventArgs e)
            {
                if (this.manipulationHandler != null && e.NewSource is System.Windows.Interop.HwndSource newHwnd)
                {
                    this.manipulationHandler.HwndSource = newHwnd;
                    this.manipulationHandler.TranslationUpdated += this.ManipulationHandler_TranslationUpdated;
                }
            }

            private static T FindSimpleVisualParent<T>(DependencyObject element)
              where T : class
            {
                while (element != null)
                {
                    if (element is T)
                    {
                        return element as T;
                    }

                    element = VisualTreeHelper.GetParent(element);
                }

                return null;
            }

            private void ManipulationHandler_TranslationUpdated(float arg1, float arg2)
            {
                var hoveredElement = this.window.InputHitTest(InputManager.Current.PrimaryMouseDevice.GetPosition(this.window));
                var scrollableParent = FindSimpleVisualParent<ScrollViewer>(hoveredElement as DependencyObject);

                if (hoveredElement != null)
                {
                    var scrollViewer = scrollableParent as ScrollViewer;
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + arg1);
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + arg2);
                    return;
                }

                IInputElement focusedControl = FocusManager.GetFocusedElement(this.window);

                MouseDevice mouseDev = InputManager.Current.PrimaryMouseDevice;
                var raisedEvent = new MouseWheelEventArgs(
                    mouseDev,
                    Environment.TickCount,
                    (int)arg2);
                raisedEvent.RoutedEvent = UIElement.MouseWheelEvent;
                raisedEvent.Source = this;

                focusedControl.RaiseEvent(raisedEvent);

                //this.window.ScrollToHorizontalOffset(this.window.HorizontalOffset + arg1);
                //this.window.ScrollToVerticalOffset(this.window.VerticalOffset + arg2);
               // var tt = this.GetTranslateTransform(this.window);

                // tt.X += arg1;
                // tt.Y += arg2;
            }

            private void Dispose(bool disposing)
            {
            }
        }
    }
}
