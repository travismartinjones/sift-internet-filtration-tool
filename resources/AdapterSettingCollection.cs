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
    public class AdapterSettingCollection : ConfigurationElementCollection
    {        
        
        public AdapterSettingCollection()
        {                       
            this.AddElementName = "adapter";
        }

        #region Properties
        public AdapterSetting DefaultSettings
        {
            get
            {
                return GetByAdapterId(Resources.Constants.AdapterDefaultSettingId);
            }
        }

        public System.Collections.Generic.List<AdapterSetting> SystemAdapters
        {
            get
            {
                System.Collections.Generic.List<AdapterSetting> systemAdapters = new System.Collections.Generic.List<AdapterSetting>();
                
                foreach (AdapterSetting adapter in this)
                    if (adapter.Id != Resources.Constants.AdapterDefaultSettingId)
                        systemAdapters.Add(adapter);
                
                return systemAdapters;
            }
        }
        #endregion

        #region Serialization
        public AdapterSettingCollection(System.Runtime.Serialization.SerializationInfo si, System.Runtime.Serialization.StreamingContext sc)
        {
            
        }

        #endregion

        public AdapterSetting this[int index]
        {
            get
            {
                return (AdapterSetting)base.BaseGet(index);
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

        public void Add(AdapterSetting adapterSetting)
        {
            BaseAdd(adapterSetting);
        }

        public void Clear()
        {
            BaseClear();
        }

        public void Remove(AdapterSetting adapterSetting)
        {
            BaseRemove(adapterSetting.Id);
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
            return new AdapterSetting();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AdapterSetting)element).Id;
        }

        public AdapterSetting GetByAdapterId(Guid id)
        {
            foreach (AdapterSetting adapter in this)
            {
                if (adapter.Id == id)
                    return adapter;
            }

            return null;
        }
    }
}
