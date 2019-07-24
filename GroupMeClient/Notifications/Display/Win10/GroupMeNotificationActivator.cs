using System;
using System.Runtime.InteropServices;
using DesktopNotifications;

namespace GroupMeClient.Notifications.Display.Win10
{
    /// <summary>
    /// Provides a COM Interface to support activation when a user clicks on a Windows 10 Toast Notfication.
    /// </summary>
    /// <remarks>
    /// Squirrel automatically generates a CLDID based on the NuGet package name.
    /// For 'GroupMeDesktopClient', this generated GUID is 'bc6.....e4'.
    /// </remarks>
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(INotificationActivationCallback))]
    [ComVisible(true)]
    [Guid("bc6ba895-fa13-5e3f-8d6b-6f99f0c2e6e4")]
    public class GroupMeNotificationActivator : NotificationActivator
    {
        /// <inheritdoc/>
        public override void OnActivated(string invokedArgs, NotificationUserInput userInput, string appUserModelId)
        {
            // TODO: Handle activation
        }
    }
}
