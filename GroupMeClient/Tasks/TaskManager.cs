using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace GroupMeClient.Tasks
{
    /// <summary>
    /// <see cref="TaskManager"/> manages a collection of running <see cref="GroupMeTask"/>s.
    /// </summary>
    public class TaskManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskManager"/> class.
        /// </summary>
        public TaskManager()
        {
            this.RunningTasks = new ObservableCollection<GroupMeTask>();
        }

        /// <summary>
        /// Event that fires when the number of tasks that are currently executing changes.
        /// </summary>
        public event Action<object, EventArgs> TaskCountChanged;

        /// <summary>
        /// Gets a listing of <see cref="GroupMeTask"/> that are currently running.
        /// </summary>
        public ObservableCollection<GroupMeTask> RunningTasks { get; }

        /// <summary>
        /// Begins execution of a new task.
        /// </summary>
        /// <param name="name">The name of the operation.</param>
        /// <param name="tag">A user-defined tag for the <see cref="GroupMeTask"/>.</param>
        /// <param name="payload">The task payload that should be scheduled for execution.</param>
        /// <param name="cancellationTokenSource">The token source that should be used to cancel the task.</param>
        public void AddTask(string name, string tag, Task payload, CancellationTokenSource cancellationTokenSource = null)
        {
            var task = new GroupMeTask(name, tag, payload, cancellationTokenSource);
            payload.ContinueWith(x => this.TaskCompleted(x, task));
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.RunningTasks.Add(task);
            });

            this.TaskCountChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Updates the count of unnamed, background loading tasks that are currently executing.
        /// </summary>
        /// <param name="loadCount">The number of background tasks running.</param>
        public void UpdateNumberOfBackgroundLoads(int loadCount)
        {
            GroupMeTask currentBackgroundTask = this.RunningTasks.FirstOrDefault(t => t.Tag == "backgroundcount");
            if (currentBackgroundTask != null)
            {
                this.RunningTasks.Remove(currentBackgroundTask);
            }

            if (loadCount > 0)
            {
                var newBackgroundTask = new GroupMeTask(
                    $"{loadCount} Group Updates",
                    "backgroundcount",
                    Task.CompletedTask);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    this.RunningTasks.Insert(0, newBackgroundTask);
                });
            }

            this.TaskCountChanged?.Invoke(this, new EventArgs());
        }

        private void TaskCompleted(Task value, GroupMeTask taskWrapper)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.RunningTasks.Remove(taskWrapper);
            });

            this.TaskCountChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// <see cref="GroupMeTask"/> represents an asychronous named operation that runs with access to <see cref="Group"/> or <see cref="Chat"/> data.
        /// </summary>
        public class GroupMeTask
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GroupMeTask"/> class.
            /// </summary>
            /// <param name="name">The name of the operation.</param>
            /// <param name="tag">A user-defined tag for the <see cref="GroupMeTask"/>.</param>
            /// <param name="payload">The task payload that should be scheduled for execution.</param>
            /// <param name="cancellationTokenSource">The token source that should be used to cancel the task.</param>
            public GroupMeTask(string name, string tag, Task payload, CancellationTokenSource cancellationTokenSource = null)
            {
                this.Name = name;
                this.Tag = tag;
                this.Payload = payload;
                this.CancellationTokenSource = cancellationTokenSource ?? new CancellationTokenSource();
            }

            /// <summary>
            /// Gets the name of the background task.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Gets the tag used to uniquely identify this task.
            /// </summary>
            public string Tag { get; }

            private CancellationTokenSource CancellationTokenSource { get; }

            private Task Payload { get; }

            /// <summary>
            /// Cancels the ongoing task.
            /// </summary>
            public void Cancel()
            {
                this.CancellationTokenSource.Cancel();
            }
        }
    }
}
