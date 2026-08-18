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
    public class ListGroup : ConfigurationElement
    {
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

        [ConfigurationProperty("Description", IsRequired = false)]
        public string Description
        {
            get
            {
                return (string)this["Description"];
            }
            set
            {
                this["Description"] = value;
            }
        }

        [ConfigurationProperty("Details", IsRequired = false)]
        public string Details
        {
            get
            {
                return (string)this["Details"];
            }
            set
            {
                this["Details"] = value;
            }
        }

        [ConfigurationProperty("Enabled", IsRequired = false)]
        public bool Enabled
        {
            get
            {
                return (bool)this["Enabled"];
            }
            set
            {
                this["Enabled"] = value;
            }
        }

        [ConfigurationProperty("Log", DefaultValue=true, IsRequired = false)]
        public bool Log
        {
            get
            {
                return (bool)this["Log"];
            }
            set
            {
                this["Log"] = value;
            }
        }

        [ConfigurationProperty("lists")]
        public ListCollection Lists
        {
            get
            {
                return (ListCollection)this["lists"];
            }
            set
            {
                this["lists"] = value;
            }
        }

        [ConfigurationProperty("listGroups")]
        public ListGroupCollection ListGroups
        {
            get
            {
                return (ListGroupCollection)this["listGroups"];
            }
            set
            {
                this["listGroups"] = value;
            }
        }

        public List GetByListId(Guid id)
        {
            List list = Lists.GetByListId(id);

            if (list != null)
                return list;

            foreach (ListGroup listGroup in ListGroups)
            {
                list = listGroup.GetByListId(id);
                if (list != null)
                    return list;
            }

            return null;
        }

        public ListGroup GetByListGroupId(Guid id)
        {
            ListGroup matchingListGroup = null;

            if (this.Id == id)
                return this;

            foreach (ListGroup listGroup in ListGroups)
            {
                matchingListGroup = listGroup.GetByListGroupId(id);
                if (matchingListGroup != null)
                    return matchingListGroup;
            }

            return null;
        }
    }
}
