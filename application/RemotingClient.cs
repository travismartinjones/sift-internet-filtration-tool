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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Net;

namespace Sift
{
    class RemotingClient
    {
        private static System.Xml.XmlDocument _configuration = new System.Xml.XmlDocument();
        private static Resources.Settings.RemotingSettings _settings = null;

        private static string _previousHost = string.Empty;
        private static int    _previousPort = 0;
        private static string _previousDomain = string.Empty;
        private static string _previousUsername = string.Empty;
        private static string _previousPassword = string.Empty;

        private static Resources.Settings.RemotingSettings GetSettings(string host, int port, string domain, string username, string password)
        {
            if (_previousHost != host || _previousPort != port || _previousDomain != domain || _previousUsername != username || 
                _previousPassword != password || _settings == null)
            {
                _settings = null;

                _previousHost = host;
                _previousPort = port;
                _previousDomain = domain;
                _previousUsername = username;
                _previousPassword = password;

                using (new Resources.Impersonator(username, domain, password))
                {
                    // Create a channel for communicating w/ the remote object
                    // Notice no port is specified on the client
                    try
                    {
                        TcpChannel chan = new TcpChannel();
                        ChannelServices.RegisterChannel(chan, true);
                    }
                    catch
                    {
                        // channel service is already registered
                    }

                    // Create an instance of the remote object
                    _settings = (Resources.Settings.RemotingSettings)Activator.GetObject(
                        typeof(Resources.Settings.RemotingSettings),
                        "tcp://" + host + ":" + port + "/" + Resources.Constants.RemotingGetConfiguration);
                }
            }

            return _settings;
        }

        public static bool IsRemotingAvailable(string host, int port, string domain, string username, string password)
        {
            using (new Resources.Impersonator(username, domain, password))
            {
                bool isRemotingAvailable = true;

                try
                {
                    System.Xml.XmlDocument configuration = new System.Xml.XmlDocument();
                    Resources.Settings.RemotingSettings settings = GetSettings(host, port, domain, username, password);                    
                    string xml = settings.GetConfiguration();
                }
                catch
                {
                    isRemotingAvailable = false;
                }

                return isRemotingAvailable;
            }
        }

        public static void GetConfiguration(string host, int port, string domain, string username, string password)
        {
            using (new Resources.Impersonator(username, domain, password))
            {                
                Resources.Settings.RemotingSettings settings = GetSettings(host, port, domain, username, password);

                string xml = settings.GetConfiguration();

                _configuration.LoadXml(xml);

                if (System.IO.File.Exists(Resources.Constants.InstallPath + Resources.Constants.ConfigurationRemotingFilename))
                {                    
                    System.IO.File.Delete(Resources.Constants.InstallPath + Resources.Constants.ConfigurationRemotingFilename);
                }

                _configuration.Save(Resources.Constants.InstallPath + Resources.Constants.ConfigurationRemotingFilename);
            }
        }

        public static void SaveConfiguration(string host, int port, string domain, string username, string password)
        {
            using (new Resources.Impersonator(username, domain, password))
            {                
                Resources.Settings.RemotingSettings settings = GetSettings(host, port, domain, username, password);

                _configuration.Load(Resources.Constants.InstallPath + Resources.Constants.ConfigurationRemotingFilename);
                _settings.SaveConfiguration(_configuration.OuterXml);
            }
        }

        public static void SaveListFile(string host, int port, string domain, string username, string password, Guid listID, string file)
        {
            using (new Resources.Impersonator(username, domain, password))
            {                
                Resources.Settings.RemotingSettings settings = GetSettings(host, port, domain, username, password);

                settings.SaveListFile(listID, file);
            }
        }

        public static void SaveListUpdates(string host, int port, string domain, string username, string password)
        {
            using (new Resources.Impersonator(username, domain, password))
            {
                if (System.IO.File.Exists(Resources.Constants.InstallPath + Resources.Constants.ListUpdatesConfigurationRemotingFilename))
                {
                    Resources.Settings.RemotingSettings settings = GetSettings(host, port, domain, username, password);

                    System.Xml.XmlDocument document = new System.Xml.XmlDocument();
                    document.Load(Resources.Constants.InstallPath + Resources.Constants.ListUpdatesConfigurationRemotingFilename);
                    settings.SaveListUpdates(document.OuterXml);
                }
            }
        }

        public static System.IO.StreamReader GetListStream(Guid listId, string host, int port, string domain, string username, string password)
        {
            using (new Resources.Impersonator(username, domain, password))
            {
                Resources.Settings.RemotingSettings settings = GetSettings(host, port, domain, username, password);

                return settings.GetListStream(listId);
            }
        }

        public static Resources.ServiceStatistics GetServiceStatistics(string host, int port, string domain, string username, string password)
        {
            using (new Resources.Impersonator(username, domain, password))
            {
                Resources.Settings.RemotingSettings settings = GetSettings(host, port, domain, username, password);

                try
                {
                    return settings.GetServiceStatistics();
                }
                catch
                {
                    System.Threading.Thread.Sleep(100); // sleep for a bit to give the service time to open remoting back up

                    try
                    {
                        return settings.GetServiceStatistics();
                    }
                    catch
                    {
                        return null; // give up
                    }
                }
            }
        }
    }
}


