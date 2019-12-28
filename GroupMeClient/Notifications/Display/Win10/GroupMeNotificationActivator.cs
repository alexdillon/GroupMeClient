using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Runtime.InteropServices;
using static Microsoft.Toolkit.Uwp.Notifications.NotificationActivator;

namespace GroupMeClient.Notifications.Display.Win10
{
    /// <summary>
    /// Provides a COM Interface to support activation when a user clicks on a Windows 10 Toast Notfication.
    /// </summary>
    /// <remarks>
    /// Squirrel automatically generates a CLDID based on the NuGet package name.
    /// For 'GroupMeDesktopClient', this generated GUID is 'c43177b6-7264-5132-b051-89078b372559'.
    /// </remarks>
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(INotificationActivationCallback))]
    [ComVisible(true)]
    [Guid("3d1bf80b-078b-5aee-b9a0-fc40af7fc030")]
    public class GroupMeNotificationActivator : NotificationActivator
    {
        /// <inheritdoc/>
        public override void OnActivated(string invokedArgs, NotificationUserInput userInput, string appUserModelId)
        {
            // TODO: Handle activation
        }
    }
}
