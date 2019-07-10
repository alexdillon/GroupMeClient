using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupMeClient.Extensions
{
    class WebBrowserHelper
    {
        public static void OpenUrl(string url)
        {
            System.Diagnostics.Process.Start(url);
        }
    }
}
