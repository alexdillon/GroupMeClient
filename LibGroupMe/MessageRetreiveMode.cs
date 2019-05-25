namespace LibGroupMe
{
    /// <summary>
    /// Specifies which subset of messages should be returned.
    /// </summary>
    public enum MessageRetreiveMode
    {
        /// <summary>
        /// No filtering is applied
        /// </summary>
        None,

        /// <summary>
        /// Return messages immediately preceding the given message will be returned, in descending order
        /// </summary>
        BeforeId,

        /// <summary>
        /// Return messages that immediately follow a given message, this time in ascending order
        /// This mode is not supported for Direct Messages (Chats)
        /// </summary>
        AfterId,

        /// <summary>
        /// Return messages created after the given message, but it retrieves the most recent messages
        /// </summary>
        SinceId
    }
}
