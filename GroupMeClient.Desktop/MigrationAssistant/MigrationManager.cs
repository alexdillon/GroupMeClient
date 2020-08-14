using System;
using System.Collections.Generic;
using System.Text;
using GalaSoft.MvvmLight.Ioc;
using GroupMeClient.Core;
using GroupMeClient.Core.Settings;

namespace GroupMeClient.Desktop.MigrationAssistant
{
    public class MigrationManager
    {
        public static bool EnsureMigration(Startup.StartupParameters startupParameters)
        {
            var settingsManager = SimpleIoc.Default.GetInstance<SettingsManager>();
            if (settingsManager.CoreSettings.MigrationVersion < 1)
            {
                var migrator = new MigrationGMDC30();
                return migrator.DoMigration(startupParameters);
            }

            return true;
        }
    }
}
