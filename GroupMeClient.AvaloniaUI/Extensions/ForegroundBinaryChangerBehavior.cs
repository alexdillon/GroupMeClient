using System;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Xaml.Interactivity;

namespace GroupMeClient.AvaloniaUI.Extensions
{
    /// <summary>
    /// <see cref="ForegroundBinaryChangerBehavior"/> provides a behavior to dynamically change the foreground of a control based
    /// on a the value of a binary data binding.
    /// </summary>
    public class ForegroundBinaryChangerBehavior : Behavior<ContentControl>
    {
        /// <summary>
        /// Gets an Avalonia Property for the value to assign to the background when the bound condition is true.
        /// </summary>
        public static readonly DirectProperty<ForegroundBinaryChangerBehavior, IBrush> TrueValueProperty =
            AvaloniaProperty.RegisterDirect<ForegroundBinaryChangerBehavior, IBrush>(
                nameof(TrueValue),
                bbcb => bbcb.TrueValue,
                (bbcb, value) => bbcb.TrueValue = value);

        /// <summary>
        /// Gets an Avalonia Property for the value to assign to the background when the bound condition is false.
        /// </summary>
        public static readonly DirectProperty<ForegroundBinaryChangerBehavior, IBrush> FalseValueProperty =
            AvaloniaProperty.RegisterDirect<ForegroundBinaryChangerBehavior, IBrush>(
                nameof(FalseValue),
                bbcb => bbcb.FalseValue,
                (bbcb, value) => bbcb.FalseValue = value);

        /// <summary>
        /// Gets an Avalonia Property for the condition to bind to.
        /// </summary>
        public static readonly DirectProperty<ForegroundBinaryChangerBehavior, bool> ConditionProperty =
            AvaloniaProperty.RegisterDirect<ForegroundBinaryChangerBehavior, bool>(
                nameof(Condition),
                bbcb => bbcb.Condition,
                (bbcb, value) => bbcb.Condition = value);

        private IBrush trueValue;
        private IBrush falseValue;
        private bool condition;

        /// <summary>
        /// Gets or sets the foreground to assign when the bound condition is true.
        /// </summary>
        public IBrush TrueValue
        {
            get => this.trueValue;
            set => this.SetAndRaise(TrueValueProperty, ref this.trueValue, value);
        }

        /// <summary>
        /// Gets or sets the foreground to assign when the bound condition is false.
        /// </summary>
        public IBrush FalseValue
        {
            get => this.falseValue;
            set => this.SetAndRaise(FalseValueProperty, ref this.falseValue, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="TrueValue"/> or <see cref="FalseValue"/> foreground should be applied.
        /// </summary>
        public bool Condition
        {
            get => this.condition;
            set
            {
                this.SetAndRaise(ConditionProperty, ref this.condition, value);
                this.UpdateValue(this.AssociatedObject);
            }
        }

        /// <inheritdoc/>
        protected override void OnAttached()
        {
            base.OnAttached();

            Observable.FromEventPattern(this.AssociatedObject, nameof(this.AssociatedObject.LayoutUpdated))
                .Take(1)
                .Subscribe(_ =>
                {
                    this.UpdateValue(this.AssociatedObject);
                });
        }

        /// <inheritdoc/>
        protected override void OnDetaching()
        {
            base.OnDetaching();
        }

        private void UpdateValue(ContentControl control)
        {
            if (control != null)
            {
                if (this.Condition)
                {
                    control.Foreground = this.TrueValue;
                }
                else
                {
                    control.Foreground = this.FalseValue;
                }
            }
        }
    }
}
