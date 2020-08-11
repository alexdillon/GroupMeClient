using System;
using System.Threading.Tasks;

namespace GroupMeClient.Core.Services
{
    /// <summary>
    /// <see cref="IUserInterfaceDispatchService"/> provides a service for dispatching calls on the the user-interface thread of an application.
    /// </summary>
    public interface IUserInterfaceDispatchService
    {
        /// <summary>
        /// Invokes an action sychronously on the UI dispatcher.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        void Invoke(Action action);

        /// <summary>
        /// Invokes an action asychronously on the UI dispatcher.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task InvokeAsync(Action action);
    }
}
