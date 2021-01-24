using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace MicroCubeAvalonia.Controls
{
    /// <summary>
    /// <see cref="ProgressRing"/> provides a Windows 10 styled circular loading animation.
    /// </summary>
    /// <remarks>
    /// Adapted from the MahApps Metro implementation at
    /// https://github.com/MahApps/MahApps.Metro/blob/develop/src/MahApps.Metro/Controls/ProgressRing.cs.
    /// </remarks>
    public class ProgressRing : TemplatedControl
    {
        public static readonly AvaloniaProperty IsActiveProperty =
            AvaloniaProperty.Register<ProgressRing, bool>(
                "IsActive",
                inherits: true,
                defaultValue: true);

        public static readonly AvaloniaProperty MaxSideLengthProperty =
            AvaloniaProperty.Register<ProgressRing, double>(
                "MaxSideLength",
                inherits: true);

        public static readonly AvaloniaProperty EllipseDiameterProperty =
            AvaloniaProperty.Register<ProgressRing, double>(
                "EllipseDiameter",
                inherits: true);

        public static readonly AvaloniaProperty EllipseOffsetProperty =
            AvaloniaProperty.Register<ProgressRing, Thickness>(
                "EllipseOffset",
                inherits: true);

        public static readonly AvaloniaProperty EllipseDiameterScaleProperty =
            AvaloniaProperty.Register<ProgressRing, double>(
                "EllipseDiameterScale",
                inherits: true,
                defaultValue: 1D);

        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public ProgressRing()
        {
            this.GetObservable(Control.BoundsProperty).ForEachAsync((rect) =>
            {
                this.SetEllipseDiameter(rect.Width);
                this.SetEllipseOffset(rect.Width);
                this.SetMaxSideLength(rect.Width);
            }).DisposeWith(this.disposables);
        }

        public double MaxSideLength
        {
            get => (double)this.GetValue(MaxSideLengthProperty);
            private set => this.SetValue(MaxSideLengthProperty, value);
        }

        public double EllipseDiameter
        {
            get => (double)this.GetValue(EllipseDiameterProperty);
            private set => this.SetValue(EllipseDiameterProperty, value);
        }

        public double EllipseDiameterScale
        {
            get => (double)this.GetValue(EllipseDiameterScaleProperty);
            set => this.SetValue(EllipseDiameterScaleProperty, value);
        }

        public Thickness EllipseOffset
        {
            get => (Thickness)this.GetValue(EllipseOffsetProperty);
            private set => this.SetValue(EllipseOffsetProperty, value);
        }

        public bool IsActive
        {
            get => (bool)this.GetValue(IsActiveProperty);
            set => this.SetValue(IsActiveProperty, value);
        }

        private void SetMaxSideLength(double width)
        {
            this.MaxSideLength = width <= 20 ? 20 : width;
        }

        private void SetEllipseDiameter(double width)
        {
            this.EllipseDiameter = (width / 8) * this.EllipseDiameterScale;
        }

        private void SetEllipseOffset(double width)
        {
            this.EllipseOffset = new Thickness(0, width / 2, 0, 0);
        }
    }
}
