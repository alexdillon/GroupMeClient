using DesktopNotifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static DesktopNotifications.NotificationActivator;

namespace GroupMeClient.Notifications.Display.Win10
{
    // The GUID CLSID must be unique to your app. Create a new GUID if copying this code.
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(INotificationActivationCallback))]
    [Guid("445fd45d-6126-46cb-bdf9-bd5343eaee5a"), ComVisible(true)]
    public class MyNotificationActivator : NotificationActivator
    {
        public override void OnActivated(string invokedArgs, NotificationUserInput userInput, string appUserModelId)
        {
            // TODO: Handle activation
        }
    }
}
