using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupMeClient.Extensions.ModernScrolling
{
    internal class ModernScrollBase
    {
        private static bool enabled = false;

        public static void Setup()
        {
            if (!enabled)
            {
                //AppContext.SetSwitch("Switch.System.Windows.Input.Stylus.DisableStylusAndTouchSupport", true);
                AppContext.SetSwitch("Switch.System.Windows.Input.Stylus.EnablePointerSupport", true);

                enabled = true;
            }
        }
    }
}
