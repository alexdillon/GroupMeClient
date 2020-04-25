using GalaSoft.MvvmLight;

namespace GroupMeClient.ViewModels.Controls.Attachments
{
    /// <summary>
    /// <see cref="AttachmentViewModelBase"/> provides the base type for all <see cref="Attachments"/>.
    /// </summary>
    public class AttachmentViewModelBase : ViewModelBase
    {
        /// <summary>
        /// Gets a value indicating whether this attachment is allowed to be shrunk when it is displayed.
        /// </summary>
        public virtual bool Shrinkable => true;
    }
}
