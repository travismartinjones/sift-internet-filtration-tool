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

namespace Sift
{
    /// <summary>
    /// A collection of content lists types used to
    /// filter IP and HTTP traffic.
    /// </summary>
    class FilterActionList
    {
        #region MEMBERS

        FilterIPList        ipList;
        FilterDomainList    domainList;
        FilterURLList       urlList;

        #endregion MEMBERS

        #region CONSTRUCTOR_DESTRUCTOR

        /// <summary>
        /// Initializes a new instance of the FilterActionList class.
        /// </summary>
        public FilterActionList()
        {
            ipList = null;
            domainList = null;
            urlList = null;
        }

        #endregion CONSTRUCTOR_DESTRUCTOR

        #region PROPERTIES

        /// <summary>
        /// Gets the number of domains loaded.
        /// </summary>
        public int DomainCount
        {
            get
            {
                if (this.domainList == null)
                {
                    return 0;
                }
                else
                {
                    return this.domainList.Count;
                }
            }
        }

        /// <summary>
        /// Gets the number of IPs loaded.
        /// </summary>
        public int IPCount
        {
            get
            {
                if (this.ipList == null)
                {
                    return 0;
                }
                else
                {
                    return this.ipList.Count;
                }
            }
        }

        /// <summary>
        /// Gets the number of URLs loaded.
        /// </summary>
        public int URLCount
        {
            get
            {
                if (this.urlList == null)
                {
                    return 0;
                }
                else
                {
                    return this.urlList.Count;
                }

            }
        }

        #endregion PROPERTIES

        #region METHODS

        /// <summary>
        /// Clears all loaded filter lists.
        /// </summary>
        public void Clear()
        {
            if (ipList != null)
            {
                ipList.Clear();
            }

            if (urlList != null)
            {
                urlList.Clear();
            }

            if (domainList != null)
            {
                domainList.Clear();
            }
        }

        /// <summary>
        /// Filters a packet against all loaded filter lists.
        /// </summary>
        /// <param name="packet"></param>
        /// <returns>Returns the FilterListElement the packet
        /// matched against or returns null if no match is made.
        /// </returns>
        public FilterListElement Search(Packet packet)
        {            
            Resources.Globals.Log.Write("Searching. Content: " + packet.Content, Sift.Resources.Types.LogType.Information, Sift.Resources.Types.LogGroupType.ServiceDebug, Sift.Resources.Types.LogDetailType.Verbose);
            int result;

            if (ipList != null)
            {
                Resources.Globals.Log.Write("Searching IP : " + packet.IPToString(packet.DestinationIP), Sift.Resources.Types.LogType.Information, Sift.Resources.Types.LogGroupType.FilterDecision, Sift.Resources.Types.LogDetailType.Verbose);

                // IP List
                result = this.ipList.BinarySearch(packet.DestinationIP);
                if (result >= 0)
                {
                    FilterListElement match = (FilterListElement)this.ipList[result];

                    if (match != null)
                    {
                        if (match.Log)
                            Resources.Globals.Log.Write("IP-Match [" + packet.IPToString(packet.DestinationIP) + "] with [" + match.Category + ":" + match.LineNumber + "][" + packet.IPToString((uint)match.Value) + "]", Resources.Types.LogType.Information, Resources.Types.LogGroupType.FilterMatch, Resources.Types.LogDetailType.Minimal);
                        else
                            Resources.Globals.Log.Write("IP-Match [" + packet.IPToString(packet.DestinationIP) + "] with [" + match.Category + ":" + match.LineNumber + "][" + packet.IPToString((uint)match.Value) + "]", Resources.Types.LogType.Information, Resources.Types.LogGroupType.FilterMatch, Resources.Types.LogDetailType.Verbose);
                    }

                    return match;
                }            
            }

            // only do domain and url searches
            // if the packet is TCP/IP HTTP
            if (packet.Content != null)
            {
                // Domain List
                if (domainList != null)
                {
                    Resources.Globals.Log.Write("Searching Domain : " + packet.Domain, Sift.Resources.Types.LogType.Information, Sift.Resources.Types.LogGroupType.FilterDecision, Sift.Resources.Types.LogDetailType.Verbose);                    

                    result = this.domainList.BinarySearch(packet.Domain);
                    if (result >= 0)
                    {
                        FilterListElement match = (FilterListElement)this.domainList[result];

                        if (match != null)
                        {
                            if (match.Log)
                                Resources.Globals.Log.Write("Domain-Match [" + packet.Domain + "] with [" + match.Category + ":" + match.LineNumber + "][" + this.domainList.StringReverse((string)match.Value) + "]", Resources.Types.LogType.Information, Resources.Types.LogGroupType.FilterMatch, Resources.Types.LogDetailType.Minimal);
                            else
                                Resources.Globals.Log.Write("Domain-Match [" + packet.Domain + "] with [" + match.Category + ":" + match.LineNumber + "][" + this.domainList.StringReverse((string)match.Value) + "]", Resources.Types.LogType.Information, Resources.Types.LogGroupType.FilterMatch, Resources.Types.LogDetailType.Verbose);
                        }

                        return match;
                    }
                }

                // URL List
                if (urlList != null)
                {
                    Resources.Globals.Log.Write("Searching URL : " + packet.URL, Sift.Resources.Types.LogType.Information, Sift.Resources.Types.LogGroupType.FilterDecision, Sift.Resources.Types.LogDetailType.Verbose);

                    result = this.urlList.BinarySearch(packet.Domain + packet.URL);
                    if (result >= 0)
                    {
                        FilterListElement match = (FilterListElement)this.urlList[result];

                        if (match != null)
                        {
                            if (match.Log)
                                Resources.Globals.Log.Write("URL-Match [" + packet.Domain + packet.URL + "] with [" + match.Category + ":" + match.LineNumber + "][" + this.domainList.StringReverse((string)match.Value) + "]", Resources.Types.LogType.Information, Resources.Types.LogGroupType.FilterMatch, Resources.Types.LogDetailType.Minimal);
                            else
                                Resources.Globals.Log.Write("URL-Match [" + packet.Domain + packet.URL + "] with [" + match.Category + ":" + match.LineNumber + "][" + this.domainList.StringReverse((string)match.Value) + "]", Resources.Types.LogType.Information, Resources.Types.LogGroupType.FilterMatch, Resources.Types.LogDetailType.Verbose);
                        }

                        return match;
                    }
                }

                // URL Converter to IP List
                if (urlList != null)
                {
                    result = this.urlList.BinarySearch(packet.IPToString(packet.DestinationIP) + packet.URL);
                    if (result >= 0)
                    {
                        FilterListElement match = (FilterListElement)this.urlList[result];

                        if (match != null)
                        {
                            if (match.Log)
                                Resources.Globals.Log.Write("URL-Match [" + packet.IPToString(packet.DestinationIP) + packet.URL + "] with [" + match.Category + ":" + match.LineNumber + "][" + this.domainList.StringReverse((string)match.Value) + "]", Resources.Types.LogType.Information, Resources.Types.LogGroupType.FilterMatch, Resources.Types.LogDetailType.Minimal);
                            else
                                Resources.Globals.Log.Write("URL-Match [" + packet.IPToString(packet.DestinationIP) + packet.URL + "] with [" + match.Category + ":" + match.LineNumber + "][" + this.domainList.StringReverse((string)match.Value) + "]", Resources.Types.LogType.Information, Resources.Types.LogGroupType.FilterMatch, Resources.Types.LogDetailType.Verbose);
                        }

                        return match;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Loads an IP list from a file.
        /// </summary>
        /// <param name="filename">The filename to load.</param>
        /// <returns>Returns true if the load was successfull.</returns>
        public int LoadIPList(string filename)
        {
            if (this.ipList == null)
            {
                this.ipList = new FilterIPList();
            }
                        
            return this.ipList.LoadList(filename);
        }

        /// <summary>
        /// Loads an IP list from a file.
        /// </summary>
        /// <param name="filename">The filename to load.</param>
        /// <param name="category">The category the list belongs to.</param>
        /// <returns>Returns true if the load was successfull.</returns>
        public int LoadIPList(string filename,string category)
        {
            if (this.ipList == null)
            {
                this.ipList = new FilterIPList();
            }

            return this.ipList.LoadList(filename,category);
        }

        /// <summary>
        /// Loads an IP list from a file.
        /// </summary>
        /// <param name="filename">The filename to load.</param>
        /// <param name="category">The category the list belongs to.</param>
        /// <param name="log">Indicates if the items in the file should be
        /// logged if a subsequent match is made.</param>
        /// <returns>Returns true if the load was successfull.</returns>
        public int LoadIPList(string filename, string category,bool log)
        {
            if (this.ipList == null)
            {
                this.ipList = new FilterIPList();
            }

            return this.ipList.LoadList(filename, category,log);
        }

        /// <summary>
        /// Loads a URL list from a file.
        /// </summary>
        /// <param name="filename">The filename to load.</param>
        /// <returns>Returns true if the load was successfull.</returns>
        public int LoadURLList(string filename)
        {
            if (this.urlList == null)
            {
                this.urlList = new FilterURLList();
            }
            
            return this.urlList.LoadList(filename);
        }

        /// <summary>
        /// Loads a URL list from a file.
        /// </summary>
        /// <param name="filename">The filename to load.</param>
        /// <param name="category">The category the list belongs to.</param>
        /// <returns>Returns true if the load was successfull.</returns>
        public int LoadURLList(string filename,string category)
        {
            if (this.urlList == null)
            {
                this.urlList = new FilterURLList();
            }

            return this.urlList.LoadList(filename,category);
        }

        /// <summary>
        /// Loads a URL list from a file.
        /// </summary>
        /// <param name="filename">The filename to load.</param>
        /// <param name="category">The category the list belongs to.</param>
        /// <param name="log">Indicates if the items in the file should be
        /// logged if a subsequent match is made.</param>
        /// <returns>Returns true if the load was successfull.</returns>
        public int LoadURLList(string filename, string category,bool log)
        {
            if (this.urlList == null)
            {
                this.urlList = new FilterURLList();
            }

            return this.urlList.LoadList(filename, category,log);
        }

        /// <summary>
        /// Loads a Domain list from a file.
        /// </summary>
        /// <param name="filename">The filename to load.</param>
        /// <returns>Returns true if the load was successfull.</returns>
        public int LoadDomainList(string filename)
        {
            if (this.domainList == null)
            {
                this.domainList = new FilterDomainList();
            }

            return this.domainList.LoadList(filename);            
        }

        /// <summary>
        /// Loads a Domain list from a file.
        /// </summary>
        /// <param name="filename">The filename to load.</param>
        /// <param name="category">The category the list belongs to.</param>
        /// <returns>Returns true if the load was successfull.</returns>
        public int LoadDomainList(string filename,string category)
        {
            if (this.domainList == null)
            {
                this.domainList = new FilterDomainList();
            }

            return this.domainList.LoadList(filename,category);
        }

        /// <summary>
        /// Loads a Domain list from a file.
        /// </summary>
        /// <param name="filename">The filename to load.</param>
        /// <param name="category">The category the list belongs to.</param>
        /// <param name="log">Indicates if the items in the file should be
        /// logged if a subsequent match is made.</param>
        /// <returns>Returns true if the load was successfull.</returns>
        public int LoadDomainList(string filename,string category,bool log)
        {
            if (this.domainList == null)
            {
                this.domainList = new FilterDomainList();
            }

            return this.domainList.LoadList(filename,category,log);
        }

        /// <summary>
        /// Adds an IP to the IP list.
        /// </summary>
        /// <param name="ip">The IP string to add.</param>
        public void AddIP(string ip)
        {
            if (this.ipList == null)
            {
                this.ipList = new FilterIPList();
            }

            FilterListElement element = new FilterListElement(ip);
            this.ipList.Add(element);
        }

        /// <summary>
        /// Adds a URL to the URL list.
        /// </summary>
        /// <param name="ip">The URL string to add.</param>
        public void AddURL(string url)
        {
            if (this.urlList == null)
            {
                this.urlList = new FilterURLList();
            }

            FilterListElement element = new FilterListElement(url);
            this.urlList.Add(url);
        }

        /// <summary>
        /// Adds a Domain to the Domain list.
        /// </summary>
        /// <param name="ip">The Domain string to add.</param>
        public void AddDomain(string domain)
        {
            if (this.domainList == null)
            {
                this.domainList = new FilterDomainList();
            }

            FilterListElement element = new FilterListElement(domain);
            this.domainList.Add(domain);
        }

        /// <summary>
        /// Sorts all loaded filter lists.
        /// </summary>
        public void Sort()
        {            
            if (this.ipList != null)
            {
                this.ipList.Sort();
            }

            if (this.urlList != null)
            {
                this.urlList.Sort();
            }

            if (this.domainList != null)
            {
                this.domainList.Sort();
            }
        }

        /// <summary>
        /// Removes the specified ip from the list.
        /// </summary>
        /// <param name="ip">The ip to remove.</param>
        public void RemoveIP(string ip)
        {
            FilterListElement match = SearchIP(ip);

            if(match != null)
                this.ipList.Remove(match);
        }

        /// <summary>
        /// Removes the specified url from the list.
        /// </summary>
        /// <param name="url">The url to remove.</param>
        public void RemoveURL(string url)
        {
            FilterListElement match = SearchURL(url);

            if(match != null)
                this.urlList.Remove(match);
        }

        /// <summary>
        /// Removes the specified domain from the list.
        /// </summary>
        /// <param name="url">The domain to remove.</param>
        public void RemoveDomain(string domain)
        {
            FilterListElement match = SearchURL(domain);

            if (match != null)
                this.domainList.Remove(domain);
        }

        //
        // FOR DEBUGGING ONLY
        //
        public FilterListElement SearchIP(string ip)
        {
            if (ipList != null)
            {
                int result = ipList.BinarySearch(ip);
                if (result >= 0)
                    return ((FilterListElement)ipList[result]);
            }

            return null;
        }

        public FilterListElement SearchURL(string url)
        {
            if (urlList != null)
            {
                int result = urlList.BinarySearch(url);
             
                if (result >= 0)
                    return ((FilterListElement)urlList[result]);
            }
            
            return null;            
        }

        public FilterListElement SearchDomain(string domain)
        {
            if (domainList != null)
            {
                int result = domainList.BinarySearch(domain);
                
                if (result >= 0)
                    return ((FilterListElement)domainList[result]);
            }

            return null;
        }

        #endregion METHODS
    }
}
