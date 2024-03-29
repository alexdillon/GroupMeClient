﻿using System;
using GroupMeClient.Core.Messaging;
using GroupMeClient.Core.ViewModels.Controls;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

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
        /// <param name="tag">A unique value tagging this window.</param>
        public WindowHostViewModel(object content, string tag = "")
        {
            this.Content = content;
            this.Tag = tag;

            this.DialogManagerRegular = new PopupViewModel()
            {
                EasyClosePopupCallback = new RelayCommand(this.CloseBigPopup),
                ClosePopupCallback = new RelayCommand(this.CloseBigPopup),
            };

            this.DialogManagerTopMost = new PopupViewModel()
            {
                EasyClosePopupCallback = new RelayCommand(this.CloseBigTopMostPopup),
                ClosePopupCallback = new RelayCommand(this.CloseBigTopMostPopup),
            };

            WeakReferenceMessenger.Default.Register<WindowHostViewModel, DialogRequestMessage>(this, (r, m) => r.OpenBigPopup(m));
            WeakReferenceMessenger.Default.Register<WindowHostViewModel, DialogDismissMessage>(this, (r, m) => r.DismissCallback(m));
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

        /// <summary>
        /// Gets a value indicating whether this ViewModel is represents a complete copy of GMDC.
        /// </summary>
        public bool IsFullGMDC => false;

        private string Tag { get; }

        private void OpenBigPopup(DialogRequestMessage dialog)
        {
            if (this.Tag == dialog.Destination || string.IsNullOrEmpty(this.Tag))
            {
                if (dialog.TopMost)
                {
                    this.DialogManagerTopMost.OpenPopup(dialog.Dialog, dialog.DialogId);
                }
                else
                {
                    this.DialogManagerRegular.OpenPopup(dialog.Dialog, dialog.DialogId);
                }
            }
        }

        private void CloseBigPopup()
        {
            if (this.DialogManagerRegular.PopupDialog is IDisposable d)
            {
                d.Dispose();
            }

            var closeId = this.DialogManagerRegular.PopupId;
            this.DialogManagerRegular.ClosePopup();
            WeakReferenceMessenger.Default.Send(new DialogDismissMessage(closeId));
        }

        private void CloseBigTopMostPopup()
        {
            if (this.DialogManagerTopMost.PopupDialog is IDisposable d)
            {
                d.Dispose();
            }

            var closeId = this.DialogManagerTopMost.PopupId;
            this.DialogManagerTopMost.ClosePopup();
            WeakReferenceMessenger.Default.Send(new DialogDismissMessage(closeId));
        }

        private void DismissCallback(DialogDismissMessage dismissMessage)
        {
            if (this.DialogManagerTopMost.PopupId == dismissMessage.DialogId && dismissMessage.DialogId != Guid.Empty)
            {
                this.CloseBigTopMostPopup();
            }
            else if (this.DialogManagerRegular.PopupId == dismissMessage.DialogId && dismissMessage.DialogId != Guid.Empty)
            {
                this.CloseBigPopup();
            }
        }
    }
}
