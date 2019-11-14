//using System;
//using System.Linq;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Interop;
//using System.Windows.Media;
//using GroupMeClient.Utilities.DirectManipulation;

//namespace GroupMeClient.Extensions.ModernScrolling
//{
//    public static class ModernListBoxExtension
//    {
//        /// <summary>
//        /// Gets a dependency property indicating if Direct Manipulation Scrolling is enabled.
//        /// </summary>
//        public static readonly DependencyProperty DirectManipulationEnabledProperty =
//            DependencyProperty.RegisterAttached(
//                "EnableDirectManipulation",
//                typeof(bool),
//                typeof(ModernListBoxExtension),
//                new PropertyMetadata(false, HookupEnableDirectManipulation));

//        /// <summary>
//        /// Gets a value indicating whether Direct Manipulation Scrolling is enabled.
//        /// </summary>
//        /// <param name="instance">The dependency object to retreive the property from.</param>
//        /// <returns>A boolean indicating whether enabled.</returns>
//        public static bool GetEnableDirectManipulation(ListBox instance)
//        {
//            return (bool)instance.GetValue(DirectManipulationEnabledProperty);
//        }

//        /// <summary>
//        /// Sets a value indicating whether Direct Manipulation Scrolling is enabled.
//        /// </summary>
//        /// <param name="instance">The dependency object to retreive the property from.</param>
//        /// <param name="value">Whether Direct Manipulation Scrolling is enabled. </param>
//        public static void SetEnableDirectManipulation(ListBox instance, bool value)
//        {
//            if (value)
//            {
//                instance.Loaded += Loaded;

//                instance.SetValue(DirectManipulationEnabledProperty, true);
//            }
//        }

//        private static void Loaded(object sender, EventArgs e)
//        {
//            (sender as ListBox).Loaded -= Loaded;

//            var scrollViewer = Extensions.ListBoxExtensions.FindSimpleVisualChild<ScrollViewer>(sender as ListBox);
//            ModernScrollViewerExtension.SetEnableDirectManipulation(scrollViewer, true);
//        }

//        private static void OnListBoxLoaded(object sender, RoutedEventArgs e)
//        {

//        }

//        private static void HookupEnableDirectManipulation(DependencyObject d, DependencyPropertyChangedEventArgs e)
//        {
//            if (!(d is ListBox scrollViewer))
//            {
//                return;
//            }

//            SetEnableDirectManipulation(scrollViewer, (bool)e.NewValue);
//        }
//    }
//}
