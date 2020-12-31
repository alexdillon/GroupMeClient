using System;
using System.Windows.Controls;
using System.Windows.Documents;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GroupMeClient.Core.Services;

namespace GroupMeClient.WpfUI.Extensions
{
    /// <summary>
    /// <see cref="CopyableHyperlink"/> is a <see cref="Hyperlink"/> control with support for copying the URL.
    /// </summary>
    public class CopyableHyperlink : Hyperlink
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CopyableHyperlink"/> class.
        /// </summary>
        /// <param name="childElement">The contents of this link.</param>
        public CopyableHyperlink(Inline childElement)
            : base(childElement)
        {
            this.ContextMenu = new ContextMenu()
            {
                Items =
                {
                    new MenuItem()
                    {
                        Header = "Copy URL",
                        Command = new RelayCommand(this.CopyUrl),
                    },
                },
            };
        }

        private void CopyUrl()
        {
            var clipboardService = SimpleIoc.Default.GetInstance<IClipboardService>();
            clipboardService.CopyText(this.NavigateUri.ToString());
        }
    }
}
