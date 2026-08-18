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
using System.Collections.Generic;

namespace Sift.Resources.Settings
{
    public class ListEntryUpdateCollection : ConfigurationElementCollection
    {
        private static int elementKeyIndex = 0;

        public ListEntryUpdateCollection()
        {
            this.AddElementName = "listEntryUpdate";
        }

        public ListEntryUpdate this[int index]
        {
            get
            {
                return (ListEntryUpdate)base.BaseGet(index);
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

        public string[] Values
        {
            get
            {                
                List<string> values = new List<string>();
                foreach (ListEntryUpdate listEntryUpdate in this)
                    if (!values.Contains(listEntryUpdate.Value))
                        values.Add(listEntryUpdate.Value);               

                return values.ToArray();
            }
        }

        public void Add(ListEntryUpdate list)
        {            
            BaseAdd(list);
        }

        public void Clear()
        {
            BaseClear();                        
        }

        public void Remove(ListEntryUpdate listEntryUpdate)
        {
            BaseRemove(listEntryUpdate.Value);
        }

        public void Remove(string path)
        {
            BaseRemove(path);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ListEntryUpdate();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            // we don't care about duplicate values, so generate an arbitrary uniqueness            
            //return DateTime.Now;
            return ++ListEntryUpdateCollection.elementKeyIndex;
        }

        private class ListEntryUpdateComparer : IComparer<ListEntryUpdate>
        {
            int System.Collections.Generic.IComparer<ListEntryUpdate>.Compare(ListEntryUpdate x, ListEntryUpdate y)
            {
                return DateTime.Compare(x.DateCreated, y.DateCreated);
            }
        }

        public List<ListEntryUpdate> GetByValue(string value)
        {
            List<ListEntryUpdate> matches = new List<ListEntryUpdate>();

            foreach (ListEntryUpdate listEntryUpdate in this)
                if (listEntryUpdate.Value == value)
                    matches.Add(listEntryUpdate);

            // before returning, sort by date
            matches.Sort(new ListEntryUpdateComparer());
            
            return matches;
        }

        public ListEntryUpdate GetMostRecentByValue(string value)
        {
            List<ListEntryUpdate> matches = new List<ListEntryUpdate>();

            foreach (ListEntryUpdate listEntryUpdate in this)
                if (listEntryUpdate.Value == value)
                    matches.Add(listEntryUpdate);

            // before returning, sort by date
            matches.Sort(new ListEntryUpdateComparer());

            if (matches.Count > 0)
                return matches[matches.Count - 1];
            else
                return null;
        }

        public List<ListEntryUpdate> GetAllSorted()
        {
            List<ListEntryUpdate> all = new List<ListEntryUpdate>();

            foreach (ListEntryUpdate listEntryUpdate in this)
                all.Add(listEntryUpdate);

            // before returning, sort by date
            all.Sort(new ListEntryUpdateComparer());

            return all;
        }
    }
}
