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
    public class ListUpdate : ConfigurationElement
    {
        [ConfigurationProperty("ListId", IsRequired = true)]
        public Guid ListId
        {
            get
            {
                return (Guid)this["ListId"];
            }
            set
            {
                this["ListId"] = value;
            }
        }

        [ConfigurationProperty("Action", IsRequired = false)]
        public Sift.Resources.Types.ListUpdateType Action
        {
            get
            {
                Sift.Resources.Types.ListUpdateType? type;
                type = (Sift.Resources.Types.ListUpdateType)Enum.Parse(typeof(Sift.Resources.Types.ListUpdateType), this["Action"].ToString());
                return type.Value;
            }
            set
            {
                this["Action"] = value.ToString();
            }
        }

        [ConfigurationProperty("listEntryUpdates")]
        public ListEntryUpdateCollection Updates
        {
            get
            {
                return (ListEntryUpdateCollection)this["listEntryUpdates"];
            }
            set
            {
                this["listEntryUpdates"] = value;
            }
        }
    }
}
