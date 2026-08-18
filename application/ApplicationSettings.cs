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
using System.Security.Cryptography;

namespace Sift.Application
{
    public class ApplicationSettings : ConfigurationSection
    {
        private static Configuration _configuration = null;
        private static ApplicationSettings _settings = null;

        private static string configurationFilename = System.Windows.Forms.Application.StartupPath + @"\Sift.exe.config";

        private static Configuration ConfigurationSettings
        {
            get
            {
                if (_configuration == null)
                {
                    ExeConfigurationFileMap configurationFileMap = new ExeConfigurationFileMap();
                    configurationFileMap.ExeConfigFilename = configurationFilename;
                    _configuration = ConfigurationManager.OpenMappedExeConfiguration(configurationFileMap, ConfigurationUserLevel.None);
                }

                return _configuration;
            }
            set
            {
                _configuration = value;
            }
        }

        public static ApplicationSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    ConfigurationManager.RefreshSection("ApplicationSettings");
                    _settings = (ApplicationSettings)ConfigurationSettings.Sections["ApplicationSettings"];
                }
                return _settings;
            }
        }

        [ConfigurationProperty("IsFirstStartup", DefaultValue = true, IsRequired = false)]
        public bool IsFirstStartup
        {
            get
            {
                return (bool)this["IsFirstStartup"];
            }
            set
            {
                this["IsFirstStartup"] = value;
            }
        }

        [ConfigurationProperty("LastUpdatePoll", DefaultValue = "01/01/2007 12:00PM", IsRequired = false)]
        public DateTime LastUpdatePoll
        {
            get
            {
                return (DateTime)this["LastUpdatePoll"];
            }
            set
            {
                this["LastUpdatePoll"] = value;
            }
        }

        [ConfigurationProperty("RemotingHostName", DefaultValue = "localhost", IsRequired = false)]
        public string RemotingHostName
        {
            get
            {
                return (string)this["RemotingHostName"];
            }
            set
            {
                this["RemotingHostName"] = value;
            }
        }

        [ConfigurationProperty("RemotingPort", DefaultValue = "8080", IsRequired = false)]
        public int RemotingPort
        {
            get
            {
                return (int)this["RemotingPort"];
            }
            set
            {
                this["RemotingPort"] = value;
            }
        }

        [ConfigurationProperty("RemotingUsername", IsRequired = false)]
        public string RemotingUsername
        {
            get
            {
                return (string)this["RemotingUsername"];
            }
            set
            {
                this["RemotingUsername"] = value;
            }
        }

        [ConfigurationProperty("RemotingDomain", IsRequired = false)]
        public string RemotingDomain
        {
            get
            {
                return (string)this["RemotingDomain"];
            }
            set
            {
                this["RemotingDomain"] = value;
            }
        }

        [ConfigurationProperty("RemotingPassword", IsRequired = false)]
        public string RemotingPassword
        {
            get
            {
                string value = (string)this["RemotingPassword"];

                if (value == string.Empty)
                    return value;
                else
                    return Decrypt(value);
            }
            set
            {
                if (value == string.Empty)
                    this["RemotingPassword"] = value;
                else
                    this["RemotingPassword"] = Encrypt(value);
            }
        }

        public static void Open(string filename)
        {
            configurationFilename = filename;
            ConfigurationSettings = null;
        }

        public static void Save()
        {
            _settings.SectionInformation.ForceSave = true;
            ConfigurationSettings.Save();
        }


        public static void RejectChanges()
        {
            if (_settings != null)
            {
                ExeConfigurationFileMap configurationFileMap = new ExeConfigurationFileMap();
                configurationFileMap.ExeConfigFilename = configurationFilename;
                ConfigurationSettings = ConfigurationManager.OpenMappedExeConfiguration(configurationFileMap, ConfigurationUserLevel.None);
                ConfigurationManager.RefreshSection("ApplicationSettings");
                _settings = (ApplicationSettings)ConfigurationSettings.Sections["ApplicationSettings"];
            }
        }

        private static string Encrypt(string value)
        {
            DESCryptoServiceProvider encryptor = new DESCryptoServiceProvider();
            encryptor.Padding = PaddingMode.PKCS7;
            byte[] valueBytes = System.Text.Encoding.UTF8.GetBytes(value);
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor.CreateEncryptor(System.Text.Encoding.UTF8.GetBytes(Resources.Constants.CryptographyKey), Resources.Constants.CryptographyIV), CryptoStreamMode.Write);
            cryptoStream.Write(valueBytes, 0, valueBytes.Length);
            cryptoStream.FlushFinalBlock();
            return Convert.ToBase64String(memoryStream.ToArray());
        }

        private static string Decrypt(string value)
        {
            byte[] valueBytes = new byte[value.Length];
            
            DESCryptoServiceProvider encryptor = new DESCryptoServiceProvider();
            encryptor.Padding = PaddingMode.PKCS7;
            valueBytes = Convert.FromBase64String(value);
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor.CreateDecryptor(System.Text.Encoding.UTF8.GetBytes(Resources.Constants.CryptographyKey), Resources.Constants.CryptographyIV), CryptoStreamMode.Write);
            cryptoStream.Write(valueBytes, 0, valueBytes.Length);
            cryptoStream.FlushFinalBlock();
            System.Text.Encoding encoding = System.Text.Encoding.UTF8;            
            return encoding.GetString(memoryStream.ToArray());
        }
    }
}
