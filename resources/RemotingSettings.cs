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
using System.Collections.Generic;
using System.Text;

namespace Sift.Resources.Settings
{
    /// <summary>
    /// Used by the remoting configuration application to adjust sift settings. Ideally this class would serialize the configuration
    /// objects, but they are not serializable and the .net SOAP and Binary serialization objects also do not fit the bill. To
    /// adjust for these limitations, we essentially use the remoting service as a soap web service allowing us to get and set xml.    
    /// </summary>
    public class RemotingSettings : MarshalByRefObject
    {
        private System.Xml.XmlDocument _configurationDocument = new System.Xml.XmlDocument();        

        public RemotingSettings()
        {
            _configurationDocument.Load(Resources.Constants.InstallPath + Resources.Constants.ConfigurationFilename);
        }

        public string GetConfiguration()
        {
            return _configurationDocument.OuterXml;
        }

        public void SaveConfiguration(string xml)
        {
            _configurationDocument.LoadXml(xml);
            _configurationDocument.Save(Resources.Constants.InstallPath + Resources.Constants.ConfigurationFilename);          
        }

        public void SaveListFile(Guid listID, string content)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(Resources.Settings.List.GetPathByListId(listID));

            file.Write(content);
        }

        public void SaveListUpdates(string listUpdates)
        {
            string listUpdateFilename = Resources.Constants.InstallPath + @"\" + Resources.Constants.ListUpdatesConfigurationFilename;
            if(System.IO.File.Exists(listUpdateFilename))
                System.IO.File.Delete(listUpdateFilename);

            System.Xml.XmlDocument document = new System.Xml.XmlDocument();
            document.LoadXml(listUpdates);
            document.Save(listUpdateFilename);            
        }

        public System.IO.StreamReader GetListStream(Guid listID)
        {
            System.IO.StreamReader fileReader = null;

            if (System.IO.File.Exists(Resources.Settings.List.GetPathByListId(listID)))
            {
                fileReader = new System.IO.StreamReader(Resources.Settings.List.GetPathByListId(listID));
            }

            if (fileReader == null)
            {
                // read off an empty file if no list file exists
                string tempFile = System.IO.Path.GetTempFileName();
                System.IO.File.CreateText(tempFile).Close();
                fileReader = new System.IO.StreamReader(tempFile);
            }

            return fileReader;
        }

        public Sift.Resources.ServiceStatistics GetServiceStatistics()
        {
            return Resources.Globals.Statistics;
        }
    }
}
