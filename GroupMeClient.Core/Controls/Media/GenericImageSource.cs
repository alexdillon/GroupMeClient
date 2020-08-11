using System;
using System.Collections.Generic;
using System.Text;

namespace GroupMeClient.Core.Controls.Media
{
    public class GenericImageSource
    {
        public GenericImageSource(byte[] rawImageData)
        {
            this.RawImageData = rawImageData;
        }

        public GenericImageSource(byte[] rawImageData, int renderWidth, int renderHeight)
        {
            this.RawImageData = rawImageData;
            this.RenderWidth = renderWidth;
            this.RenderHeight = renderHeight;
        }

        public byte[] RawImageData { get; }

        public int RenderWidth { get; set; }

        public int RenderHeight { get; set; }
    }
}
