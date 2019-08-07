using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;

namespace GroupMeClient.Extensions
{
    /// <summary>
    /// <see cref="ListBoxWithPosition"/> provides a ListBox control that reports on scroll position.
    /// </summary>
    public class ListBoxWithPosition : ListBox
    {
        /// <summary>
        /// Gets a Dependency Property Key for the IsNotAtBottom Property.
        /// </summary>
        internal static readonly DependencyPropertyKey IsNotAtBottomPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "IsNotAtBottom",
                typeof(bool),
                typeof(ListBoxWithPosition),
                new PropertyMetadata(false));

#pragma warning disable SA1202 // Elements should be ordered by access. Initialization for IsNotAtBottomProperty depends on IsNotAtBottomPropertyKey.
        /// <summary>
        /// Gets a Dependency Property indicating whether the ListBox is scrolled to the bottom.
        /// </summary>
        public static readonly DependencyProperty IsNotAtBottomProperty =
            IsNotAtBottomPropertyKey.DependencyProperty;
#pragma warning restore SA1202 // Elements should be ordered by access

        /// <summary>
        /// Initializes a new instance of the <see cref="ListBoxWithPosition"/> class.
        /// </summary>
        public ListBoxWithPosition()
        {
            this.Loaded += this.ListBoxWithPosition_Loaded;
            this.ScrollToEnd = new RelayCommand(this.DoScrollToEnd);
        }

        /// <summary>
        /// Gets a value indicating whether the ListBox is scrolled to the bottom.
        /// </summary>
        public bool IsNotAtBottom => (bool)this.GetValue(IsNotAtBottomProperty);

        /// <summary>
        /// Gets a command that can be executed to scroll this <see cref="ListBox"/> to the bottom.
        /// </summary>
        public ICommand ScrollToEnd { get; }

        private ScrollViewer ScrollViewer { get; set; }

        private bool ShouldSnapToBottom { get; set; } = false;

        private void ListBoxWithPosition_Loaded(object sender, RoutedEventArgs e)
        {
            this.ScrollViewer = ListBoxExtensions.FindSimpleVisualChild<ScrollViewer>(this);
            this.ScrollViewer.ScrollChanged += this.ScrollViewer_ScrollChanged;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var atBottom = e.VerticalOffset == (e.OriginalSource as ScrollViewer).ScrollableHeight;
            this.SetValue(IsNotAtBottomPropertyKey, !atBottom);

            if (this.ShouldSnapToBottom)
            {
                if (atBottom)
                {
                    this.ShouldSnapToBottom = false;
                }
                else
                {
                    this.ScrollViewer.ScrollToBottom();
                }
            }
        }

        private void DoScrollToEnd()
        {
            this.ShouldSnapToBottom = true;
            this.ScrollViewer.ScrollToBottom();
        }
    }
}
