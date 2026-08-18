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
using System.IO;
using System.Configuration;

namespace Sift
{
    /// <summary>
    /// Implements a filter list that stores an array of 
    /// Domain strings. Used to lookup a http domain request in
    /// the array to determine the packet action.
    /// </summary>
    class FilterDomainList : FilterList
    {
        /// <summary>
        /// URLComparer is used to compare http request Domains.
        /// </summary>
        class DomainComparer : IComparer
        {
            /// <summary>
            /// Compares a http domain to to the Filter Domain List until a 
            /// match is made or until all elements are compared.
            /// </summary>
            /// <param name="x">The object to compare as a string. Or a 
            /// FilterListElement with a domain contained in the Value.
            /// </param>
            /// <param name="y">The current object being compared in 
            /// the Domain List.</param>
            /// <returns>Returns a match if the request is contained in
            /// an entry in the Domain List. Otherwise returns the
            /// value of System.String.Compare()</returns>
            int System.Collections.IComparer.Compare(Object x, Object y)
            {
                if (y.GetType() == typeof(string))
                {
                    Resources.Globals.Log.Write("Domain Compare " + (string)((FilterListElement)x).Value + " " + (string)y, Resources.Types.LogType.Information, Resources.Types.LogGroupType.FilterDecision, Resources.Types.LogDetailType.Verbose);
                    if (Regex.IsMatch((string)y,
                                      "^" + @Regex.Escape((string)((FilterListElement)x).Value + ".")))
                    {
                        return 0;
                    }                    
                    return string.Compare((string)((FilterListElement)x).Value,(string)y);
                }

                if (y.GetType() == typeof(FilterListElement))
                {
                    Resources.Globals.Log.Write("Domain Compare " + (string)((FilterListElement)x).Value + " " + (string)((FilterListElement)y).Value, Resources.Types.LogType.Information, Resources.Types.LogGroupType.FilterDecision, Resources.Types.LogDetailType.Verbose);
                    return string.Compare((string)((FilterListElement)x).Value,
                                          (string)((FilterListElement)y).Value);
                }

                throw new Exception("Illegal comparison");                
            }
        }

        #region METHODS

        /// <summary>
        /// Reverses a string.
        /// </summary>
        /// <param name="input">The string to reverse.</param>
        /// <returns>The reversed input string.</returns>
        public string StringReverse(string input)
        {
            char[] inputChar = input.ToCharArray();
            Array.Reverse(inputChar);
            return new string(inputChar);
        }

        /// <summary>
        /// Loads a new line delimited domain list file.
        /// </summary>
        /// <param name="filename">The filename containing the domain list.</param>
        /// <returns>Returns true if the file was successfully loaded.</returns>
        public override int LoadList(string filename)
        {
            return LoadList(filename, string.Empty);
        }

        /// <summary>
        /// Loads a new line delimited domain list file.
        /// </summary>
        /// <param name="filename">The filename containing the domain list.</param>
        /// <param name="category">The category the daomin entries belong to. Also
        /// used in logging functionality.</param>
        /// <returns>Returns true if the file was successfully loaded.</returns>
        public override int LoadList(string filename,string category)
        {
            return LoadList(filename, category, false);
        }

        /// <summary>
        /// Loads a new line delimited domain list file.
        /// </summary>
        /// <param name="filename">The filename containing the domain list.</param>
        /// <param name="category">The category the daomin entries belong to. Also
        /// used in logging functionality.</param>
        /// <param name="log">Indicates if the entry should be logged if a match is
        /// made.</param>
        /// <returns>Returns the number of list entries added.</returns>
        public override int LoadList(string filename, string category,bool log)
        {
            int linesLoaded = 0;

            if (!File.Exists(filename))
            {
                return linesLoaded;
            }

            StreamReader file = new StreamReader(filename);
            string domain = string.Empty;

            int i = 0;

            while (domain != null)
            {
                i++;
                domain = file.ReadLine();

                if (domain != null)
                {

                    FilterListElement element;
                    
                    if(category == string.Empty)
                        element = new FilterListElement(domain, null, filename, i, log);
                    else
                        element = new FilterListElement(domain, category, filename, i, log);

                    this.Add(element);
                    linesLoaded++;
                }
            }
            // close our file handle
            file.Close();
            return linesLoaded;
        }

        /// <summary>
        /// Adds a FilterListElement to the FilterDomainList.
        /// </summary>
        /// <param name="value">A FilterListElement with the Value field
        /// populated with a http domain.</param>
        /// <returns>The zero-based index where the object was added.</returns>
        public override int Add(object value)
        {
            DomainComparer comparer = new DomainComparer();

            if (value.GetType() == typeof(String))
            {
                FilterListElement element = new FilterListElement(
                                     StringReverse((String)value));
                return base.Add(element, comparer);
            }
            else if (value.GetType() == typeof(FilterListElement))
            {
                ((FilterListElement)value).Value = StringReverse((string)((FilterListElement)value).Value);
                return base.Add(value, comparer);
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
            if (value.GetType() == typeof(String))
            {
                DomainComparer comparer = new DomainComparer();

                return base.BinarySearch(StringReverse((string)value) + ".",comparer);
            }

            throw new Exception("Value must be of type System.String");
        }

        /// <summary>
        /// Sorts the FilterDomainList.
        /// </summary>
        public override void Sort()
        {
            DomainComparer comparer = new DomainComparer();
            base.Sort(comparer);
        }      

        #endregion METHODS
    }
}
