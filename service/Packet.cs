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
// remove for production
using System.Diagnostics;

namespace Sift
{
    /// <summary>
    /// Represents an ip packet returned from a lower
    /// adapter.
    /// </summary>
    class Packet
    {
        #region CONSTANTS

        const uint         ICMP_DATA_SIZE = 28;
        const uint         IP_TCP_PROTOCOL = 6;
        const uint         IP_UDP_PROTOCOL = 17;
        const int          ETHERNET_HEADER_SIZE = 14;
        const int          IP_HEADER_SIZE = 20;
        const int          TCP_HEADER_SIZE = 20;
        const int          UDP_HEADER_SIZE = 8;

        #endregion CONSTANTS

        #region MEMBERS

        private byte[]      rawData;    // stored the unmodified raw packet data
        private bool        isOutbound; // true - the packet is outbound, false - the packet is inbound
        private uint        id;         // the ID of the packet (lower 32 bits of the pointer)
        private byte        ipProtocol;
        private uint        sourceIP;
        private uint        destinationIP;
        private ushort      sourcePort;
        private ushort      destinationPort;
        private string      content;
        private bool        processed;
        private byte        dropStatus; // 0 - unset, 
                                        // 1 - PACKET_ACTION_DROP
                                        // 2 - PACKET_ACTION_ALLOW

        #endregion MEMBERS

        #region CONSTRUCTOR_DESTRUCTOR

        /// <summary>
        /// Initializes a new instance of the Packet class.
        /// </summary>
        public Packet()
        {
            // setup default values
            isOutbound = true;
            rawData = null;
            id = 0;
            ipProtocol = 0;
            sourceIP = 0;
            destinationIP = 0;
            sourcePort = 0;
            destinationPort = 0;
            content = null;
            dropStatus = 0;
            processed = false;
        }

        #endregion CONSTRUCTOR_DESTRUCTOR

        #region PROPERTIES

        /// <summary>
        /// Gets or sets the raw packet information.
        /// </summary>
        public byte[] RawData
        {
            get
            {
                return rawData;
            }
            set
            {                
                this.rawData = value;
            }
        }

        /// <summary>
        /// Gets an IMCP packet based on data in the
        /// packet's raw data.
        /// </summary>
        public byte[] ICMPPacket
        {
            get
            {
                byte[] packet = new byte[42];

                //////////////////
                // ETHERNET HEADER
                //////////////////

                // swap ethernet source & destination MACs
                Array.Copy(rawData, 6, packet, 0, 6);
                Array.Copy(rawData, 0, packet, 6, 6);
                // copy ethernet type
                packet[12] = 0x08;
                packet[13] = 0x00;

                //////////////////
                // IP HEADER
                //////////////////

                // copy ip version (4) and header length (5bytes)
                packet[14] = 0x45;
                // copy ip type
                packet[15] = 0x00;
                // total packet length (ushort)
                Array.Copy(BitConverter.GetBytes((ushort)System.Net.IPAddress.HostToNetworkOrder((short)(packet.Length - 14))), 0, packet, 16, sizeof(ushort));
                // ip identification (ushort)
                packet[18] = 0x95;
                packet[19] = 0xa3;
                // ip flags
                packet[20] = 0x00;
                // ip offset
                packet[21] = 0x00;
                // ip time to live
                packet[22] = 128;
                // ip protocol
                packet[23] = 0x06;
                // ip checksum, initialize to zero, calculate later
                packet[24] = 0;
                packet[25] = 0;
                // swap ip source and destination, could fail 
                // if detected as an IP spoof
                Array.Copy(rawData, 30, packet, 26, 4);
                Array.Copy(rawData, 26, packet, 30, 4);

                //////////////////
                // ICMP DATA
                //////////////////

                // icmp type "Destination Host Unreachable"
                packet[34] = 3;
                // icmp code "Packet Destination Administratively Prohibited"
                packet[35] = 13;
                // initialize checksum to zero
                packet[36] = 0;
                packet[37] = 0;

                // id
                Array.Copy(BitConverter.GetBytes((ushort)DateTime.Now.Millisecond), 0,
                           packet, 38, sizeof(ushort));                

                // sequence number
                Array.Copy(BitConverter.GetBytes((ushort)0), 0,
                           packet, 40, sizeof(ushort));

                // ip checksum
                Array.Copy(BitConverter.GetBytes(IPOnesComplement(packet)), 0, packet, 24, sizeof(ushort));

                return packet;
            }
        }
        
        /// <summary>
        /// Gets a TCP ACK RST packet based on data 
        /// in the packet's raw data.
        /// </summary>
        public byte[] TCPACKRSTPacket
        {
            get
            {
                byte[] packet = new byte[54];

                //////////////////
                // ETHERNET HEADER
                //////////////////

                // swap ethernet source & destination MACs
                Array.Copy(rawData, 6, packet, 0, 6);
                Array.Copy(rawData, 0, packet, 6, 6);
                // copy ethernet type
                packet[12] = 0x08;
                packet[13] = 0x00;

                //////////////////
                // IP HEADER
                //////////////////

                // copy ip version (4) and header length (5bytes)
                packet[14] = 0x45;
                // copy ip type
                packet[15] = 0x00;
                // total packet length (ushort)
                Array.Copy(BitConverter.GetBytes((ushort)System.Net.IPAddress.HostToNetworkOrder((short)(packet.Length - 14))), 0, packet, 16, sizeof(ushort));
                // ip identification (ushort)
                packet[18] = 0x95;
                packet[19] = 0xa3;
                // ip flags
                packet[20] = 0x00;
                // ip offset
                packet[21] = 0x00;
                // ip time to live
                packet[22] = 128;
                // ip protocol
                packet[23] = 0x06;
                // ip checksum, initialize to zero, calculate later
                packet[24] = 0;
                packet[25] = 0;
                // swap ip source and destination, could fail 
                // if detected as an IP spoof
                Array.Copy(rawData, 30, packet, 26, 4);
                Array.Copy(rawData, 26, packet, 30, 4);

                //////////////////
                // TCP HEADER
                //////////////////

                // swap source and destination ports
                Array.Copy(rawData, 36, packet, 34, 2);
                Array.Copy(rawData, 34, packet, 36, 2);
                // copy sequence number
                Array.Copy(rawData, 42, packet, 38, 4);
                // copy acknowledgement number, this could
                // really by any 32bit number we wished, but
                // to save on computation, we use the old ack #          

                //uint temp = BitConverter.ToUInt16(m_iRawData, 38);
                //temp = (uint)System.Net.IPAddress.NetworkToHostOrder((int)temp);
                //temp += 1;
                //temp = (uint)System.Net.IPAddress.HostToNetworkOrder((int)temp);
                //Array.Copy(BitConverter.GetBytes(temp), 0, packet, 42, sizeof(uint));

                Array.Copy(rawData, 38, packet, 42, 4);
                packet[45] += 100;

                // copy header length (5 bytes)
                packet[46] = 0x50;
                // tcp flags (Acknowledgement)
                packet[47] = 0x14;
                // fake the maximum window size that will be
                // accepted by the remote source, since the
                // request is denied, this value hold no importance
                packet[48] = 0xff;
                packet[49] = 0xff;
                // checksum calculated later
                packet[50] = 0;
                packet[51] = 0;
                // urgent pointer
                packet[52] = 0;
                packet[53] = 0;

                // calculate checksums, in reverse order, tcp, then ip
                Array.Copy(BitConverter.GetBytes(TCPOnesComplement(packet)), 0, packet, 50, sizeof(ushort));

                // ip checksum
                Array.Copy(BitConverter.GetBytes(IPOnesComplement(packet)), 0, packet, 24, sizeof(ushort));

                return packet;
            }
        }

        /// <summary>
        /// Gets a TCP ACK packet based on data in the
        /// packet's raw data.
        /// </summary>
        public byte[] TCPACKPacket
        {
            get
            {
                byte[] packet = new byte[54];

                //////////////////
                // ETHERNET HEADER
                //////////////////

                // swap ethernet source & destination MACs
                Array.Copy(rawData, 6, packet, 0, 6);
                Array.Copy(rawData, 0, packet, 6, 6);
                // copy ethernet type
                packet[12] = 0x08;
                packet[13] = 0x00;

                //////////////////
                // IP HEADER
                //////////////////

                // copy ip version (4) and header length (5bytes)
                packet[14] = 0x45;
                // copy ip type
                packet[15] = 0x00;
                // total packet length (ushort)
                Array.Copy(BitConverter.GetBytes((ushort)System.Net.IPAddress.HostToNetworkOrder((short)(packet.Length - 14))), 0, packet, 16, sizeof(ushort));
                // ip identification (ushort)
                packet[18] = 0x95;
                packet[19] = 0xa3;
                // ip flags
                packet[20] = 0x00;
                // ip offset
                packet[21] = 0x00;
                // ip time to live
                packet[22] = 128;
                // ip protocol
                packet[23] = 0x06;
                // ip checksum, initialize to zero, calculate later
                packet[24] = 0;
                packet[25] = 0;
                // swap ip source and destination, could fail 
                // if detected as an IP spoof
                Array.Copy(rawData, 30, packet, 26, 4);
                Array.Copy(rawData, 26, packet, 30, 4);

                //////////////////
                // TCP HEADER
                //////////////////

                // swap source and destination ports
                Array.Copy(rawData, 36, packet, 34, 2);
                Array.Copy(rawData, 34, packet, 36, 2);
                // copy sequence number
                Array.Copy(rawData, 42, packet, 38, 4);
                // copy acknowledgement number, this could
                // really by any 32bit number we wished, but
                // to save on computation, we use the old ack #          

                //uint temp = BitConverter.ToUInt16(m_iRawData, 38);
                //temp = (uint)System.Net.IPAddress.NetworkToHostOrder((int)temp);
                //temp += 1;
                //temp = (uint)System.Net.IPAddress.HostToNetworkOrder((int)temp);
                //Array.Copy(BitConverter.GetBytes(temp), 0, packet, 42, sizeof(uint));

                Array.Copy(rawData, 38, packet, 42, 4);
                packet[45] += 100;

                // copy header length (5 bytes)
                packet[46] = 0x50;
                // tcp flags (Acknowledgement)
                packet[47] = 0x10;
                // fake the maximum window size that will be
                // accepted by the remote source, since the
                // request is denied, this value hold no importance
                packet[48] = 0xff;
                packet[49] = 0xff;
                // checksum calculated later
                packet[50] = 0;
                packet[51] = 0;
                // urgent pointer
                packet[52] = 0;
                packet[53] = 0;

                // calculate checksums, in reverse order, tcp, then ip
                Array.Copy(BitConverter.GetBytes(TCPOnesComplement(packet)), 0, packet, 50, sizeof(ushort));

                // ip checksum
                Array.Copy(BitConverter.GetBytes(IPOnesComplement(packet)), 0, packet, 24, sizeof(ushort));

                return packet;
            }
        }

        /// <summary>
        /// Gets an HTTP response packet based on data 
        /// in the packet's raw data. INCOMPLETE
        /// </summary>
        public byte[] HTTPPacket
        {
            get
            {
                string contentMain =
                    @"<html><head><title>Filter Denied</title></head>" +
                    @"<body bgcolor=""ff0000"">DENIED</body></html>" + "\r\n\r\n";

                string contentHead =
                    "HTTP/1.1 200 OK\r\n" +
                    "Cache-Control: private\r\n" +
                    "Content-Type: text/html\r\n" +
                    "Server: GWS/2.1\r\n" +
                    "Content-Length: " + contentMain.Length +
                    "\r\n";

                char[] content = ((string)(contentHead + contentMain)).ToCharArray();

                byte[] packet = new byte[54 + content.Length];

                //////////////////
                // ETHERNET HEADER
                //////////////////

                // swap ethernet source & destination MACs
                Array.Copy(rawData, 6, packet, 0, 6);
                Array.Copy(rawData, 0, packet, 6, 6);
                // copy ethernet type
                packet[12] = 0x08;
                packet[13] = 0x00;

                //////////////////
                // IP HEADER
                //////////////////

                // copy ip version (4) and header length (5bytes)
                packet[14] = 0x45;
                // copy ip type
                packet[15] = 0x00;
                // total packet length (ushort)
                Array.Copy(BitConverter.GetBytes((ushort)System.Net.IPAddress.HostToNetworkOrder((short)(packet.Length - 14))), 0, packet, 16, sizeof(ushort));
                // ip identification (ushort)
                packet[18] = 0x95;
                packet[19] = 0xd3;
                // ip flags
                packet[20] = 0x00;
                // ip offset
                packet[21] = 0x00;
                // ip time to live
                packet[22] = 128;
                // ip protocol
                packet[23] = 0x06;
                // ip checksum, initialize to zero, calculate later
                packet[24] = 0;
                packet[25] = 0;
                // swap ip source and destination, could fail 
                // if detected as an IP spoof
                Array.Copy(rawData, 30, packet, 26, 4);
                Array.Copy(rawData, 26, packet, 30, 4);

                //////////////////
                // TCP HEADER
                //////////////////

                // swap source and destination ports
                Array.Copy(rawData, 36, packet, 34, 2);
                Array.Copy(rawData, 34, packet, 36, 2);
                // copy sequence number
                Array.Copy(rawData, 42, packet, 38, 4);
                // copy acknowledgement number, this could
                // really by any 32bit number we wished, but
                // to save on computation, we use the old ack #          

                //uint temp = BitConverter.ToUInt16(m_iRawData, 38);
                //temp = (uint)System.Net.IPAddress.NetworkToHostOrder((int)temp);
                //temp += 1;
                //temp = (uint)System.Net.IPAddress.HostToNetworkOrder((int)temp);
                //Array.Copy(BitConverter.GetBytes(temp), 0, packet, 42, sizeof(uint));                 

                Array.Copy(rawData, 38, packet, 42, 4);
                packet[45] += 100;

                // copy header length (5 bytes)
                packet[46] = 0x50;
                // tcp flags (ACK,FIN)
                packet[47] = 0x18;
                // fake the maximum window size that will be
                // accepted by the remote source, since the
                // request is denied, this value hold no importance
                packet[48] = 0xff;
                packet[49] = 0xff;
                // checksum calculated later
                packet[50] = 0;
                packet[51] = 0;
                // urgent pointer
                packet[52] = 0;
                packet[53] = 0;

                //////////////////
                // HTTP CONTENTS
                //////////////////

                Array.Copy(System.Text.Encoding.UTF8.GetBytes(content), 0, packet, 54, content.Length);

                //////////////////
                // CHECKSUM CALC
                //////////////////

                // calculate checksums, in reverse order, tcp, then ip
                Array.Copy(BitConverter.GetBytes(TCPOnesComplement(packet)), 0, packet, 50, sizeof(ushort));

                // ip checksum
                Array.Copy(BitConverter.GetBytes(IPOnesComplement(packet)), 0, packet, 24, sizeof(ushort));

                return packet;
            }
        }

        /// <summary>
        /// Gets or sets if the packet has been
        /// processed by the filter service.
        /// </summary>
        public bool Processed
        {
            get
            {
                return processed;
            }
            set
            {
                processed = value;
            }
        }

        /// <summary>
        /// Gets or sets the packet ID.
        /// </summary>
        public uint ID
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        /// <summary>
        /// Gets or sets the ip packet protocol.
        /// </summary>
        public byte IPProtocol
        {
            get
            {
                return this.ipProtocol;
            }
            set
            {
                this.ipProtocol = value;
            }
        }

        /// <summary>
        /// Gets or sets the packet source ip.
        /// </summary>
        public uint SourceIP
        {
            get
            {
                return this.sourceIP;
            }
            set
            {
                this.sourceIP = value;
            }
        }

        /// <summary>
        /// Gets or sets the packet destination IP.
        /// </summary>
        public uint DestinationIP
        {
            get
            {
                return this.destinationIP;
            }
            set
            {
                this.destinationIP = value;
            }
        }

        /// <summary>
        /// Gets or sets the packet source port.
        /// </summary>
        public ushort SourcePort
        {
            get
            {
                return this.sourcePort;
            }
            set
            {
                this.sourcePort = value;
            }
        }

        /// <summary>
        /// Gets or sets the packet destination port.
        /// </summary>
        public ushort DestinationPort
        {
            get
            {
                return this.destinationPort;
            }
            set
            {
                this.destinationPort = value;
            }
        }

        /// <summary>
        /// Gets or sets the drop status. 1 - drop, 2 - allow
        /// </summary>
        public byte DropStatus
        {
            get
            {
                return this.dropStatus;
            }
            set
            {
                this.dropStatus = value;
            }
        }

        /// <summary>
        /// Gets the packet domain if the packet contains
        /// an http request.
        /// </summary>
        public string Domain
        {
            get
            {
                if (this.content == null)
                {
                    return "";
                }
                else
                {
                    Match m;
                    m = Regex.Match(this.content, @"Host:(.*)\n");
                    if (m.Success)
                    {
                        return Regex.Replace(m.Result("$1"), @"\s", "");
                    }
                    else
                    {
                        return "";
                    }
                }
            }
        }

        /// <summary>
        /// Gets the packet URL if the packet contains
        /// an http request.
        /// </summary>
        public string URL
        {
            get
            {
                if (this.content == null)
                {
                    return "";
                }
                else
                {
                    Match m;

                    m = Regex.Match(this.content, @"GET(.*)HTTP|POST(.*)HTTP");
                    if (m.Success)
                    {
                        return Regex.Replace(m.Result("$1"), @"\s", "");
                    }
                    else
                    {
                        return "";
                    }
                }
            }
        }

        /// <summary>
        /// True if the packet is outbound on the
        /// specific adapter.
        /// </summary>
        public bool IsOutbound
        {            
            get
            {
                return isOutbound;
            }
        }

        /// <summary>
        /// Gets the packet content if the packet contains
        /// an http request.
        /// </summary>
        public string Content
        {
            get
            {
                return this.content;
            }
            set
            {
                this.content = value;
            }
        }

        #endregion PROPERTIES

        #region METHODS

        /// <summary>
        /// Calculates the tcp ones complement on an array
        /// of bytes.
        /// </summary>
        /// <param name="data">The array of bytes to
        /// calculate the tcp ones complement</param>
        /// <returns>Returns the tcp ones complement of the 
        /// byte array.</returns>
        public ushort TCPOnesComplement(byte[] data)
        {
            //calculate checksum
            uint checkSum = 0;

            // calculate source/destination ip + tcp header + tcp data
            for (int i = 26; i < data.Length; i+=2)
                if (i == data.Length - 1)
                    checkSum += (uint)((data[i] << 8) & 0xff00);
                else
                    checkSum += (uint)(((data[i] << 8) & 0xff00) + (data[i + 1] & 0xff));

            // add protocol and length
            checkSum += (ushort)6;
            checkSum += (ushort)(data.Length - 34);

            while(checkSum >> 16 != 0)
                checkSum = (checkSum >> 16) + (checkSum & 0xffff);

            checkSum = (uint)(((checkSum >> 8) & 0xff) + ((checkSum << 8) & 0xff00));

            return (ushort)~checkSum;
        }

        /// <summary>
        /// Calculates the ip ones complement on an array
        /// of bytes.
        /// </summary>
        /// <param name="data">The array of bytes to
        /// calculate the ip ones complement</param>
        /// <returns>Returns the ip ones complement of the 
        /// byte array.</returns>
        public ushort IPOnesComplement(byte[] data)
        {
            //calculate checksum
            int checkSum = 0;

            // since our ip headers will always by 20 bytes
            // long, we do not need to adjust for odd lengths
            for (int i = 14; i < 34; i += 2)
                checkSum += Convert.ToInt32(BitConverter.ToUInt16(data, i));

            while (checkSum >> 16 != 0)
                checkSum = (checkSum >> 16) + (checkSum & 0xffff);

            return (ushort)~checkSum;
        }

        /// <summary>
        /// Prints the formatted contents to the Event Log.
        /// </summary>
        public void PrintContents()
        {
            EventLog.WriteEntry("Sift","-Packet contents-");
            EventLog.WriteEntry("Sift","Packet ID       : " + this.id);
            EventLog.WriteEntry("Sift","Source IP       : " + this.sourceIP);
            EventLog.WriteEntry("Sift","Destination IP  : " + this.destinationIP);
            EventLog.WriteEntry("Sift","Source Port     : " + this.sourcePort);
            EventLog.WriteEntry("Sift","Destination Port: " + this.destinationPort);
            EventLog.WriteEntry("Sift","Content Length  : " + this.content.Length);
            EventLog.WriteEntry("Sift",this.content);
            Match m;
            m = Regex.Match(this.content, "Host:(.*)\\n");
            if (m.Success)
            {
                EventLog.WriteEntry("Sift","Host            : " + Regex.Replace(m.Result("$1"),"\\s",""));
            }
        }

        /// <summary>
        /// Converts an Int32 to a formatted ip string.
        /// </summary>
        /// <param name="ip">A 32bit internet packet address.</param>
        /// <returns>A formatted ip string.</returns>
        public string IPToString(uint ip)
        {
            byte[] ipBytes;

            ipBytes = BitConverter.GetBytes(ip);

            return ipBytes[3].ToString() + "." + ipBytes[2].ToString() + "." +
                   ipBytes[1].ToString() + "." + ipBytes[0].ToString();
        }

        #endregion METHODS
    }
}
