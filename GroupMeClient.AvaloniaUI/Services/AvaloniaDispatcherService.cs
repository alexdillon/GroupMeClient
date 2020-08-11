using System;
using System.Threading.Tasks;
using GroupMeClient.Core.Services;

namespace GroupMeClient.AvaloniaUI.Services
{
    /// <summary>
    /// <see cref="AvaloniaDispatcherService"/> provides service support for dispatching actions onto the WPF UI Dispatcher.
    /// </summary>
    public class AvaloniaDispatcherService : IUserInterfaceDispatchService
    {
        /// <inheritdoc/>
        public void Invoke(Action action)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(action);
        }

        /// <inheritdoc/>
        public Task InvokeAsync(Action action)
        {
            return Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(action);
        }
    }
}
