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


namespace Sift
{
    class FilterList : SortableArrayList
    {
        #region CONSTRUCTOR_DESTRUCTOR

        public FilterList()
        {         
        }

        public FilterList(string filename)
        {
            this.LoadList(filename);
        }

        #endregion CONSTRUCTOR_DESTRUCTOR

        #region METHODS

        public virtual int LoadList(string filename)
        {
            return LoadList(filename, string.Empty);
        }

        public virtual int LoadList(string filename,string category)
        {
            return LoadList(filename, category, false);
        }

        public virtual int LoadList(string filename, string category,bool log)
        {
            int linesLoaded = 0;
            string line = string.Empty;

            if (!File.Exists(filename))
            {
                return linesLoaded;
            }
            StreamReader file = new StreamReader(filename);

            int i = 0;

            while (line != null)
            {
                i++;
                line = file.ReadLine();

                if (line != null)
                {
                    FilterListElement element;
                    
                    if(category == string.Empty)
                        element = new FilterListElement(line, null, filename, i, log);
                    else
                        element = new FilterListElement(line, category, filename, i, log);

                    this.Add(element);
                    linesLoaded++;
                }
            }

            // close our file handle
            file.Close();
            return linesLoaded;
        }

        public override int BinarySearch(object value)        
        {
            this.Sort();
            return base.BinarySearch(value);
        }

        #endregion METHODS
    }
}
