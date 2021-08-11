using System;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace GroupMeClient.Core.ViewModels.Controls
{
    /// <summary>
    /// <see cref="PopupViewModel"/> provides a ViewModel for the <see cref="GroupMeClient.Core.Views.Controls.Popup"/> control.
    /// </summary>
    public class PopupViewModel : ObservableObject
    {
        private ObservableObject popupDialog;
        private ICommand closePopupCallback;
        private ICommand easyClosePopupCallback;
        private bool isShowingDialog;

        /// <summary>
        /// Initializes a new instance of the <see cref="PopupViewModel"/> class.
        /// </summary>
        public PopupViewModel()
        {
            this.ClosePopup();
        }

        /// <summary>
        /// Gets the Popup Dialog that should be displayed.
        /// Null specifies that no popup is shown.
        /// </summary>
        public ObservableObject PopupDialog
        {
            get => this.popupDialog;
            private set
            {
                this.SetProperty(ref this.popupDialog, value);
                this.IsShowingDialog = this.PopupDialog != null;
            }
        }

        /// <summary>
        /// Gets the identifier of the dialog being displayed.
        /// </summary>
        public Guid PopupId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a dialog is being shown.
        /// </summary>
        public bool IsShowingDialog
        {
            get => this.isShowingDialog;
            private set => this.SetProperty(ref this.isShowingDialog, value);
        }

        /// <summary>
        /// Gets or sets the action to be be performed when the big popup has been closed.
        /// </summary>
        public ICommand ClosePopupCallback
        {
            get => this.closePopupCallback;
            set => this.SetProperty(ref this.closePopupCallback, value);
        }

        /// <summary>
        /// Gets or sets the action to be be performed when the big popup has been closed indirectly.
        /// This typically is from the user clicking in the gray area around the popup to dismiss it.
        /// </summary>
        public ICommand EasyClosePopupCallback
        {
            get => this.easyClosePopupCallback;
            set => this.SetProperty(ref this.easyClosePopupCallback, value);
        }

        /// <summary>
        /// Displays a new popup.
        /// </summary>
        /// <param name="content">The dialog to show as a popup.</param>
        /// <param name="id">The dialog unique ID.</param>
        public void OpenPopup(ObservableObject content, Guid id)
        {
            this.PopupDialog = content;
            this.PopupId = id;
        }

        /// <summary>
        /// Closes the currently displayed dialog.
        /// </summary>
        public void ClosePopup()
        {
            this.PopupDialog = null;
            this.PopupId = Guid.Empty;
        }
    }
}
