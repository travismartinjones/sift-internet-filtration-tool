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
    public class ListCollection : ConfigurationElementCollection
    {
        public ListCollection()
        {
            this.AddElementName = "list";
        }

        public List this[int index]
        {
            get
            {
                return (List)base.BaseGet(index);
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

        public List GetByListId(Guid id)
        {
            foreach (List list in this)
                if (list.Id == id)
                    return list;

            return null;
        }

        public void Add(List list)
        {
            BaseAdd(list);
        }

        public void Clear()
        {
            BaseClear();                        
        }

        public void Remove(List list)
        {
            BaseRemove(list.Id);
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
            return new List();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((List)element).Id;
        }

    }
}
