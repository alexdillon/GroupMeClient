using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace GroupMeClient.AvaloniaUI.Extensions
{
    public class ListBoxExt : ListBox, IStyleable
    {
        /// <summary>
        /// Dependency property for <see cref="IsAtBottom"/>.
        /// </summary>
        public static readonly DirectProperty<ListBoxExt, bool> IsAtBottomProperty =
          AvaloniaProperty.RegisterDirect<ListBoxExt, bool>(
              nameof(IsAtBottom),
              o => o.IsAtBottom);

        /// <summary>
        /// Gets an Avalonia Property for the command to execute when scrolled to the top of the list.
        /// </summary>
        public static readonly DirectProperty<ListBoxExt, ICommand> ReachedTopCommandProperty =
            AvaloniaProperty.RegisterDirect<ListBoxExt, ICommand>(
                nameof(ReachedTopCommand),
                isb => isb.ReachedTopCommand,
                (isb, command) => isb.ReachedTopCommand = command);

        /// <summary>
        /// Gets an Avalonia Property indicating whether the list should automatically scroll to the bottom.
        /// </summary>
        public static readonly DirectProperty<ListBoxExt, bool> AutoScrollToBottomProperty =
          AvaloniaProperty.RegisterDirect<ListBoxExt, bool>(
              nameof(AutoScrollToBottom),
              isb => isb.AutoScrollToBottom);

        /// <summary>
        /// Gets an Avalonia Property indicating whether automatic scrolling to the bottom is currently engaged.
        /// </summary>
        public static readonly DirectProperty<ListBoxExt, bool> LockedToBottomProperty =
          AvaloniaProperty.RegisterDirect<ListBoxExt, bool>(
              nameof(LockedToBottom),
              isb => isb.LockedToBottom);

        private readonly CompositeDisposable disposables = new CompositeDisposable();
        private double verticalHeightMax = 0.0;

        private bool isAtBottom;
        private ICommand reachedTopCommand;
        private bool autoScrollToBottom;
        private bool isLockedToBottom = true;

        /// <inheritdoc/>
        Type IStyleable.StyleKey => typeof(ListBox);

        protected override void OnLoaded(Avalonia.Interactivity.RoutedEventArgs e)
        {
            base.OnLoaded(e);
            Observable.FromEventPattern(this, nameof(this.LayoutUpdated))
                .Take(1)
                .Subscribe(_ =>
                {
                    var scrollViewer = this.Scroll as ScrollViewer;
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
                            this.IsAtBottom = true;
                            this.LockedToBottom = true;
                        }
                        else
                        {
                            // Not at bottom
                            this.IsAtBottom = false;
                            this.LockedToBottom = false;
                        }
                    })
                    .DisposeWith(this.disposables);
                });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListBoxExt"/> class.
        /// </summary>
        public ListBoxExt()
        {
            
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ListBoxExt"/> is scrolled
        /// to the bottom.
        /// </summary>
        public bool IsAtBottom
        {
            get => this.isAtBottom;
            private set => this.SetAndRaise(IsAtBottomProperty, ref this.isAtBottom, value);
        }

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
    }
}
