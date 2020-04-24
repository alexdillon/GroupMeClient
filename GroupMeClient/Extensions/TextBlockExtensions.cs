using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace GroupMeClient.Extensions
{
    /// <summary>
    /// <see cref="TextBlockExtensions"/> provide MVVM support for binding <see cref="Inline"/>s to a TextBlock.
    /// </summary>
    /// <remarks>
    /// Adapted into an extension method based on original answer from
    /// https://stackoverflow.com/a/9546372.
    /// </remarks>
    public class TextBlockExtensions
    {
        /// <summary>
        /// Gets or sets a dependency property for the bindable <see cref="Inline"/>s for this <see cref="TextBlock"/>.
        /// </summary>
        public static readonly DependencyProperty InlineListProperty =
            DependencyProperty.RegisterAttached(
                "InlineList",
                typeof(ObservableCollection<Inline>),
                typeof(TextBlockExtensions),
                new UIPropertyMetadata(null, OnPropertyChanged));

        /// <summary>
        /// Gets a value indicating containing the <see cref="Inline"/>s for this <see cref="TextBlock"/>.
        /// </summary>
        /// <param name="instance">The dependency object to retreive the property from.</param>
        /// <returns>A boolean indicating whether enabled.</returns>
        public static ObservableCollection<Inline> GetInlineList(TextBlock instance)
        {
            return (ObservableCollection<Inline>)instance.GetValue(InlineListProperty);
        }

        /// <summary>
        /// Sets a value containing a list of <see cref="Inline"/>s for this <see cref="TextBlock"/>.
        /// </summary>
        /// <param name="instance">The dependency object to apply the property to.</param>
        /// <param name="value">Whether scroll to end is enabled. </param>
        public static void SetInlineList(TextBlock instance, ObservableCollection<Inline> value)
        {
            instance.SetValue(InlineListProperty, value);
        }

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            ObservableCollection<Inline> list = e.NewValue as ObservableCollection<Inline>;

            if (list != null)
            {
                list.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(InlineCollectionChanged);

                textBlock.Inlines.Clear();
                textBlock.Inlines.AddRange(list);
            }
        }

        private static void InlineCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    int idx = e.NewItems.Count - 1;
                    Inline inline = e.NewItems[idx] as Inline;
                    textBlock.Inlines.Add(inline);
                }
            }
        }
    }
}
