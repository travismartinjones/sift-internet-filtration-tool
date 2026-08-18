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
    public class LogSettings : ConfigurationSection
    {
        private static Configuration _configuration = null;
        private static LogSettings _settings = null;

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
                }

                return _configuration;
            }
            set
            {
                _configuration = value;
            }
        }

        public static LogSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    ConfigurationManager.RefreshSection("LogSettings");
                    _settings = (LogSettings)ConfigurationSettings.Sections["LogSettings"];
                }
                return _settings;                                
            }
        }

        [ConfigurationProperty("logLevel", DefaultValue = "None", IsRequired = false)]
        public Sift.Resources.Types.LogDetailType LogLevel
        {
            get 
            {
                Sift.Resources.Types.LogDetailType? type;
                type = (Sift.Resources.Types.LogDetailType)Enum.Parse(typeof(Sift.Resources.Types.LogDetailType), this["logLevel"].ToString());
                if (type.HasValue)
                    return type.Value;
                else
                    return Sift.Resources.Types.LogDetailType.None;
            }
            set
            {
                this["logLevel"] = value.ToString();
            }
        }

        [ConfigurationProperty("logDriver", DefaultValue = false, IsRequired = false)]
        public bool LogDriver
        {
            get
            {
                return (bool)this["logDriver"];
            }
            set
            {
                this["logDriver"] = value;
            }
        }

        [ConfigurationProperty("logService", DefaultValue = false, IsRequired = false)]
        public bool LogService
        {
            get
            {
                return (bool)this["logService"];
            }
            set
            {
                this["logService"] = value;
            }
        }

        [ConfigurationProperty("logListMatch", DefaultValue = false, IsRequired = false)]
        public bool LogListMatch
        {
            get
            {
                return (bool)this["logListMatch"];
            }
            set
            {
                this["logListMatch"] = value;
            }
        }

        [ConfigurationProperty("logListBlock", DefaultValue = false, IsRequired = false)]
        public bool LogListBlock
        {
            get
            {
                return (bool)this["logListBlock"];
            }
            set
            {
                this["logListBlock"] = value;
            }
        }

        [ConfigurationProperty("logListAllow", DefaultValue = false, IsRequired = false)]
        public bool LogListAllow
        {
            get
            {
                return (bool)this["logListAllow"];
            }
            set
            {
                this["logListAllow"] = value;
            }
        }

        [ConfigurationProperty("logDescisionBranch", DefaultValue = false, IsRequired = false)]
        public bool LogDescisionBranch
        {
            get
            {
                return (bool)this["logDescisionBranch"];
            }
            set
            {
                this["logDescisionBranch"] = value;
            }
        }

        public static void Open(string filename)
        {
            configurationFilename = filename;
            ConfigurationSettings = null;
        }

        public static void Save()
        {            
            ConfigurationSettings.Save();
        }
    }
}
