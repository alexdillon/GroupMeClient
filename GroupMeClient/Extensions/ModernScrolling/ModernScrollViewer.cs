using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GroupMeClient.Utilities.DirectManipulation;

namespace GroupMeClient.Extensions.ModernScrolling
{
    class ModernScrollViewer : ScrollViewer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModernScrollViewer"/> class.
        /// </summary>
        public ModernScrollViewer()
        {
            this.ManipulationHandler = new PointerBasedManipulationHandler();
        }

        private PointerBasedManipulationHandler ManipulationHandler { get; set; }

        /// <inheritdoc/>
        protected override void OnInitialized(EventArgs e)
        {
            PresentationSource.AddSourceChangedHandler(this, this.HandleSourceUpdated);
            base.OnInitialized(e);

            TransformGroup group = new TransformGroup();
            ScaleTransform st = new ScaleTransform();
            group.Children.Add(st);
            TranslateTransform tt = new TranslateTransform();
            group.Children.Add(tt);
            this.RenderTransform = group;
            this.RenderTransformOrigin = new Point(0.0, 0.0);
        }

        private TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform)(element.RenderTransform as TransformGroup)
              .Children.First(tr => tr is TranslateTransform);
        }


        private void HandleSourceUpdated(object sender, SourceChangedEventArgs e)
        {
            if (this.ManipulationHandler != null && e.NewSource is System.Windows.Interop.HwndSource newHwnd)
            {
                this.ManipulationHandler.HwndSource = newHwnd;
                this.ManipulationHandler.TranslationUpdated += this.ManipulationHandler_TranslationUpdated;
            }
        }

        private void ManipulationHandler_TranslationUpdated(float arg1, float arg2)
        {
            this.ScrollToHorizontalOffset(this.HorizontalOffset + arg1);
            this.ScrollToVerticalOffset(this.VerticalOffset + arg2);
            var tt = this.GetTranslateTransform(this);
            //tt.X += arg1;
            //tt.Y += arg2;

        }
    }
}
