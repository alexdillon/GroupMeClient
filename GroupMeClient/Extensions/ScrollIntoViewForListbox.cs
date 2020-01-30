using System;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace GroupMeClient.Extensions
{
    /// <summary>
    /// <see cref="ScrollIntoViewForListBox"/> provides a behavior to ensure a selected item in a <see cref="ListBox"/>
    /// is visible.
    /// </summary>
    /// <remarks>
    /// Adapted from https://stackoverflow.com/a/8830961.
    /// </remarks>
    public class ScrollIntoViewForListBox : Behavior<ListBox>
    {
        /// <inheritdoc/>
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.SelectionChanged += this.AssociatedObject_SelectionChanged;
        }

        /// <inheritdoc/>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.SelectionChanged -= this.AssociatedObject_SelectionChanged;
        }

        private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox)
            {
                ListBox listBox = sender as ListBox;
                if (listBox.SelectedItem != null)
                {
                    listBox.Dispatcher.BeginInvoke(
                        (Action)(() =>
                        {
                            listBox.UpdateLayout();
                            if (listBox.SelectedItem != null)
                            {
                                listBox.ScrollIntoView(listBox.SelectedItem);
                            }
                        }));
                }
            }
        }
    }
}
