using System;
using System.Runtime.InteropServices;
using DesktopNotifications;

namespace GroupMeClient.Notifications.Display.Win10
{
    /// <summary>
    /// Provides a COM Interface to support activation when a user clicks on a Windows 10 Toast Notfication.
    /// </summary>
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(INotificationActivationCallback))]
    [ComVisible(true)]
    [Guid("445fd45d-6126-46cb-bdf9-bd5343eaee5a")]
    public class GroupMeNotificationActivator : NotificationActivator
    {
        /// <inheritdoc/>
        public override void OnActivated(string invokedArgs, NotificationUserInput userInput, string appUserModelId)
        {
            // TODO: Handle activation
        }
    }
}
