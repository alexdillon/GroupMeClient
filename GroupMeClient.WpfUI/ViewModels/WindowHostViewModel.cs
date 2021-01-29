using System;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GroupMeClient.Core.ViewModels.Controls;

namespace GroupMeClient.WpfUI.ViewModels
{
    /// <summary>
    /// <see cref="WindowHostViewModel"/> provides a wrapper for top-level content in a window
    /// that allows for displaying multiple layers of modal in-app popups above it.
    /// </summary>
    public class WindowHostViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowHostViewModel"/> class.
        /// </summary>
        /// <param name="content">The content to display in this window.</param>
        public WindowHostViewModel(object content)
        {
            this.Content = content;

            this.DialogManagerRegular = new PopupViewModel()
            {
                EasyClosePopup = new RelayCommand(this.CloseBigPopup),
                ClosePopup = new RelayCommand(this.CloseBigPopup),
                PopupDialog = null,
            };

            this.DialogManagerTopMost = new PopupViewModel()
            {
                EasyClosePopup = new RelayCommand(this.CloseBigTopMostPopup),
                ClosePopup = new RelayCommand(this.CloseBigTopMostPopup),
                PopupDialog = null,
            };

            Messenger.Default.Register<Core.Messaging.DialogRequestMessage>(this, this.OpenBigPopup);
        }

        /// <summary>
        /// Gets the content being displayed in this window.
        /// </summary>
        public object Content { get; }

        /// <summary>
        /// Gets or sets the manager for the dialog that should be displayed as a large popup.
        /// </summary>
        public PopupViewModel DialogManagerRegular { get; set; }

        /// <summary>
        /// Gets or sets the manager for the dialog that should be displayed as a large topmost popup.
        /// </summary>
        public PopupViewModel DialogManagerTopMost { get; set; }

        private void OpenBigPopup(Core.Messaging.DialogRequestMessage dialog)
        {
            if (dialog.TopMost)
            {
                this.DialogManagerTopMost.PopupDialog = dialog.Dialog;
            }
            else
            {
                this.DialogManagerRegular.PopupDialog = dialog.Dialog;
            }
        }

        private void CloseBigPopup()
        {
            if (this.DialogManagerRegular.PopupDialog is IDisposable d)
            {
                d.Dispose();
            }

            this.DialogManagerRegular.PopupDialog = null;
        }

        private void CloseBigTopMostPopup()
        {
            if (this.DialogManagerTopMost.PopupDialog is IDisposable d)
            {
                d.Dispose();
            }

            this.DialogManagerTopMost.PopupDialog = null;
        }
    }
}
