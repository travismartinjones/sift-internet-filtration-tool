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
    /// Represents a collection of PredictivePacketElements.
    /// </summary>
    class PredictivePacket
    {
        #region MEMBERS

        // stores the PredictivePacketElements used to represent
        // pieces and parts of a packet to match against
        ArrayList   elements;   
        // the Time to Live for a packet. each time  a packet is
        // matched against, this value is decremented.        
        int         ttl;
        // the number of times the packet can be matched against.
        // decremented each time a packet is matched. if the value
        // is nagative, the number of times it can be matched
        // against is infinite
        uint        matchTime;

        #endregion MEMBERS
        
        #region CONSTRUCTOR_DESTRUCTOR                

        /// <summary>
        /// Initializes a new instance of the PredictivePacket class.
        /// </summary>
        public PredictivePacket()
        {
            elements = new ArrayList();
            ttl = 0;
            matchTime = 0;
        }

        #endregion CONSTRUCTOR_DESTRUCTOR

        #region PROPERTIES

        /// <summary>
        /// Get or sets the time to live value.
        /// </summary>
        public int TTL
        {
            get
            {
                return ttl;
            }
            set
            {
                ttl = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of times the PredictivePacket
        /// can be matched against. If the value is less than zero,
        /// the life is infinite.
        /// </summary>
        public uint MatchTime
        {
            get
            {
                return matchTime;
            }
            set
            {
                matchTime = value;
            }
        }

        /// <summary>
        /// Gets the alive status of the PredictivePacket.
        /// </summary>
        public bool IsAlive
        {
            get
            {
                if (matchTime != 0 && ttl > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion PROPERTIES

        #region METHODS

        /// <summary>
        /// Matches an array of bytes composing an ip packet
        /// to the PredictivePacket PredictivePacketElements.        
        /// </summary>
        /// <param name="packet">An array of bytes </param>
        /// <returns>Returns true   if the packet matches all PredictivePacketElements.        </returns>
        public bool Match(byte[] packet)
        {
            if (elements != null)
            {
                if (matchTime != 0 && ttl > 0)
                {
                    PredictivePacketElement element;
                    bool matched = true;
                    int i = 0;

                    // loop while the packet matches our predictivePacket
                    // and we have more elements to compare against
                    while (matched && i < elements.Count)
                    {
                        element = (PredictivePacketElement)elements[i];

                        if (element.Start + element.Data.Length <= packet.Length)
                        {
                            byte[] data;
                            data = element.Data;
                            int n = 0;

                            while (matched && n < element.Data.Length)
                            {
                                if (element.Data[n] != packet[n + element.Start])
                                {
                                    matched = false;
                                }

                                // increment our counter
                                n++;
                            }
                        }
                        else
                        {
                            // the element is not in the packet because
                            // the packet is not long enough to contain
                            // the element length + the element start
                            matched = false;
                        }

                        // increment the counter
                        i++;
                    }

                    if (matched)
                    {
                        // if we made a match, reduce the number of times
                        // the packet can be matched by 1, if the packet 
                        // is less than zero, the packet can be matched an
                        // infinite amount of times
                        if (matchTime > 0)
                        {
                            matchTime--;
                        }
                    }

                    return matched;
                }
            }

            return false;
        }

        /// <summary>
        /// Adds a PredictivePacketElement to the PredictivePacket list.
        /// </summary>
        /// <param name="element">An allocated and populated PredictivePacketElement.</param>
        /// <returns>Returns the zero-based index the element was added.</returns>
        public int AddElement(PredictivePacketElement element)
        {
            if (elements != null)
            {
                return elements.Add(element);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Removes a predictive packet element from the PredictivePacket 
        /// list.
        /// </summary>
        /// <param name="element">A PredictivePacketElement to remove
        /// from the PredictivePacket list.</param>
        public void RemoveElement(PredictivePacketElement element)
        {
            if (elements != null)
            {
                elements.Remove(element);
            }
        }

        #endregion METHODS
    }
}
