/*
 *    Copyright 2007 Travis Jones
 * 
 *    This file is part of SIFT.
 *
 *    SIFT is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    SIFT is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with SIFT.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Configuration;

namespace Sift.Resources.Settings
{
    public class AdapterSettings : ConfigurationSection
    {
        private static Configuration _configuration = null;
        private static AdapterSettings _settings = null;

        private static string configurationFilename = Resources.Constants.InstallPath + Resources.Constants.ConfigurationFilename;

        /// <summary>
        /// This method handles a condition where the service or application is reading the file and has it locked down.
        /// It will continue to try after a period of delays, where hopefully the file will get unlocked.
        /// </summary>
        /// <param name="maximumTryCount"></param>
        /// <returns></returns>
        private static Configuration GetConfigurationFailsafe(ExeConfigurationFileMap configurationFileMap, int maximumTryCount)
        {
            try
            {
                return ConfigurationManager.OpenMappedExeConfiguration(configurationFileMap, ConfigurationUserLevel.None);
            }
            catch (ConfigurationErrorsException)
            {
                System.Threading.Thread.Sleep(100);
                return GetConfigurationFailsafe(configurationFileMap, --maximumTryCount);
            }
        }

        private static Configuration ConfigurationSettings
        {
            get
            {
                if (_configuration == null)
                {
                    ExeConfigurationFileMap configurationFileMap = new ExeConfigurationFileMap();
                    configurationFileMap.ExeConfigFilename = configurationFilename;
                    _configuration = GetConfigurationFailsafe(configurationFileMap, 30);
                    ConfigurationManager.RefreshSection("AdapterSettings");
                }

                return _configuration;
            }
            set
            {
                _configuration = value;
            }
        }

        public static AdapterSettings Settings
        {
            get
            {
                if (_settings == null)
                {                                        
                    ConfigurationManager.RefreshSection("AdapterSettings");
                    _settings = (AdapterSettings)ConfigurationSettings.Sections["AdapterSettings"];                    
                }
                return _settings;
            }
        }               

        [ConfigurationProperty("adapters")]
        public AdapterSettingCollection Adapters
        {
            get
            {
                return (AdapterSettingCollection)Settings["adapters"];
            }
        }

        public static void Open(string filename)
        {
            if (filename != configurationFilename)
            {
                // close out any previously loaded configuration
                Close();
                configurationFilename = filename;
                ConfigurationSettings = null;                               
            }
        }

        public static void Save()
        {
            try
            {
                ConfigurationSettings.Save();
            }
            catch (ConfigurationErrorsException)
            {
                // the configuration file has changed, refresh the settings and save again
                Refresh();
                ConfigurationSettings.Save();
            }
        }

        public static void Refresh()
        {
            AdapterSetting[] currentAdapterSettings = new AdapterSetting[Settings.Adapters.Count];
            Settings.Adapters.CopyTo(currentAdapterSettings, 0);

            Close();

            foreach (AdapterSetting adapterSettings in currentAdapterSettings)
            {
                AdapterSetting fileAdapter = Settings.Adapters.GetByAdapterId(adapterSettings.Id);

                if (fileAdapter != null)
                {
                    // replace any adapter that is in the file
                    // by only replacing, we leave newly added adapters alone
                    Settings.Adapters.Remove(fileAdapter);
                    Settings.Adapters.Add(adapterSettings);
                }
            }
            
        }

        public static void RejectChanges()
        {
            if (_settings != null)
                _settings.Reset((ConfigurationElement)Settings["adapters"]);

            ExeConfigurationFileMap configurationFileMap = new ExeConfigurationFileMap();
            configurationFileMap.ExeConfigFilename = configurationFilename;
            ConfigurationSettings = ConfigurationManager.OpenMappedExeConfiguration(configurationFileMap, ConfigurationUserLevel.None);
            ConfigurationManager.RefreshSection("AdapterSettings");
            _settings = (AdapterSettings)ConfigurationSettings.Sections["AdapterSettings"];
        }

        public static void Close()
        {
            if (_settings != null)
                _settings.Adapters.Clear();

            _settings = null;
            _configuration = null;
        }
    }
}
