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
    /// URL strings. Used to lookup a http url request in
    /// the array to determine the packet action.
    /// </summary>
    class FilterURLList : FilterList
    {
        /// <summary>
        /// URLComparer is used to compare http request urls.
        /// </summary>
        class URLComparer : IComparer
        {
            /// <summary>
            /// Compares a http url to to the Filter URL List until a 
            /// match is made or until all elements are compared.
            /// </summary>
            /// <param name="x">The object to compare as a string. Or a 
            /// FilterListElement with a url contained in the Value.
            /// </param>
            /// <param name="y">The current object being compared in 
            /// the URL List.</param>
            /// <returns>Returns a match if the request is contained in
            /// an entry in the URL List. Otherwise returns the
            /// value of System.String.Compare()</returns>
            int System.Collections.IComparer.Compare(Object x, Object y)
            {
                // provided to compare urls without the creation of a bulky
                // FilterListElement object
                if(y.GetType() == typeof(string))
                {
                    Resources.Globals.Log.Write("URL Compare " + (string)((FilterListElement)x).Value + " " + (string)y, Resources.Types.LogType.Information, Resources.Types.LogGroupType.FilterDecision, Resources.Types.LogDetailType.Verbose);
                    if (Regex.IsMatch((string)y,
                        @Regex.Escape((string)((FilterListElement)x).Value)))
                    {         
                        return 0;
                    }

                    return string.Compare((string)((FilterListElement)x).Value, 
                                          (string)y); 
                }

                if(y.GetType() == typeof(FilterListElement))
                {
                    Resources.Globals.Log.Write("URL Compare " + (string)((FilterListElement)x).Value + " " + (string)((FilterListElement)y).Value, Resources.Types.LogType.Information, Resources.Types.LogGroupType.FilterDecision, Resources.Types.LogDetailType.Verbose);
                    return string.Compare((string)((FilterListElement)x).Value, 
                                          (string)((FilterListElement)y).Value); 
                }

                throw new Exception("Illegal comparison");        
            }
        }

        #region METHODS

        /// <summary>
        /// Adds a FilterListElement to the FilterURLList.
        /// </summary>
        /// <param name="value">A FilterListElement with the Value field
        /// populated with a http url.</param>
        /// <returns>The zero-based index where the object was added.</returns>
        public override int Add(object value)
        {
            URLComparer comparer = new URLComparer();

            // add the URL, remove any trailing '/' that
            // could potentially cause match problems
            // also remove www. from any url, this helps
            // in the situation that the www server is also
            // addressable as 'someplace.com'
            if (value.GetType() == typeof(String))
            {
                FilterListElement element = new FilterListElement(
                    Regex.Replace(Regex.Replace((string)value, "/$", ""), @"^www\.", ""));
                return base.Add(element,comparer);
            }
            else if (value.GetType() == typeof(FilterListElement))
            {
                ((FilterListElement)value).Value = Regex.Replace(Regex.Replace((string)((FilterListElement)value).Value, "/$", ""), @"^www\.", "");
                return base.Add(value, comparer);
            }

            throw new Exception("Illegal object type to add.");            
        }

        /// <summary>
        /// Searches for a http url string in the FilterURLList.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>The zero-based index where the object was found. If not
        /// found, returns a negative.</returns>
        public override int BinarySearch(object value)
        {
            if(value.GetType() == typeof(String))
            {
                URLComparer comparer = new URLComparer();

                this.Sort();

                return base.BinarySearch(Regex.Replace((string)value, @"^www\.", ""), comparer);
            }

            throw new Exception("Value must be of type System.String");
        }

        /// <summary>
        /// Sorts the FilterURLList.
        /// </summary>
        public override void Sort()
        {
            URLComparer comparer = new URLComparer();
            base.Sort(comparer);
        }      

        #endregion METHODS
    }
}