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
    public class ListSettings : ConfigurationSection
    {
        private static Configuration _configuration = null;
        private static ListSettings _settings = null;

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
                    ConfigurationManager.RefreshSection("ListSettings");
                }

                return _configuration;
            }
            set
            {
                _configuration = value;
            }
        }

        public static ListSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    ConfigurationManager.RefreshSection("ListSettings");
                    _settings = (ListSettings)ConfigurationSettings.Sections["ListSettings"];                    
                }
                return _settings;                
            }
        }

        [ConfigurationProperty("listGroups")]
        public ListGroupCollection ListGroups
        {
            get
            {
                return (ListGroupCollection)Settings["listGroups"];
            }
        }

        [ConfigurationProperty("Path", DefaultValue="", IsRequired = false)]
        public string Path
        {
            get
            {
                string path = (string)this["Path"];

                // the path has not been set, the value was either removed, or the application is starting for the first time
                if (path == "")
                {
                    path = Resources.Constants.InstallPath + "lists/";
                    this["Path"] = path;
                }

                return path;
            }
            set
            {
                this["Path"] = value;
            }
        }

        /// <summary>
        /// High level list search. Useful for quickly finding a list in any list group.
        /// </summary>
        /// <param name="id">The unique identifier of the list.</param>
        /// <returns></returns>
        public List GetByListId(Guid id)
        {
            foreach (ListGroup listGroup in ListGroups)
            {
                List list = listGroup.GetByListId(id);
                
                if (list != null)
                    return list;
            }

            return null;
        }

        public ListGroup GetByListGroupId(Guid id)
        {
            ListGroup matchingListGroup = null;

            foreach (ListGroup listGroup in ListGroups)
            {
                matchingListGroup = listGroup.GetByListGroupId(id);

                if (matchingListGroup != null)
                    return matchingListGroup;
            }

            return null;
        }

        public Sift.Resources.Settings.ListCollection FlattenLists(Sift.Resources.Settings.ListGroupCollection listGroups)
        {
            Sift.Resources.Settings.ListCollection flattenedLists = new Sift.Resources.Settings.ListCollection();

            foreach (Sift.Resources.Settings.ListGroup listGroup in listGroups)
            {
                foreach(Sift.Resources.Settings.List list in listGroup.Lists)
                    flattenedLists.Add(list);

                Sift.Resources.Settings.ListCollection subLists = FlattenLists(listGroup.ListGroups);

                foreach (Sift.Resources.Settings.List subList in subLists)
                    flattenedLists.Add(subList);
            }

            return flattenedLists;
        }

        public Sift.Resources.Settings.ListGroupCollection FlattenListGroups(Sift.Resources.Settings.ListGroupCollection listGroups)
        {
            Sift.Resources.Settings.ListGroupCollection flattenedListGroups = new Sift.Resources.Settings.ListGroupCollection();

            foreach (Sift.Resources.Settings.ListGroup listGroup in listGroups)
            {
                flattenedListGroups.Add(listGroup);

                Sift.Resources.Settings.ListGroupCollection subListGroups = FlattenListGroups(listGroup.ListGroups);

                foreach (Sift.Resources.Settings.ListGroup subListGroup in subListGroups)
                    flattenedListGroups.Add(subListGroup);
            }

            return flattenedListGroups;
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
            catch
            {
                // the configuration file has changed, refresh the settings and save again
                Refresh();
                ConfigurationSettings.Save();
            }
        }

        /// <summary>
        /// Forces the configuration manager to re-read from the configuration file. All list changes that had
        /// been previously saved will be retained. Only newly added entries will be loaded.
        /// </summary>
        public static void Refresh()
        {
            /// manually refresh the configuration file, if we did not and the configuration file has changed
            /// and exception would be thrown stating that the file had been changed

            
            ListGroup [] currentListGroups = new ListGroup[Settings.ListGroups.Count];
            Settings.ListGroups.CopyTo(currentListGroups, 0);

            string currentPath = Settings.Path;

            Close();

            Settings.ListGroups.Clear();
            foreach (ListGroup listGroup in currentListGroups)
                Settings.ListGroups.Add(listGroup);

            Settings.Path = currentPath;
        }

        public static void RejectChanges()
        {            
            if (_settings != null)
                _settings.Reset((ConfigurationElement)Settings["listGroups"]);

            ExeConfigurationFileMap configurationFileMap = new ExeConfigurationFileMap();
            configurationFileMap.ExeConfigFilename = configurationFilename;
            ConfigurationSettings = ConfigurationManager.OpenMappedExeConfiguration(configurationFileMap, ConfigurationUserLevel.None);
            ConfigurationManager.RefreshSection("ListSettings");
            _settings = (ListSettings)ConfigurationSettings.Sections["ListSettings"];            
        }

        /// <summary>
        /// Closes the configuration file and loses all modified list information.
        /// </summary>
        public static void Close()
        {
            if (_settings != null)
                _settings.ListGroups.Clear();

            _settings = null;
            _configuration = null;
        }
    }
}
