using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace GroupMeClient.WpfUI.Extensions
{
    /// <summary>
    /// Calls a method on a specified object when invoked.
    /// </summary>
    public class SetFocusBehavior : TriggerAction<DependencyObject>
    {
        /// <summary>
        /// A Dependency Property for the target that will receive focus.
        /// </summary>
        public static readonly DependencyProperty TargetObjectProperty =
            DependencyProperty.Register(
                "TargetObject",
                typeof(object),
                typeof(SetFocusBehavior),
                new PropertyMetadata());

        /// <summary>
        /// Initializes a new instance of the <see cref="SetFocusBehavior"/> class.
        /// </summary>
        public SetFocusBehavior()
        {
        }

        /// <summary>
        /// Gets or sets the object that will recieve focus when invoked.
        /// </summary>
        public object TargetObject
        {
            get { return (object)this.GetValue(TargetObjectProperty); }
            set { this.SetValue(TargetObjectProperty, value); }
        }

        /// <inheritdoc/>
        protected override void Invoke(object parameter)
        {
            (this.TargetObject as Control)?.Focus();
        }
    }
}
