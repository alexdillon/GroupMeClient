using System;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace GroupMeClient.Core.ViewModels.Controls
{
    /// <summary>
    /// <see cref="LoadingControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.LoadingControl"/> control.
    /// </summary>
    public class LoadingControlViewModel : ObservableObject
    {
        private string message;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadingControlViewModel"/> class.
        /// </summary>
        public LoadingControlViewModel()
        {
        }

        /// <summary>
        /// Gets or sets the progress message.
        /// </summary>
        public string Message
        {
            get => this.message;
            set => this.SetProperty(ref this.message, value);
        }
    }
}
