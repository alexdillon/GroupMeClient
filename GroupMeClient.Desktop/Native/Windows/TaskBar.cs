using System;
using System.Runtime.InteropServices;

namespace GroupMeClient.Desktop.Native.Windows
{
    public class TaskBar
    {
        [DllImport("shell32.dll", SetLastError = true)]
        public static extern void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppID);
    }
}
