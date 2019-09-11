using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GroupMeClient.Extensions
{
    /// <summary>
    /// <see cref="ZoomBorder"/> provides support for panning and zooming WPF controls.
    /// </summary>
    /// <remarks>
    /// Adapted from https://stackoverflow.com/a/6782715.
    /// </remarks>
    public class ZoomBorder : Border
    {
        private UIElement child = null;
        private Point origin;
        private Point start;

        /// <inheritdoc/>
        public override UIElement Child
        {
            get
            {
                return base.Child;
            }

            set
            {
                if (value != null && value != this.Child)
                {
                    this.Initialize(value);
                }

                base.Child = value;
            }
        }

        /// <summary>
        /// Initializes the <see cref="ZoomBorder"/> control.
        /// </summary>
        /// <param name="element">The child control to allow zooming and panning operations on.</param>
        public void Initialize(UIElement element)
        {
            this.child = element;
            if (this.child != null)
            {
                TransformGroup group = new TransformGroup();
                ScaleTransform st = new ScaleTransform();
                group.Children.Add(st);
                TranslateTransform tt = new TranslateTransform();
                group.Children.Add(tt);
                this.child.RenderTransform = group;
                this.child.RenderTransformOrigin = new Point(0.0, 0.0);
                this.MouseWheel += this.Child_MouseWheel;
                this.MouseLeftButtonDown += this.Child_MouseLeftButtonDown;
                this.MouseLeftButtonUp += this.Child_MouseLeftButtonUp;
                this.MouseMove += this.Child_MouseMove;
                this.PreviewMouseRightButtonDown += new MouseButtonEventHandler(
                  this.Child_PreviewMouseRightButtonDown);
                this.ManipulationStarting += this.ZoomBorder_ManipulationStarting;
                this.ManipulationDelta += this.ZoomBorder_ManipulationDelta;
            }
        }

        /// <summary>
        /// Resets the zoom and scale factors to default.
        /// </summary>
        public void Reset()
        {
            if (this.child != null)
            {
                // reset zoom
                var st = this.GetScaleTransform(this.child);
                st.ScaleX = 1.0;
                st.ScaleY = 1.0;

                // reset pan
                var tt = this.GetTranslateTransform(this.child);
                tt.X = 0.0;
                tt.Y = 0.0;
            }
        }

        private TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform)(element.RenderTransform as TransformGroup)
              .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform)(element.RenderTransform as TransformGroup)
              .Children.First(tr => tr is ScaleTransform);
        }

        private void Child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (this.child != null)
            {
                var st = this.GetScaleTransform(this.child);
                var tt = this.GetTranslateTransform(this.child);

                if (st == null || tt == null)
                {
                    return;
                }

                double zoom = e.Delta > 0 ? .2 : -.2;
                if (!(e.Delta > 0) && (st.ScaleX < .4 || st.ScaleY < .4))
                {
                    return;
                }

                Point relative = e.GetPosition(this.child);
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

        private void Child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.child != null)
            {
                var tt = this.GetTranslateTransform(this.child);
                this.start = e.GetPosition(this);
                this.origin = new Point(tt.X, tt.Y);
                this.Cursor = Cursors.Hand;
                this.child.CaptureMouse();
            }
        }

        private void Child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.child != null)
            {
                this.child.ReleaseMouseCapture();
                this.Cursor = Cursors.Arrow;
            }
        }

        private void ZoomBorder_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = this;
        }

        private void ZoomBorder_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (this.child != null)
            {
                var st = this.GetScaleTransform(this.child);
                var tt = this.GetTranslateTransform(this.child);

                if (st == null || tt == null)
                {
                    return;
                }

                st.ScaleX *= e.DeltaManipulation.Scale.X;
                st.ScaleY *= e.DeltaManipulation.Scale.Y;
                if (e.DeltaManipulation.Scale.X > 1.0 || e.DeltaManipulation.Scale.Y > 1.0)
                {
                    st.CenterX = e.ManipulationOrigin.X;
                    st.CenterY = e.ManipulationOrigin.Y;
                }

                tt.X += e.DeltaManipulation.Translation.X;
                tt.Y += e.DeltaManipulation.Translation.Y;

                if (st.ScaleX < 1.0 || st.ScaleY < 1.0)
                {
                    st.ScaleX = 1.0;
                    st.ScaleY = 1.0;

                    tt.X = 0;
                    tt.Y = 0;
                }
            }
        }

        private void Child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Reset();
        }

        private void Child_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.child != null)
            {
                if (this.child.IsMouseCaptured)
                {
                    var tt = this.GetTranslateTransform(this.child);
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