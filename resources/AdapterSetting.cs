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
    public class AdapterSetting : ConfigurationElement
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

        [ConfigurationProperty("DefaultAction", IsRequired = false)]
        public Sift.Resources.Types.DefaultActionType DefaultAction
        {
            get
            {
                Sift.Resources.Types.DefaultActionType? type;
                type = (Sift.Resources.Types.DefaultActionType)Enum.Parse(typeof(Sift.Resources.Types.DefaultActionType), this["DefaultAction"].ToString());
                if (type.HasValue)
                    return type.Value;
                else
                    return Sift.Resources.Types.DefaultActionType.Drop;
            }
            set
            {
                this["DefaultAction"] = value.ToString();
            }
        }

        [ConfigurationProperty("FilterInbound", IsRequired = false)]
        public bool FilterInbound
        {
            get
            {
                return (bool)this["FilterInbound"];
            }
            set
            {
                this["FilterInbound"] = value;
            }
        }

        [ConfigurationProperty("FilterOutbound", IsRequired = false)]
        public bool FilterOutbound
        {
            get
            {
                return (bool)this["FilterOutbound"];
            }
            set
            {
                this["FilterOutbound"] = value;
            }
        }
        
        [ConfigurationProperty("FilterHTTP", IsRequired = false)]
        public bool FilterHTTP
        {
            get
            {
                return (bool)this["FilterHTTP"];
            }
            set
            {
                this["FilterHTTP"] = value;
            }
        }

        [ConfigurationProperty("FilterTCP", IsRequired = false)]
        public bool FilterTCP
        {
            get
            {
                return (bool)this["FilterTCP"];
            }
            set
            {
                this["FilterTCP"] = value;
            }
        }

        [ConfigurationProperty("FilterUDP", IsRequired = false)]
        public bool FilterUDP
        {
            get
            {
                return (bool)this["FilterUDP"];
            }
            set
            {
                this["FilterUDP"] = value;
            }
        }

        [ConfigurationProperty("FilterAll", IsRequired = false)]
        public bool FilterAll
        {
            get
            {
                return (bool)this["FilterAll"];
            }
            set
            {
                this["FilterAll"] = value;
            }
        }

        [ConfigurationProperty("UseDefaults", DefaultValue = true, IsRequired = false)]
        public bool UseDefaults
        {
            get
            {
                return (bool)this["UseDefaults"];
            }
            set
            {
                this["UseDefaults"] = value;
            }
        }                
    }
}
