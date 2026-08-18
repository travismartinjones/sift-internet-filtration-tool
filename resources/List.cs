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
    public class List : ConfigurationElement
    {
        #region CONFIGURATION PROPERTIES

        public string Path
        {
            get
            {
                return Resources.Settings.ListSettings.Settings.Path + this.Id.ToString() + ".lis";
                //return this.Id.ToString() + ".lis";
            }
        }

        [ConfigurationProperty("Id", IsRequired = true)]
        public Guid Id
        {
            get
            {
                return (Guid)this["Id"];
            }
            set
            {
                this["Id"] = value;
            }
        }

        [ConfigurationProperty("Content", IsRequired = true)]
        public Sift.Resources.Types.ContentType Content
        {
            get
            {
                Sift.Resources.Types.ContentType? type;
                type = (Sift.Resources.Types.ContentType)Enum.Parse(typeof(Sift.Resources.Types.ContentType), this["Content"].ToString());
                return type.Value;
            }
            set
            {
                this["Content"] = value.ToString();
            }
        }

        [ConfigurationProperty("MatchAction", IsRequired = false,DefaultValue=Resources.Types.MatchActionType.Block)]
        public Sift.Resources.Types.MatchActionType MatchAction
        {
            get
            {
                Sift.Resources.Types.MatchActionType? type;
                type = (Sift.Resources.Types.MatchActionType)Enum.Parse(typeof(Sift.Resources.Types.MatchActionType), this["MatchAction"].ToString());
                if (type.HasValue)
                    return type.Value;
                else
                    return Sift.Resources.Types.MatchActionType.Block;
            }
            set
            {
                this["MatchAction"] = value;
            }
        }
        
        [ConfigurationProperty("Description", IsRequired = false, DefaultValue="List File")]
        public string Description
        {
            get
            {
                return (string)this["Description"];
            }
            set
            {
                this["Description"] = (string)value;
            }
        }

        [ConfigurationProperty("Enabled", IsRequired = false, DefaultValue = true)]
        public bool Enabled
        {
            get
            {
                return (bool)this["Enabled"];
            }
            set
            {
                this["Enabled"] = (bool)value;
            }
        }
        
        [ConfigurationProperty("LastUpdated", IsRequired = false)]
        public DateTime LastUpdated
        {
            get
            {
                return (DateTime)this["LastUpdated"];
            }
            set
            {
                this["LastUpdated"] = (DateTime)value;
            }
        }

        #endregion

        #region METHODS
        
        /// <summary>
        /// Used this helper method get the list file path resolved relative to the sift service when the List object cannot be obtained.
        /// </summary>
        /// <remarks>
        /// This method is less ideal than the configuration property, yet is needed in order to always be able to determine the list
        /// filename. Even in the case when the configuration list entry does not exist, such as when a list is being deleted from 
        /// the sift application.
        /// </remarks>
        /// <param name="id"></param>
        /// <returns>The full path of the list file.</returns>
        public static string GetPathByListId(Guid id)
        {
            return Resources.Settings.ListSettings.Settings.Path + id.ToString() + ".lis";
            //return id.ToString() + ".lis";
        }

        #endregion
    }
}
