using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace GroupMeClient.AvaloniaUI.Extensions
{
    /// <summary>
    /// <see cref="ZoomBorder"/> provides support for panning and zooming WPF controls.
    /// </summary>
    /// <remarks>
    /// Adapted from https://stackoverflow.com/a/6782715.
    /// </remarks>
    public class ZoomBorder : Border
    {
        private Point origin;
        private Point start;

        private bool IsMouseDown { get; set; }

        /// <inheritdoc/>
        public override void EndInit()
        {
            base.EndInit();
            this.Initialize();
        }

        /// <summary>
        /// Initializes the <see cref="ZoomBorder"/> control.
        /// </summary>
        public void Initialize()
        {
            if (this.Child != null)
            {
                var group = new TransformGroup();
                var st = new ScaleTransform();
                var tt = new TranslateTransform();

                group.Children.Add(st);
                group.Children.Add(tt);

                this.Child.RenderTransform = group;
                this.Child.RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Absolute);

                this.PointerWheelChanged += this.ZoomBorder_PointerWheelChanged;
                this.PointerPressed += this.ZoomBorder_PointerPressed;
                this.PointerReleased += this.ZoomBorder_PointerReleased;
                this.PointerMoved += this.ZoomBorder_PointerMoved;
            }
        }

        /// <summary>
        /// Resets the zoom and scale factors to default.
        /// </summary>
        public void Reset()
        {
            if (this.Child != null)
            {
                // reset zoom
                var st = this.GetScaleTransform(this.Child);
                st.ScaleX = 1.0;
                st.ScaleY = 1.0;

                // reset pan
                var tt = this.GetTranslateTransform(this.Child);
                tt.X = 0.0;
                tt.Y = 0.0;
            }
        }

        private TranslateTransform GetTranslateTransform(Control element)
        {
            return (TranslateTransform)(element.RenderTransform as TransformGroup)
              .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(Control element)
        {
            return (ScaleTransform)(element.RenderTransform as TransformGroup)
              .Children.First(tr => tr is ScaleTransform);
        }

        private void ZoomBorder_PointerWheelChanged(object sender, Avalonia.Input.PointerWheelEventArgs e)
        {
            if (this.Child != null)
            {
                var st = this.GetScaleTransform(this.Child);
                var tt = this.GetTranslateTransform(this.Child);

                if (st == null || tt == null)
                {
                    return;
                }

                double zoom = e.Delta.Y > 0 ? .1 : -.1;
                if (!(e.Delta.Y > 0) && (st.ScaleX < .4 || st.ScaleY < .4))
                {
                    return;
                }

                Point relative = e.GetPosition(this.Child);
                double absoluteX;
                double absoluteY;

                absoluteX = (relative.X * st.ScaleX) + tt.X;
                absoluteY = (relative.Y * st.ScaleY) + tt.Y;

                st.ScaleX += zoom;
                st.ScaleY += zoom;

                tt.X = absoluteX - (relative.X * st.ScaleX);
                tt.Y = absoluteY - (relative.Y * st.ScaleY);

                if (st.ScaleX < 1.0 || st.ScaleY < 1.0)
                {
                    st.ScaleX = 1.0;
                    st.ScaleY = 1.0;

                    tt.X = 0;
                    tt.Y = 0;
                }
            }
        }

        private void ZoomBorder_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (this.Child != null && e.GetCurrentPoint(null).Properties.PointerUpdateKind == Avalonia.Input.PointerUpdateKind.RightButtonPressed)
            {
                var tt = this.GetTranslateTransform(this.Child);
                this.start = e.GetPosition(this);
                this.origin = new Point(tt.X, tt.Y);
                this.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand);
                this.IsMouseDown = true;
            }
        }

        private void ZoomBorder_PointerReleased(object sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            if (this.Child != null && e.GetCurrentPoint(null).Properties.PointerUpdateKind == Avalonia.Input.PointerUpdateKind.RightButtonReleased)
            {
                this.IsMouseDown = false;
                this.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Arrow);
            }
        }

        private void ZoomBorder_PointerMoved(object sender, Avalonia.Input.PointerEventArgs e)
        {
            if (this.Child != null)
            {
                if (this.IsMouseDown)
                {
                    var tt = this.GetTranslateTransform(this.Child);
                    if (tt == null)
                    {
                        return;
                    }

                    Vector v = this.start - e.GetPosition(this);
                    tt.X = this.origin.X - v.X;
                    tt.Y = this.origin.Y - v.Y;
                }
            }
        }
    }
}