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
    /// The predictive packet lists stores an array
    /// of PredictivePackets. Used to 
    /// compare a packet against all elements in
    /// the array. If all elements are contained 
    /// in the packet, then a match is made.
    /// </summary>
    class PredictivePacketList
    {
        #region MEMBERS

        private ArrayList predictivePackets;        

        #endregion MEMBERS

        #region CONSTRUCTOR_DESTRUCTOR

        /// <summary>
        /// Initializes a new instance of the PredictivePacketList class.
        /// </summary>
        public PredictivePacketList()
        {
            predictivePackets = new ArrayList();
        }

        #endregion CONSTRUCTOR_DESTRUCTOR

        #region METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet">byte[] of all bytes that
        /// compose the packet to compare.</param>
        /// <returns>Returns true if all PredictivePackets in the 
        /// PredictivePacketList are contained in the packet.</returns>
        public bool Search(byte[] packet)
        {
            PredictivePacket predictivePacket;
            bool result = false;
            int i = 0;
            
            while (!result && i < predictivePackets.Count)
            {
                predictivePacket = (PredictivePacket)predictivePackets[i];
                result = predictivePacket.Match(packet);
                i++;
            }

            // remove any packets that have expired
            this.RemoveExpiredPackets();
            return result;
        }

        /// <summary>
        /// Adds a PredictivePacket to the predictive packet list.
        /// </summary>
        /// <param name="packet">Allocated and populated PredictivePacket.</param>
        public void AddPacket(PredictivePacket packet)
        {
            predictivePackets.Add(packet);
        }

        /// <summary>
        /// Removes a PredictivePacket from the predictive packet list if the 
        /// PredictivePacket references matches an element in the PredictivePacketList.
        /// </summary>
        /// <param name="packet">A predictive packet reference.</param>
        public void RemovePacket(PredictivePacket packet)
        {
            predictivePackets.Remove(packet);
        }

        /// <summary>
        /// Removes the PredictivePacket located at the 
        /// specified zero-based index.
        /// </summary>
        /// <param name="index">The zero-based index of the PredictivePacket to remove.</param>
        public void RemovePacketAt(int index)
        {
            predictivePackets.RemoveAt(index);
        }

        /// <summary>
        /// Removes any expired PredictivePackets;
        /// </summary>
        public void RemoveExpiredPackets()
        {
            PredictivePacket packet;
            for (int i = 0; i < predictivePackets.Count; i++)
            {
                packet = (PredictivePacket)predictivePackets[i];

                // decrease the time to live
                packet.TTL--;

                if (!packet.IsAlive)
                {
                    // the packet is dead, remove it
                    predictivePackets.Remove(packet);
                }
            }
        }

        #endregion METHODS
    }
}
