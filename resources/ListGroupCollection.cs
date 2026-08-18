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
    public class ListGroupCollection : ConfigurationElementCollection
    {
        public ListGroupCollection()
        {
            this.AddElementName = "listGroup";
        }

        public ListGroup GetByListGroupId(Guid id)
        {
            foreach (ListGroup listGroup in this)
            {
                if (listGroup.Id == id)
                    return listGroup;
            }

            ListGroup childMatch = null;

            /// recurse through the children
            foreach (ListGroup listGroup in this)
            {
                childMatch = listGroup.ListGroups.GetByListGroupId(id);

                if(childMatch != null)
                    return childMatch;
            }

            return null;
        }

        public ListGroup this[int index]
        {
            get
            {
                return (ListGroup)base.BaseGet(index);
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        public void Add(ListGroup listGroup)
        {
            BaseAdd(listGroup);
        }

        public void Clear()
        {
            BaseClear();
        }

        /// <summary>
        /// Removal all occurrances of the list group in the ListGroup collection as well as all children.
        /// </summary>
        /// <param name="listGroup">This list group to remove.</param>
        public void DeepRemove(ListGroup listGroup)
        {
            for(int i=0; i < this.Count; i++)
            {
                ListGroup group = this[i];
                group.ListGroups.DeepRemove(listGroup);
                if (group.Id == listGroup.Id)
                    this.RemoveAt(i--);                                    
            }
        }

        public void Remove(ListGroup listGroup)
        {            
            BaseRemove(listGroup.Id);
        }

        public void Remove(Guid id)
        {
            BaseRemove(id);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ListGroup();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ListGroup)element).Id;
        } 

    }
}
