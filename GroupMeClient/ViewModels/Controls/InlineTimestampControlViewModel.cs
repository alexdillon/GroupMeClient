using System;
using System.Windows.Media;
using GroupMeClientApi.Models;

namespace GroupMeClient.ViewModels.Controls
{
    /// <summary>
    /// <see cref="InlineTimestampControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.InlineTimestampControl"/> control.
    /// </summary>
    public class InlineTimestampControlViewModel : MessageControlViewModelBase, IDisposable
    {
        private DateTime timestamp;

        /// <summary>
        /// Initializes a new instance of the <see cref="InlineTimestampControlViewModel"/> class.
        /// </summary>
        /// <param name="timestamp">The timestamp to display.</param>
        /// <param name="id">The message ID to show the timestamp near.</param>
        /// <param name="didISendIt">Whether the timestamp corresponds to a message sent by the current user.</param>
        public InlineTimestampControlViewModel(DateTime timestamp, string id, bool didISendIt)
        {
            this.Timestamp = timestamp;
            this.Id = id;
            this.DidISendIt = didISendIt;
        }

        /// <inheritdoc/>
        public override string Id { get; }

        /// <inheritdoc/>
        public override Message Message
        {
            get => null;
            set { }
        }

        /// <summary>
        /// Gets a value indicating whether the current user sent this <see cref="Message"/>.
        /// </summary>
        public bool DidISendIt { get; private set; }

        /// <inheritdoc />
        public override bool IsSelectable => false;

        /// <summary>
        /// Gets the timestamp for this marker.
        /// </summary>
        public DateTime Timestamp
        {
            get => this.timestamp;
            private set => this.Set(() => this.Timestamp, ref this.timestamp, value);
        }

        /// <summary>
        /// Gets the timestamp string for this marker.
        /// </summary>
        public string TimestampString
        {
            get
            {
                if (this.Timestamp.Date == DateTime.Now.Date)
                {
                    // today, show just the time
                    return this.Timestamp.ToShortTimeString();
                }
                else if (this.Timestamp > DateTime.Now.AddDays(-6))
                {
                    // this week(ish), show "Day, Time" (Wed 12:34 PM)
                    string[] names = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames;
                    var day = names[(int)this.Timestamp.DayOfWeek];

                    return $"{day} {this.Timestamp.ToShortTimeString()}";
                }
                else
                {
                    // full format
                    return this.Timestamp.ToString("MMM d, yyyy h:mm tt");
                }
            }
        }

        /// <inheritdoc/>
        void IDisposable.Dispose()
        {
        }
    }
}
