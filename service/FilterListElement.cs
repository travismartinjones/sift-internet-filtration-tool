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
    /// A filter list element implementation.
    /// </summary>
    class FilterListElement
    {
        // stores the category name
        private string category;
        // stores the filename the list element was created from
        private string filename;
        // stores the line number the element is on
        private int    lineNumber;
        // stores the filter content
        private object value;
        // indicates if the element should be logged
        private bool   log;

        #region CONSTRUCTOR_DESTRUCTOR

        /// <summary>
        /// Initializes a new instance of the FilterListElement class.
        /// </summary>
        public FilterListElement()
        {
            this.category = null;
            this.filename = null;
            this.lineNumber = 0;
            this.value = 0;
            this.log = false;
        }

        /// <summary>
        /// Initializes a new instance of the FilterListElement class.
        /// </summary>
        public FilterListElement(object value)
        {
            this.category = null;
            this.filename = null;
            this.lineNumber = 0;
            this.value = value;
            this.log = false;            
        }

        /// <summary>
        /// Initializes a new instance of the FilterListElement class.
        /// </summary>
        public FilterListElement(object value, string category)
        {
            this.category = category;
            this.filename = null;
            this.lineNumber = 0;
            this.value = value;
            this.log = false;
        }

        /// <summary>
        /// Initializes a new instance of the FilterListElement class.
        /// </summary>
        public FilterListElement(object value, string category,
                                 string filename, int line)
        {
            this.category = category;
            this.filename = filename;
            this.lineNumber = line;
            this.value = value;
            this.log = false;
        }

        /// <summary>
        /// Initializes a new instance of the FilterListElement class.
        /// </summary>
        public FilterListElement(object value, string category,
                         string filename, int line, bool log)
        {
            this.category = category;
            this.filename = filename;
            this.lineNumber = line;
            this.value = value;
            this.log = log;
        }

        #endregion CONSTRUCTOR_DESTRUCTOR

        #region PROPERTIES

        /// <summary>
        /// Gets or sets the Category.
        /// </summary>
        public string Category
        {
            get
            {
                return this.category;
            }
            set
            {
                this.category = value;
            }
        }

        /// <summary>
        /// Gets or sets the FileName.
        /// </summary>
        public string FileName
        {
            get
            {
                return this.filename;
            }
            set
            {
                this.filename = value;
            }
        }

        /// <summary>
        /// Gets or sets the LineNumber.
        /// </summary>
        public int LineNumber
        {
            get
            {
                return this.lineNumber;
            }
            set
            {
                this.lineNumber = value;
            }
        }

        /// <summary>
        /// Gets or sets the Value.
        /// </summary>
        public object Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }

        /// <summary>
        /// Gets or sets the Log flag.
        /// </summary>
        public bool Log
        {
            get
            {
                return this.log;
            }
            set
            {
                this.log = value;
            }
        }

        #endregion PROPERTIES
    }
}
