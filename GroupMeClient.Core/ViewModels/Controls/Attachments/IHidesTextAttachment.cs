namespace GroupMeClient.Core.ViewModels.Controls.Attachments
{
    /// <summary>
    /// <see cref="IHidesTextAttachment"/> defines an interface for attachments that hide parts of the message text.
    /// </summary>
    public interface IHidesTextAttachment
    {
        /// <summary>
        /// Gets a string containing the portion of the message body that should be hidden.
        /// </summary>
        string HiddenText { get; }
    }
}
