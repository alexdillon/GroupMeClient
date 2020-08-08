using System;
using System.Threading.Tasks;
using System.Windows;
using GroupMeClient.Core.Services;

namespace GroupMeClient.WpfUI.Services
{
    /// <summary>
    /// <see cref="WpfDispatcherService"/> provides service support for dispatching actions onto the WPF UI Dispatcher.
    /// </summary>
    public class WpfDispatcherService : IUserInterfaceDispatchService
    {
        /// <inheritdoc/>
        public void Invoke(Action action)
        {
            Application.Current.Dispatcher.Invoke(action);
        }

        /// <inheritdoc/>
        public Task InvokeAsync(Action action)
        {
            return Application.Current.Dispatcher.InvokeAsync(action).Task;
        }
    }
}
