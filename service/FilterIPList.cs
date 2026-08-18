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
using System.IO;
using System.Text.RegularExpressions;

namespace Sift
{
    /// <summary>
    /// Implements a filter list that stores an array of 
    /// IPs. Used to lookup a destination IP in
    /// the array to determine the packet action.
    /// </summary>
    class FilterIPList : FilterList
    {
        /// <summary>
        /// IPComparer is used to compare IPs.
        /// </summary>
        class IPComparer : IComparer
        {
            int System.Collections.IComparer.Compare(Object x, Object y)
            {
                if (y.GetType() == typeof(UInt32))
                {
                    Resources.Globals.Log.Write("IP Compare " + (UInt32)((FilterListElement)x).Value + " " + (UInt32)y, Resources.Types.LogType.Information, Resources.Types.LogGroupType.FilterDecision, Resources.Types.LogDetailType.Verbose);
                    return (((UInt32)((FilterListElement)x).Value).CompareTo((UInt32)y));
                }

                if (y.GetType() == typeof(FilterListElement))
                {
                    Resources.Globals.Log.Write("IP Compare " + (UInt32)((FilterListElement)x).Value + " " + (UInt32)((FilterListElement)y).Value, Resources.Types.LogType.Information, Resources.Types.LogGroupType.FilterDecision, Resources.Types.LogDetailType.Verbose);
                    return ((UInt32)((FilterListElement)x).Value).CompareTo(
                                 (UInt32)((FilterListElement)y).Value);
                }

                // fail gracefully and log the error
                throw new Exception("Illegal IPComparer comparison type " + y.GetType().ToString());
            }
        }

        #region METHODS

        /// <summary>
        /// Converts an IP string to an Int32.
        /// </summary>
        /// <param name="ip">The IP string to convert.</param>
        /// <returns>The integer representation of the IP string. Returns
        /// zero if the supplied IP string is invalid.</returns>
        private uint IPToInt(string ip)
        {
            Match m;            
            uint a, b, c, d;            

            m = Regex.Match(ip,@"(\d{1,3}).(\d{1,3}).(\d{1,3}).(\d{1,3})");

            if (m.Success)
            {
                a = Convert.ToUInt32(m.Result("$1"));
                b = Convert.ToUInt32(m.Result("$2"));
                c = Convert.ToUInt32(m.Result("$3"));
                d = Convert.ToUInt32(m.Result("$4"));

                if ((a < 1 || a > 255) || (b < 0 || b > 255) ||
                    (c < 0 || c > 255) || (d < 0 || d > 255))
                    return 0;

                return (uint)(a * 16777216 + b * 65536 + c * 256 + d);
            }
            else
            {
                return 0;
            }            
        }

        /// <summary>
        /// Loads a new line delimited ip list file.
        /// </summary>
        /// <param name="filename">The filename containing the ip list.</param>
        /// <returns>Returns true if the file was successfully loaded.</returns>
        public override int LoadList(string filename)
        {
            return LoadList(filename, string.Empty);
        }

        /// <summary>
        /// Loads a new line delimited ip list file.
        /// </summary>
        /// <param name="filename">The filename containing the ip list.</param>
        /// <param name="category">The category the IP entries belong to. Also
        /// used in logging functionality.</param>
        /// <returns>Returns true if the file was successfully loaded.</returns>
        public override int LoadList(string filename,string category)
        {
            return LoadList(filename, category, false);
        }

        /// <summary>
        /// Loads a new line delimited ip list file.
        /// </summary>
        /// <param name="filename">The filename containing the ip list.</param>
        /// <param name="category">The category the IP entries belong to. Also
        /// used in logging functionality.</param>
        /// <param name="log">Indicates if the entry should be logged if a match is
        /// made.</param>
        /// <returns>Returns true if the file was successfully loaded.</returns>
        public override int LoadList(string filename, string category,bool log)
        {
            int linesLoaded = 0;
            string ip = string.Empty;
            Match m;
            if (!File.Exists(filename))
            {
                return linesLoaded;
            }
            StreamReader file = new StreamReader(filename);

            int i = 0;

            while (ip != null)
            {
                i++;
                ip = file.ReadLine();

                if (ip != null)
                {
                    // the ip must match ###.###.###.###
                    m = Regex.Match(ip, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");

                    if (m.Success)
                    {
                        FilterListElement element;

                        if (category == string.Empty)
                            element = new FilterListElement(ip, null, filename, i, log);
                        else
                            element = new FilterListElement(ip, category, filename, i, log); 
                        
                        this.Add(element);
                        linesLoaded++;
                    }
                }
            }

            // close our file handle
            file.Close();
            return linesLoaded;
        }

        /// <summary>
        /// Adds a FilterListElement to the FilterIPList.
        /// </summary>
        /// <param name="value">A FilterListElement with the Value field
        /// populated with an IP.</param>
        /// <returns>The zero-based index where the object was added.</returns>
        public override int Add(object value)
        {
            IPComparer comparer = new IPComparer();

            if (value.GetType() == typeof(Int32))
            {
                FilterListElement element = new FilterListElement((int)value);
                return base.Add(element, comparer);
            }
            else if (value.GetType() == typeof(String))
            {
                FilterListElement element = new FilterListElement(IPToInt((String)value));
                return base.Add(element, comparer);
            }
            else if (value.GetType() == typeof(FilterListElement))
            {
                if (((FilterListElement)value).Value.GetType() == typeof(String))
                {
                    ((FilterListElement)value).Value = IPToInt((string)((FilterListElement)value).Value);
                    return base.Add(value, comparer);
                }

                if (((FilterListElement)value).Value.GetType() == typeof(Int32))
                {
                    return base.Add(value, comparer);
                }
            }

            throw new Exception("Illegal object type to add.");
        }

        /// <summary>
        /// Searches for a http url string in the FilterDomainList.
        /// </summary>
        /// <param name="value">The domains string to search for in the 
        /// FilterDomainList</param>
        /// <returns>The zero-based index where the object was found. If not
        /// found, returns a negative.</returns>
        public override int BinarySearch(object value)
        {            
            IPComparer comparer = new IPComparer();

            if(value.GetType() == typeof(UInt32))
            {
                return base.BinarySearch(value,comparer);
            }

            if(value.GetType() == typeof(String))
            {
                return base.BinarySearch(IPToInt((string)value),comparer);
            }

            throw new Exception("Value must be of type System.String");
        }

        /// <summary>
        /// Sorts the FilterIPList.
        /// </summary>
        public override void Sort()
        {
            IPComparer comparer = new IPComparer();
            base.Sort(comparer);
        }      

        #endregion METHODS

    }
}