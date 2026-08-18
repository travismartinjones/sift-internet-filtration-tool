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
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Management;
// remove for production
using System.Diagnostics;

namespace Sift
{
    /// <summary>
    /// Stores all information returned from the filter driver
    /// and any utility function used to perpare data for
    /// communication with the driver.
    /// </summary>
    class Adapter
    {
        #region MEMBERS

        /// <summary>
        /// The unicode name of the driver
        /// </summary>
        private Guid  id;
        /// <summary>
        /// The adapter device description pulled from WMI.
        /// </summary>
        private string  description;
        /// <summary>
        /// Stores the driver opened/closed status.
        /// </summary>
        private bool    opened;
        /// <summary>
        /// Stores if the driver is enabled.
        /// </summary>
        private bool    enabled;
        /// <summary>
        /// Array of all packets received from the filter
        /// driver adapter binding.
        /// </summary>
        private ArrayList packets;

        /// <summary>
        /// The hardware machine address of the network adapter.
        /// </summary>
        private string mac;

        private uint    settings;        

        #endregion MEMBERS

        #region PROPERTIES

        /// <summary>
        /// Returns true if the adapter is a valid entry. This is determined by the adapter having a valid hardware mac.
        /// This helps prevent pseudo adapters from being added. Virtual adapters like VPN, etc are pseudo adapters. 
        /// We only want to look at real traffic going over the wire, so we ignore the pseudos.
        /// </summary>
        public bool IsValid
        {
            get
            {
                System.Net.NetworkInformation.NetworkInterface[] interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();

                ManagementObjectSearcher query = null;
                ManagementObjectCollection collection = null;
                string settingID = "{" + id.ToString().ToUpper() + "}";

                query = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration");

                collection = query.Get();
                System.Collections.Generic.Dictionary<string, string> settings = new System.Collections.Generic.Dictionary<string, string>();

                foreach (ManagementObject managementObject in collection)
                {
                    if (managementObject["SettingID"] != null)
                    {
                        if (managementObject["SettingID"] != null && managementObject["MACAddress"] != null)
                            settings.Add(managementObject["SettingID"].ToString(), managementObject["MACAddress"].ToString());
                    }
                }

                if (settings.ContainsKey(settingID))
                {
                    mac = settings[settingID];
                    return true;
                }
                else
                    return false;
            }
        }

        public string MAC
        {
            get
            {
                return mac;
            }
            set
            {
                mac = value;
            }
        }

        public uint Settings
        {
            get
            {
                return settings;
            }
            set
            {
                settings = value;
            }
        }

        public Guid Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        public string Name
        {
            get
            {
                return @"\Device\{" + id.ToString().ToUpper() + "}";
            }
        }

        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
            }
        }

        public byte[] UnicodeName
        {
            get
            {
                return (new UnicodeEncoding()).GetBytes(this.Name);
            }      
        }

        public string Description
        {
            get 
            {               
                return this.description; 
            }
            set
            {
                this.description = value;
            }
        }

        public bool IsOpen
        {
            get
            {
                return opened;
            }
        }

        public Packet GetPacket(int index)
        {
            if (index < packets.Count)
            {
                return (Packet)packets[index];
            }
            else
            {
                return null;
            }
        }

        public void RemovePacket(Packet packet)
        {
            // remove the packet from the adapter
            // if it exists in the packet list
            if (packet != null)
            {
                packets.Remove(packet);
            }
        }

        public int PacketCount
        {
            get
            {
                return packets.Count;
            }
        }

        #endregion PROPERTIES

        #region CONSTRUCTOR

        public Adapter()
        {
            // default constructor
            packets = new ArrayList();
            this.enabled = true;
            
        }

        public Adapter(string adapterName)
        {
            // adapter name initialization
            packets = new ArrayList();
            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(adapterName, @"\\Device\\{(.*)}");
            if(match.Success)
                this.Id = new Guid(match.Groups[1].Value);
            else
                this.Id = Guid.Empty;

            this.enabled = true;
        }

        #endregion CONSTRUCTOR
        
        #region METHODS

        /// <summary>
        /// Creates a list of adapters from a unicode list of adapter names.
        /// </summary>
        /// <param name="unicodeList">A unicode string with adapter device names
        /// seperated by a unicode null.</param>
        /// <returns>An array of adapters created from the unicode string.</returns>
        public Adapter[] UnicodeToAdapter(string unicodeList)
        {
            string[]    adapterList;
            Adapter[]  newAdapterList;
            int         numAdapters = 0;

            // check to see if a valid string was passed
            if (unicodeList == null)
            {
                return null;
            }

            // each adapter name is seperated with a unicode null
            adapterList = unicodeList.Split('\0');

            // the Split() command will sometimes return blank strings
            // we want to ignore these when creating our adapter array
            for (int i = 0; i < adapterList.Length; i++)
            {
                if (adapterList[i] != "")
                {
                    numAdapters++;
                }
            }

            // create an adapter for each string
            newAdapterList = new Adapter[numAdapters];

            for (int i = 0; i < numAdapters; i++)
            {
                newAdapterList[i] = new Adapter(adapterList[i]);

                newAdapterList[i].Description = GetWMIDeviceName(adapterList[i]);
                // think about adding logic here that pulls the adapter
                // description from the system registry
                // newAdapterList[i].m_sName = GetAdapterDescription(sAdapterList[i]);
            }
            return newAdapterList;
        }

        /// <summary>
        /// DEBUGGING Prints out a description of the adapter to the
        /// event log.
        /// </summary>
        public void PrintAdapter()
        {
            EventLog.WriteEntry("Sift","Adapter name: " + this.Name);
            EventLog.WriteEntry("Sift", "\t" + this.description);
        }

        /// <summary>
        /// Opens the adapter.
        /// </summary>
        /// <returns>Returns true.</returns>
        public bool Open()
        {
            this.opened = true;            
            return true;
        }

        /// <summary>
        /// Closes the adapter.
        /// </summary>
        /// <returns>Returns true.</returns>
        public bool Close()
        {
            this.opened = false;
            return true;
        }

        /// <summary>
        /// Adds a packet to the adapter.
        /// </summary>
        /// <param name="packet">The Packet to add to the adapter.</param>
        public void AddPacket(Packet packet)
        {
            if (this.packets != null)
            {
                this.packets.Add(packet);
            }
        }

        /// <summary>
        /// Clears all packets from the adapter.
        /// </summary>
        public void ClearPackets()
        {
            if (packets != null)
            {
                packets.Clear();
            }
        }

        /// <summary>
        /// Prints all packets currently stored in the adapter.
        /// </summary>
        public void PrintPackets()
        {
            Packet packet;

            if (packets != null)
            {
                for (int i = 0; i < packets.Count; i++)
                {
                    packet = (Packet)packets[i];
                    packet.PrintContents();
                }
            }
        }

        /// <summary>
        /// Gets the WMI Device name based on the adapter id.
        /// </summary>
        /// <param name="adapter">The adapter string returned from
        /// the filter driver.</param>
        /// <returns>The WMI device name description.</returns>
        private string GetWMIDeviceName(string adapter)
        {
            ManagementObjectSearcher query = null;
            ManagementObjectCollection collection = null;
            string settingID = Regex.Replace(adapter, @"\\Device\\", "");

            string MAC = null;

            query = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration");

            collection = query.Get();

            foreach (ManagementObject managementObject in collection)
            {
                if (managementObject["SettingID"] != null)
                {
                    if (managementObject["SettingID"].ToString() == settingID)
                    {
                        MAC = managementObject["MACAddress"].ToString();
                    }
                }
            }

            if (MAC != null)
            {
                foreach (ManagementObject managementObject in collection)
                {
                    if (managementObject["MACAddress"] != null &&
                        managementObject["SettingID"] != null)
                    {
                        if ((managementObject["MACAddress"].ToString() == MAC) &&
                           (managementObject["SettingID"].ToString() != settingID))
                        {
                            // adapter found, return the description
                            return managementObject["Description"].ToString();
                        }
                    }
                }
            }


            // could not make a match, return an empty string
            return "";
        }

        private void SetSettings(uint settings)
        {
            this.settings |= settings;
        }

        private void UnsetSettings(uint settings)
        {
            this.settings |= settings;
            this.settings ^= settings;
        }        

        #endregion METHODS
    }
}
