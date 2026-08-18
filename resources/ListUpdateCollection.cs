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
    public class ListUpdateCollection : ConfigurationElementCollection
    {        
        
        public ListUpdateCollection()
        {                       
            this.AddElementName = "listUpdate";
        }

        #region Serialization
        public ListUpdateCollection(System.Runtime.Serialization.SerializationInfo si, System.Runtime.Serialization.StreamingContext sc)
        {
            
        }

        #endregion

        public ListUpdate this[int index]
        {
            get
            {
                return (ListUpdate)base.BaseGet(index);
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

        public void Add(ListUpdate listUpdate)
        {
            BaseAdd(listUpdate);
        }

        public void Clear()
        {
            BaseClear();
        }

        public void Remove(ListUpdate listUpdate)
        {
            BaseRemove(listUpdate.ListId);
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
            return new ListUpdate();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ListUpdate)element).ListId;
        }

        public ListUpdate GetByListId(Guid listId)
        {
            foreach (ListUpdate listUpdate in this)
                if (listUpdate.ListId == listId)
                    return listUpdate;

            return null;
        }
    }
}
