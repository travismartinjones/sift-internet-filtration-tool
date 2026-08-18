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

namespace Sift
{
    /// <summary>
    /// Implementation of a sortable ArrayList class. Once the array 
    /// is sorted, any additions or removal retains the sort of the
    /// array.
    /// </summary>
    class SortableArrayList : ArrayList
    {
        #region MEMBERS

        bool sorted; 

        #endregion MEMBERS

        #region CONSTRUCTOR_DESTRUCTOR

        /// <summary>
        /// Initializes a new instance of the SortableArrayList class.
        /// </summary>
        public SortableArrayList()
        {
            sorted = false;
        }

        #endregion CONSTRUCTOR_DESTRUCTOR

        #region PROPERTIES

        /// <summary>
        /// Gets the sorted status of the array. Returns true
        /// if the array is sorted.
        /// </summary>
        public bool Sorted
        {
            get
            {
                return sorted;
            }
        }

        #endregion PROPERTIES

        #region METHODS

        /// <summary>
        /// Adds a value to the sorted array. If the array is sorted,
        /// the value inserted in sorted order. Otherwise the value
        /// is appended to the array.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Returns the zero-based index the value was
        /// inserted at.</returns>
        public override int Add(object value)
        {
            if (sorted)
            {
                // insert the element in sorted order
                int index = this.BinarySearch(value);

                if (index >= 0)
                {
                    this.Insert(index, value);
                }
                else
                {
                    this.Insert(~index, value);
                }
                return index;
            }
            else
            {
                // the list is not sorted, so append the value
                return base.Add(value);
            }
        }

        /// <summary>
        /// Adds a value to the sorted array using the comparer object. 
        /// If the array is sorted, the value inserted in sorted order. 
        /// Otherwise the value is appended to the array.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Returns the zero-based index the value was
        /// inserted at.</returns>
        public int Add(object value, IComparer comparer)
        {
            if (sorted)
            {
                // insert the element in sorted order
                int index = this.BinarySearch(value, comparer);                

                if (index >= 0)
                {
                    // item already exists in the array
                    this.Insert(index, value);
                }
                else
                {
                    // item not in the array
                    this.Insert(~index, value);
                }

                return index;
            }
            else
            {
                // the list is not sorted, so append the value
                return base.Add(value);
            }
        }

        /// <summary>
        /// Sort the array in the System.Collections.ArrayList 
        /// using the System.IComparable implementation of each
        /// element if the array is not already sorted.
        /// </summary>
        public override void Sort()
        {
            if (!sorted)
            {
                base.Sort();
            }            
            sorted = true;
        }

        /// <summary>
        /// Sort the array in the System.Collections.ArrayList 
        /// using the System.IComparable implementation of each
        /// element if the array is not already sorted.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <param name="comparer"></param>
        public override void Sort(int index, int count, IComparer comparer)
        {
            if (!sorted)
            {
                base.Sort(index, count, comparer);
            }
            sorted = true;
        }

        /// <summary>
        /// Sort the array in the System.Collections.ArrayList 
        /// using the System.IComparable implementation of each
        /// element if the array is not already sorted.
        /// </summary>
        /// <param name="comparer"></param>
        public override void Sort(IComparer comparer)
        {
            if (!sorted)
            {
                base.Sort(comparer);
            }
            sorted = true;
        }

        /// <summary>
        /// Clears the contents of the array and changes the
        /// type of array to not sorted. In order to sort the
        /// array again, a call must be made to Sort()
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            sorted = false;
        }        

        #endregion METHODS
    }
}
