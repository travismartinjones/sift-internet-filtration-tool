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
    /// A representation of a single portion of a byte
    /// array used to compose an internet packet.
    /// </summary>
    class PredictivePacketElement
    {
        #region MEMBERS

        uint   start;   // the zero based index the data begins at
        byte[] data;    // the byte array containing the data to be matched

        #endregion MEMBERS

        #region CONSTRUCTOR_DESTRUCTOR

        /// <summary>
        /// Initializes a new instance of the PredictivePacketElement class.
        /// </summary>
        public PredictivePacketElement()
        {
            this.start = 0;
            this.data = null;
        }

        /// <summary>
        /// Initializes a new instance of the PredictivePacketElement
        /// class.
        /// </summary>
        /// <param name="start">The zero-based index the data byte
        /// array begins at in the matching packet.</param>
        /// <param name="data">The byte array data to match against
        /// received packets.</param>
        public PredictivePacketElement(uint start, byte[] data)
        {            
            this.start = start;
            this.data = data;
        }

        #endregion CONSTRUCTOR_DESTRUCTOR

        #region PROPERTIES

        /// <summary>
        /// Get or sets a the zero-based start of the data byte
        /// array being at.
        /// </summary>
        public uint Start
        {
            get
            {                
                return start;
            }
            set
            {
                start = value;
            }
        }

        /// <summary>
        /// Gets or sets the byte array data used to match against
        /// received packets.
        /// </summary>
        public byte[] Data
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
            }
        }

        #endregion PROPERTIES
    }
}
