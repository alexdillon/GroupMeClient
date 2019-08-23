using System;
using System.Threading;
using GalaSoft.MvvmLight.Messaging;

namespace GroupMeClient.Utilities
{
    /// <summary>
    /// <see cref="ReliabilityStateMachine"/> provides state machine functionality for variable timing for subsequent retry attempts.
    /// </summary>
    public class ReliabilityStateMachine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReliabilityStateMachine"/> class.
        /// </summary>
        public ReliabilityStateMachine()
        {
            this.CurrentTry = 0;
        }

        private int CurrentTry { get; set; }

        private TimeSpan[] TimeoutIntervals { get; } =
            {
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30),
            };

        /// <summary>
        /// Generates a new timer to retry an action again after the appropriate timeout has elapsed. Determining the appropriate
        /// wait time is handled automatically.
        /// </summary>
        /// <param name="retryAction">The action to perform again.</param>
        /// <returns>A timer configured for the correct retry duration. After the wait period has elasped, the specified action will be retried.</returns>
        public Timer GetRetryTimer(Action retryAction)
        {
            var currentInterval = this.TimeoutIntervals[Math.Min(this.CurrentTry, this.TimeoutIntervals.Length - 1)];

            this.CurrentTry++;

            if (this.CurrentTry == 1)
            {
                // Indicate a disconnect the first time something fails
                var disconnectUpdateRequest = new Messaging.DisconnectedRequestMessage(disconnected: true);
                Messenger.Default.Send(disconnectUpdateRequest);
            }

            return new Timer(
                (l) => retryAction(),
                null,
                currentInterval,
                Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        /// Resets the state-machine status. This method should called after a retry-attempt succeeds.
        /// </summary>
        public void Succeeded()
        {
            if (this.CurrentTry > 0)
            {
                // Indicate a connection (not disconnected) only on a disconnected->connected transition.
                var connectUpdateRequest = new Messaging.DisconnectedRequestMessage(disconnected: false);
                Messenger.Default.Send(connectUpdateRequest);
            }

            this.CurrentTry = 0;
        }
    }
}
