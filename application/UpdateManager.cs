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
using System.Xml;
using System.Deployment;

namespace Sift
{
    /// <summary>
    /// Provides access to the latest information on SIFT updates.
    /// </summary>
    class UpdateManager
    {
        #region PRIVATE MEMBERS
        private static XmlDocument _updates = null;
        private static Version _availableVersion = null;
        private static string _availableDescription = null;
        private static List<string> _changes = null;
        #endregion

        #region PUBLIC PROPERTIES
        #region Updates
        /// <summary>
        /// The XmlDocument representation of the updates.xml file.
        /// </summary>
        protected static XmlDocument Updates
        {
            get
            {
                if (_updates == null)
                    _updates = UpdateManager.GetUpdateDocument();

                return _updates;
            }
            set
            {
                _updates = value;
            }
        }
        #endregion

        #region CurrentVersion
        /// <summary>
        /// The current running version of the application.
        /// </summary>
        public static Version CurrentVersion
        {
            get
            {                
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            }
        }
        #endregion

        #region AvailableVersion        
        /// <summary>
        /// The most recent version of SIFT available.
        /// </summary>
        public static Version AvailableVersion
        {
            get
            {
                if (_availableVersion == null)
                {
                    ProcessUpdateDocument(Updates);
                }

                return _availableVersion;
            }
        }
        #endregion

        #region AvailableDescription        
        /// <summary>
        /// The description associated with the most recent version of SIFT.
        /// </summary>
        public static string AvailableDescription
        {
            get
            {
                if (IsNewVersionAvailable)
                {
                    if(_availableDescription == null)
                        ProcessUpdateDocument(Updates);

                    return _availableDescription;
                }
                
                return "Your version of SIFT is up to date.";                
            }
        }
        #endregion

        #region Changes        
        public static List<string> Changes
        {
            get
            {
                if (_changes == null)
                    ProcessUpdateDocument(Updates);

                return _changes;
            }
        }
        #endregion

        #region IsNewVersionAvailable
        /// <summary>
        /// True if there is a new version of SIFT available for download, false otherwise.
        /// </summary>
        public static bool IsNewVersionAvailable
        {
            get
            {
                if (CurrentVersion.CompareTo(AvailableVersion) < 0)
                    return true;
                else
                    return false;
            }
        }
        #endregion
        #endregion

        #region PRIVATE METHODS
        /// <summary>
        /// Processes the updates.xml document into the private properties.
        /// </summary>
        /// <param name="document">The downloaded updates.xml document.</param>
        private static void ProcessUpdateDocument(XmlDocument document)
        {            
            // pull the version information from the update file
            XmlNodeList nodes = Updates.GetElementsByTagName("update");

            _availableVersion = CurrentVersion;

            _changes = new List<string>();

            foreach (XmlNode node in nodes)
            {
                Version version = new Version(node.Attributes["version"].Value);

                if (_availableVersion.CompareTo(version) < 0)
                {
                    // the version is more recent than the last
                    _availableVersion = version;
                    _availableDescription = node.SelectSingleNode("description").InnerText;
                    XmlNodeList changeNodes = node.SelectSingleNode("changes").SelectNodes("change");
                    foreach (XmlNode changeNode in changeNodes)
                        _changes.Add(changeNode.InnerText);
                }
            }                   
        }

        /// <summary>
        /// Processes the latest update information from the SIFT website.
        /// </summary>
        /// <returns>The latest updates.xml document.</returns>
        private static XmlDocument GetUpdateDocument()
        {
            XmlDocument document = new XmlDocument();

            try
            {
                document.Load(XmlReader.Create("http://sift.sourceforge.net/updates.xml"));
            }
            catch (System.Net.WebException)
            {

            }
            
            return document;
        }
        #endregion
    }
}
