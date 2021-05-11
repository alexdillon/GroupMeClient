using System;
using System.Windows.Controls;
using System.Windows.Documents;
using GroupMeClient.Core.Services;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;

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
            var clipboardService = Ioc.Default.GetService<IClipboardService>();
            clipboardService.CopyText(this.NavigateUri.ToString());
        }
    }
}
