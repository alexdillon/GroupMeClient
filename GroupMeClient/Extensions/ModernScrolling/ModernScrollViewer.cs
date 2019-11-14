//using System;
//using System.Linq;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Interop;
//using System.Windows.Media;
//using GroupMeClient.Utilities.DirectManipulation;

//namespace GroupMeClient.Extensions.ModernScrolling
//{
//    public static class ModernScrollViewerExtension
//    {
//        /// <summary>
//        /// Gets a dependency property indicating if Direct Manipulation Scrolling is enabled.
//        /// </summary>
//        public static readonly DependencyProperty DirectManipulationEnabledProperty =
//            DependencyProperty.RegisterAttached(
//                "EnableDirectManipulation",
//                typeof(bool),
//                typeof(ModernScrollViewerExtension),
//                new PropertyMetadata(false, HookupEnableDirectManipulation));

//        /// <summary>
//        /// Gets a property containing the Direct Manipulation handler.
//        /// </summary>
//        public static readonly DependencyProperty DirectManipulationHandlerProperty =
//            DependencyProperty.RegisterAttached(
//                "DirectManipulationHandler",
//                typeof(DirectManipulationHandler),
//                typeof(ModernScrollViewerExtension));

//        /// <summary>
//        /// Gets a value indicating whether Direct Manipulation Scrolling is enabled.
//        /// </summary>
//        /// <param name="instance">The dependency object to retreive the property from.</param>
//        /// <returns>A boolean indicating whether enabled.</returns>
//        public static bool GetEnableDirectManipulation(ScrollViewer instance)
//        {
//            return (bool)instance.GetValue(DirectManipulationEnabledProperty);
//        }

//        /// <summary>
//        /// Sets a value indicating whether Direct Manipulation Scrolling is enabled.
//        /// </summary>
//        /// <param name="instance">The dependency object to retreive the property from.</param>
//        /// <param name="value">Whether Direct Manipulation Scrolling is enabled. </param>
//        public static void SetEnableDirectManipulation(ScrollViewer instance, bool value)
//        {
//            if (value)
//            {
//                instance.Loaded += Loaded;
//            }
//        }

//        private static void HookupEnableDirectManipulation(DependencyObject d, DependencyPropertyChangedEventArgs e)
//        {
//            if (!(d is ScrollViewer scrollViewer))
//            {
//                return;
//            }

//            SetEnableDirectManipulation(scrollViewer, (bool)e.NewValue);
//        }

//        private static void Loaded(object sender, EventArgs e)
//        {
//            var instance = sender as ScrollViewer;

//            var oldHandler = (DirectManipulationHandler)instance.GetValue(DirectManipulationHandlerProperty);
//            if (oldHandler != null)
//            {
//                oldHandler.Dispose();
//                instance.SetValue(DirectManipulationHandlerProperty, null);
//            }

//            instance.SetValue(DirectManipulationEnabledProperty, true);
//            instance.SetValue(DirectManipulationHandlerProperty, new DirectManipulationHandler(instance));
//        }

//        /// <summary>
//        /// Handler class to maintain direct manipulation scroll information for a scrollable <see cref="ScrollViewer"/>.
//        /// </summary>
//        public class DirectManipulationHandler : DependencyObject, IDisposable
//        {
//            private readonly ScrollViewer scrollViewer;
//            private bool disposedValue = false; // To detect redundant calls, for IDisposable
//            private readonly PointerBasedManipulationHandler manipulationHandler;

//            /// <summary>
//            /// Initializes a new instance of the <see cref="DirectManipulationHandler"/> class.
//            /// </summary>
//            /// <param name="instance">The <see cref="ScrollViewer"/> to bind to.</param>
//            public DirectManipulationHandler(ScrollViewer instance)
//            {
//                this.scrollViewer = instance;
//               // this.scrollViewer.Initialized += this.ScrollViewer_Initialized;
//                this.manipulationHandler = new PointerBasedManipulationHandler();
//                this.ScrollViewer_Initialized(this, null);
//            }

//            /// <inheritdoc/>
//            public void Dispose()
//            {
//                // This code added to correctly implement the disposable pattern.
//                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
//                this.Dispose(true);
//            }

//            private void ScrollViewer_Initialized(object sender, EventArgs e)
//            {
//                HwndSource source = (HwndSource)HwndSource.FromVisual(this.scrollViewer);

//                this.manipulationHandler.HwndSource = source;
//                this.manipulationHandler.TranslationUpdated += this.ManipulationHandler_TranslationUpdated;

//                // PresentationSource.AddSourceChangedHandler(this.scrollViewer, this.HandleSourceUpdated);

//                var group = new TransformGroup();
//                var st = new ScaleTransform();
//                group.Children.Add(st);
//                var tt = new TranslateTransform();
//                group.Children.Add(tt);
//                this.scrollViewer.RenderTransform = group;
//                this.scrollViewer.RenderTransformOrigin = new Point(0.0, 0.0);
//            }

//            private TranslateTransform GetTranslateTransform(UIElement element)
//            {
//                return (TranslateTransform)(element.RenderTransform as TransformGroup)
//                  .Children.First(tr => tr is TranslateTransform);
//            }

//            private void HandleSourceUpdated(object sender, SourceChangedEventArgs e)
//            {
//                if (this.manipulationHandler != null && e.NewSource is System.Windows.Interop.HwndSource newHwnd)
//                {
//                    this.manipulationHandler.HwndSource = newHwnd;
//                    this.manipulationHandler.TranslationUpdated += this.ManipulationHandler_TranslationUpdated;
//                }
//            }

//            private void ManipulationHandler_TranslationUpdated(float arg1, float arg2)
//            {
//                this.scrollViewer.ScrollToHorizontalOffset(this.scrollViewer.HorizontalOffset + arg1);
//                this.scrollViewer.ScrollToVerticalOffset(this.scrollViewer.VerticalOffset + arg2);
//                var tt = this.GetTranslateTransform(this.scrollViewer);

//                // tt.X += arg1;
//                // tt.Y += arg2;
//            }

//            private void Dispose(bool disposing)
//            {
//            }
//        }
//    }
//}
