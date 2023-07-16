using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;

namespace GroupMeClient.AvaloniaUI.Extensions
{
    /// <summary>
    /// <see cref="InfiniteScrollBehavior"/> provides an Avalonia Behavior to allow for infinite upward scroll behavior in a
    /// <see cref="ListBox"/>.
    /// </summary>
    public class InfiniteScrollBehavior : Behavior<ListBox>
    {
        /// <summary>
        /// Gets an Avalonia Property for the command to execute when scrolled to the top of the list.
        /// </summary>
        public static readonly DirectProperty<InfiniteScrollBehavior, ICommand> ReachedTopCommandProperty =
            AvaloniaProperty.RegisterDirect<InfiniteScrollBehavior, ICommand>(
                nameof(ReachedTopCommand),
                isb => isb.ReachedTopCommand,
                (isb, command) => isb.ReachedTopCommand = command);

        /// <summary>
        /// Gets an Avalonia Property indicating whether the list should automatically scroll to the bottom.
        /// </summary>
        public static readonly DirectProperty<InfiniteScrollBehavior, bool> AutoScrollToBottomProperty =
          AvaloniaProperty.RegisterDirect<InfiniteScrollBehavior, bool>(
              nameof(AutoScrollToBottom),
              isb => isb.AutoScrollToBottom);

        /// <summary>
        /// Gets an Avalonia Property indicating whether automatic scrolling to the bottom is currently engaged.
        /// </summary>
        public static readonly DirectProperty<InfiniteScrollBehavior, bool> LockedToBottomProperty =
          AvaloniaProperty.RegisterDirect<InfiniteScrollBehavior, bool>(
              nameof(LockedToBottom),
              isb => isb.LockedToBottom);

        private readonly CompositeDisposable disposables = new CompositeDisposable();

        private double verticalHeightMax = 0.0;
        private ICommand reachedTopCommand;
        private bool autoScrollToBottom;
        private bool isLockedToBottom = true;

        /// <summary>
        /// Gets or sets the command to execute when the list is scrolled to the top.
        /// </summary>
        public ICommand ReachedTopCommand
        {
            get => this.reachedTopCommand;
            set => this.SetAndRaise(ReachedTopCommandProperty, ref this.reachedTopCommand, value);
        }

        /// <summary>
        /// Gets a value indicating whether the list should automatically scroll to the bottom.
        /// </summary>
        public bool AutoScrollToBottom
        {
            get => this.autoScrollToBottom;
            private set => this.SetAndRaise(AutoScrollToBottomProperty, ref this.autoScrollToBottom, value);
        }

        /// <summary>
        /// Gets a value indicating whether the list is currently locked to the bottom.
        /// </summary>
        public bool LockedToBottom
        {
            get => this.isLockedToBottom;
            private set => this.SetAndRaise(LockedToBottomProperty, ref this.isLockedToBottom, value);
        }

        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();

            // TODO 11
            Observable.FromEventPattern(this.AssociatedObject, nameof(this.AssociatedObject.LayoutUpdated))
                .Take(1)
                .Subscribe(_ =>
                {
                    var scrollViewer = this.AssociatedObject.Scroll as ScrollViewer;
                    scrollViewer.GetObservable(ScrollViewer.ScrollBarMaximumProperty)
                    .Subscribe(scrollMax =>
                    {
                        this.verticalHeightMax = scrollMax.Y;

                        if (this.LockedToBottom)
                        {
                            // Scroll to bottom
                            scrollViewer.SetValue(ScrollViewer.OffsetProperty, scrollViewer.Offset.WithY(this.verticalHeightMax));
                        }
                    })
                    .DisposeWith(this.disposables);

                    scrollViewer.GetObservable(ScrollViewer.OffsetProperty)
                    .ForEachAsync(offset =>
                    {
                        if (scrollViewer.Extent.Height == 0)
                        {
                            this.LockedToBottom = scrollViewer.Bounds.Height == 0;
                        }

                        if (offset.Y <= double.Epsilon)
                        {
                            // At top
                            if (this.ReachedTopCommand.CanExecute(scrollViewer))
                            {
                                this.ReachedTopCommand.Execute(scrollViewer);
                            }
                        }

                        var delta = Math.Abs(this.verticalHeightMax - offset.Y);

                        if (delta <= double.Epsilon)
                        {
                            // At bottom
                            this.AssociatedObject.SetValue(InfiniteScrollBehaviorPositionHelper.IsNotAtBottomProperty, false);
                            this.LockedToBottom = true;
                        }
                        else
                        {
                            // Not at bottom
                            this.AssociatedObject.SetValue(InfiniteScrollBehaviorPositionHelper.IsNotAtBottomProperty, true);
                            this.LockedToBottom = false;
                        }
                    })
                    .DisposeWith(this.disposables);
                });
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            base.OnDetaching();

            try
            {
                this.disposables.Dispose();
            }
            catch (Exception)
            {
            }
        }
    }
}
