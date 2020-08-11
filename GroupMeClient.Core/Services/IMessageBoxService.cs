using System;
using System.Collections.Generic;
using System.Text;

namespace GroupMeClient.Core.Services
{
    /// <summary>
    /// <see cref="IMessageRendererService"/> provides a service for displaying platform dependent Message Boxes.
    /// </summary>
    public interface IMessageBoxService
    {
        void ShowMessageBox(MessageBoxParams paramters);
    }
}
