using System;
using GroupMeClient.Core;
using GroupMeClient.Core.Settings;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace GroupMeClient.Desktop.MigrationAssistant
{
    public class MigrationManager
    {
        public static bool EnsureMigration(StartupExtensions.StartupParameters startupParameters)
        {
            var settingsManager = Ioc.Default.GetService<SettingsManager>();
            if (settingsManager.CoreSettings.MigrationVersion < 1)
            {
                var migrator = new MigrationGMDC30();
                return migrator.DoMigration(startupParameters);
            }

            return true;
        }
    }
}
