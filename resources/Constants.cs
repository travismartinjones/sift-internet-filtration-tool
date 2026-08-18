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
using System.Collections.Generic;
using System.Text;

namespace Sift.Resources
{
    public class Constants
    {
        public static readonly string ServiceName = "Sift";
        public static readonly string RegistryPath = @"SOFTWARE\Sift";
        public static readonly string DriverName = @"\\.\\Sift"; // the name of the filter driver        
        
        public static readonly Guid AdapterDefaultSettingId = new Guid("C611928C-05DD-40B8-BBEE-8B4C17DBF034");
        public static readonly Guid ListGroupCustomId = new Guid("E52FBFDE-6580-4E94-B2AF-EC81A14DF597");
        public static readonly Guid ListGroupCustomQuickAddId = new Guid("B8A4373C-2F6A-4FBD-8AB7-5317FA8B4722");

        public static readonly DateTime ListFirstUpdateDate = DateTime.Parse("1/1/2000");

        public static readonly int RemotingPort = 8080;

        #region Configuration Filenames
        public static readonly string RemotingGetConfiguration = "RemotingGetConfiguration";
        public static readonly string ConfigurationFilename = "SiftService.exe.config";
        public static readonly string ConfigurationRemotingFilename = "SiftService.exe.config.remote";
        public static readonly string ListUpdatesConfigurationFilename = "ListUpdates.xml";
        public static readonly string ListUpdatesConfigurationRemotingFilename = "ListUpdates.xml.remote";
        #endregion

        #region Cyptography Constants
        public static readonly string CryptographyKey = "a24Cs1f+";
        public static readonly byte[] CryptographyIV = { 0XF4, 0X18, 0XA1, 0X5C, 0X60, 0X7B, 0X9D, 0XBF };//{ 0X12, 0X34, 0X56, 0X78, 0X90, 0XAB, 0XCD, 0XEF };
        #endregion

        #region Service Custom Command Constants
        public const int CustomCommandLoadAdapters = 128;
        public const int CustomCommandLoadLists = 129;        
        public const int CustomCommandDisable = 131;
        public const int CustomCommandEnable = 133;
        public const int CustomCommandLoadListUpdates = 132;        
        #endregion

        private static string _installPath = string.Empty;

        /// <summary>
        /// Gets the filter service installation path to locate settings files.
        /// </summary>
        /// <returns></returns>
        public static string InstallPath
        {
            get
            {
                if (_installPath == string.Empty)
                {
                    // pull the local install path from the registry
                    // this is used to load %path%/app.config
                    Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine;

                    key = key.OpenSubKey(Sift.Resources.Constants.RegistryPath, false);

                    if (key != null)
                        _installPath = key.GetValue("InstallDir").ToString();                    
                }

                return _installPath;
            }
        }
    }
}
