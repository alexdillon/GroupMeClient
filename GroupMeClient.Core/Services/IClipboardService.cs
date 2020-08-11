using System;
using System.Collections.Generic;
using System.Text;
using GroupMeClient.Core.Controls.Media;

namespace GroupMeClient.Core.Services
{
    public interface IClipboardService
    {
        void CopyImage(GenericImageSource imageSource);

        void CopyText(string text);
    }
}
