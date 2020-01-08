using System;
using System.Windows.Media;
using GroupMeClientApi.Models;

namespace GroupMeClient.ViewModels.Controls
{
    /// <summary>
    /// <see cref="InlineReadSentMarkerControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.InlineReadSentMarkerControl"/> control.
    /// </summary>
    public class InlineReadSentMarkerControlViewModel : MessageControlViewModelBase, IDisposable
    {
        private DateTime timestamp;
        private bool showRead;

        /// <summary>
        /// Initializes a new instance of the <see cref="InlineReadSentMarkerControlViewModel"/> class.
        /// </summary>
        /// <param name="timestamp">The timestamp to display.</param>
        /// <param name="showRead">Indicates whether this control displays a Read or Sent messge.</param>
        /// <param name="id">The message ID to show the timestamp near.</param>
        /// <param name="didISendIt">Whether the timestamp corresponds to a message sent by the current user.</param>
        public InlineReadSentMarkerControlViewModel(DateTime timestamp, bool showRead, string id, bool didISendIt)
        {
            this.Timestamp = timestamp;
            this.ShowRead = showRead;
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
            get { return this.timestamp; }
            private set { this.Set(() => this.Timestamp, ref this.timestamp, value); }
        }

        /// <summary>
        /// Gets a value indicating whether this marker shows a 'Read' message.
        /// </summary>
        public bool ShowRead
        {
            get { return this.showRead; }
            private set { this.Set(() => this.ShowRead, ref this.showRead, value); }
        }

        /// <summary>
        /// Gets the label string for this marker.
        /// </summary>
        public string LabelString
        {
            get
            {
                string timeString;
                if (this.Timestamp.Date == DateTime.Now.Date)
                {
                    // today, show just the time
                    timeString = this.Timestamp.ToShortTimeString();
                }
                else if (this.Timestamp > DateTime.Now.AddDays(-6))
                {
                    // this week(ish), show "Day, Time" (Wed 12:34 PM)
                    string[] names = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames;
                    var day = names[(int)this.Timestamp.DayOfWeek];

                    timeString = $"{day} {this.Timestamp.ToShortTimeString()}";
                }
                else
                {
                    // full format
                    timeString = this.Timestamp.ToString("MMM dd h:mm tt");
                }

                if (this.ShowRead)
                {
                    return $"Read - {timeString}";
                }
                else
                {
                    return $"Message Sent - {timeString}";
                }
            }
        }

        /// <inheritdoc/>
        void IDisposable.Dispose()
        {
        }
    }
}
