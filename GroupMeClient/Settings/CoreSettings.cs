using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupMeClient.Settings
{
    /// <summary>
    /// <see cref="CoreSettings"/> defines the settings needed for basic operation.
    /// </summary>
    public class CoreSettings
    {
        /// <summary>
        /// Gets or sets the authorization token used for GroupMe Api Operations.
        /// </summary>
        public string AuthToken { get; set; }
    }
}
