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
    public class ListUpdateSettings : ConfigurationSection
    {
        private static Configuration _configuration = null;
        private static ListUpdateSettings _settings = null;

        private static readonly string blankListUpdateConfigurationFile = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
	<configSections>
		<section name=""ListUpdateSettings"" type=""Sift.Resources.Settings.ListUpdateSettings, Sift.Resources"" />
	</configSections>

	<ListUpdateSettings>
		<listUpdates>
        </listUpdates>
	</ListUpdateSettings>
</configuration>
";

        private static string configurationFilename = Resources.Constants.InstallPath + Resources.Constants.ListUpdatesConfigurationFilename;

        public static string ConfigurationFilename
        {
            get
            {
                return configurationFilename;
            }
        }

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
                return GetConfigurationFailsafe(configurationFileMap,--maximumTryCount);
            }
        }

        private static Configuration ConfigurationSettings
        {
            get
            {
                if (_configuration == null)
                {
                    #region Create the directory and blank configuration file if they don't exist
                    if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(configurationFilename)))
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(configurationFilename));

                    if (!System.IO.File.Exists(configurationFilename))
                    {
                        System.Xml.XmlDocument document = new System.Xml.XmlDocument();
                        document.LoadXml(blankListUpdateConfigurationFile);
                        document.Save(configurationFilename);
                    }
                    #endregion

                    ExeConfigurationFileMap configurationFileMap = new ExeConfigurationFileMap();
                    configurationFileMap.ExeConfigFilename = configurationFilename;                    
                    _configuration = GetConfigurationFailsafe(configurationFileMap, 30);
                    ConfigurationManager.RefreshSection("ListUpdateSettings");
                }

                return _configuration;
            }
            set
            {
                _configuration = value;
            }
        }

        public static ListUpdateSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    ConfigurationManager.RefreshSection("ListUpdateSettings");
                    _settings = (ListUpdateSettings)ConfigurationSettings.Sections["ListUpdateSettings"];
                }                
                return _settings;
            }
        }

        [ConfigurationProperty("listUpdates")]
        public ListUpdateCollection ListsUpdates
        {
            get
            {
                return (ListUpdateCollection)Settings["listUpdates"];
            }
        }

        public static void Open(string filename)
        {
            configurationFilename = filename;
            ConfigurationSettings = null;
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

            //// list updates are a write once and forget it operation            
            //Close(); 
        }

        public static void Refresh()
        {
            /// manually refresh the configuration file, if we did not and the configuration file has changed
            /// and exception would be thrown stating that the file had been changed
            ListUpdate[] currentListUpdates = new ListUpdate[Settings.ListsUpdates.Count];
            Settings.ListsUpdates.CopyTo(currentListUpdates, 0);            

            Close();

            Settings.ListsUpdates.Clear();
            foreach (ListUpdate listUpdate in currentListUpdates)
                Settings.ListsUpdates.Add(listUpdate);            
        }

        public static void RejectChanges()
        {
            if (_settings != null)
                _settings.Reset((ConfigurationElement)Settings["listUpdates"]);

            _settings = null;
            _configuration = null;
            
            ExeConfigurationFileMap configurationFileMap = new ExeConfigurationFileMap();
            configurationFileMap.ExeConfigFilename = configurationFilename;
            ConfigurationSettings = ConfigurationManager.OpenMappedExeConfiguration(configurationFileMap, ConfigurationUserLevel.None);
            ConfigurationManager.RefreshSection("ListUpdateSettings");
            _settings = (ListUpdateSettings)ConfigurationSettings.Sections["ListUpdateSettings"];
        }

        public static void Close()
        {
            if (_settings != null)
                _settings.ListsUpdates.Clear();

            _settings = null;
            _configuration = null;
         
        }
    }
}
