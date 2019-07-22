using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GroupMeClient.Settings
{
    /// <summary>
    /// <see cref="SettingsManager"/> provides runtime methods to access and save parameters for modules that
    /// form the GroupMe Desktop Client UI.
    /// </summary>
    public class SettingsManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsManager"/> class.
        /// </summary>
        /// <param name="databaseName">The name of the database file to open.</param>
        public SettingsManager(string databaseName)
        {
            this.SettingsFile = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
        }

        /// <summary>
        /// Gets or sets the core settings instance.
        /// </summary>
        public CoreSettings CoreSettings { get; set; } = new CoreSettings();

        private string SettingsFile { get; set; }

        /// <summary>
        /// Reads the configuration file.
        /// </summary>
        public void LoadSettings()
        {
            if (System.IO.File.Exists(this.SettingsFile))
            {
                string json = System.IO.File.ReadAllText(this.SettingsFile);
                JsonConvert.PopulateObject(json, this);
            }
        }

        /// <summary>
        /// Saves the configuration file.
        /// </summary>
        public void SaveSettings()
        {
            var json = JsonConvert.SerializeObject(this);
            System.IO.File.WriteAllText(this.SettingsFile, json);
        }
    }
}
