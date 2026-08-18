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
using System.Runtime.InteropServices;
using System.Collections;
using System.Text;
using System.Threading;
using Microsoft.Win32;

// used to access config data
using System.Data;
using System.Xml;

namespace Sift
{
    /// <summary>
    /// The main class used to store all filter object instances.
    /// The primary class used to interract with the filter driver.
    /// </summary>
    class Filter : Object
    {
        #region STRUCTURES

        [StructLayout(LayoutKind.Explicit)]
        public struct IPHeader
        {
            [FieldOffset(0)] public byte    version_length;
            [FieldOffset(1)] public byte    type;
            [FieldOffset(2)] public ushort  packetLength;
            [FieldOffset(4)] public ushort  id;
            [FieldOffset(6)] public ushort  offset;
            [FieldOffset(8)] public byte    ttl;
            [FieldOffset(9)] public byte    protocol;
            [FieldOffset(10)] public ushort checksum;
            [FieldOffset(12)] public ulong  source;
            [FieldOffset(16)] public ulong  destination;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct TCPHeader
        {
            [FieldOffset(0)] public ushort sourcePort;
            [FieldOffset(2)] public ushort destinationPort;
            [FieldOffset(4)] public ulong  sequenceNum;
            [FieldOffset(8)] public ulong  acknowledgementNum;
            [FieldOffset(12)] public byte  unused_offset;
            [FieldOffset(13)] public byte  flags;
            [FieldOffset(14)] public ushort window;
            [FieldOffset(16)] public ushort checksum;
            [FieldOffset(18)] public ushort urgentPointer;
        }

        //public class ICMPPacket
        //{
        //    public byte type;
        //    public byte subcode;
        //    public ushort checksum;
        //    public ushort identifier;
        //    public ushort sequenceNum;
        //    public byte[] data;
        //}

        #endregion STRUCTURES

        #region CONSTANTS       

        // file access constants
        private const uint FILE_SHARE_READ   = 1;
        private const uint FILE_SHARE_WRITE  = 2;
        private const uint FILE_SHARE_DELETE = 4;


        private const uint GENERIC_READ    = 0x80000000;
        private const uint GENERIC_WRITE   = 0x40000000;
        private const uint GENERIC_EXECUTE = 0x20000000;
        private const uint GENERIC_ALL     = 0x10000000;

        // file creation disposition constant
        private const uint CREATE_NEW         = 1;
        private const uint CREATE_ALWAYS      = 2;
        private const uint OPEN_EXISTING      = 3;
        private const uint OPEN_ALWAYS        = 4;
        private const uint TRUNCATE_EXISTING  = 5;

        //private const uint OPEN_EXISTING = 0x00000003;

        // file attributes constant
        private const uint FILE_ATTRIBUTE_READONLY            = 1;
        private const uint FILE_ATTRIBUTE_HIDDEN              = 2;
        private const uint FILE_ATTRIBUTE_SYSTEM              = 4;
        private const uint FILE_ATTRIBUTE_DIRECTORY           = 0x00000010;
        private const uint FILE_ATTRIBUTE_ARCHIVE             = 0x00000020;
        private const uint FILE_ATTRIBUTE_DEVICE              = 0x00000040;
        private const uint FILE_ATTRIBUTE_NORMAL              = 0x00000080;
        private const uint FILE_ATTRIBUTE_TEMPORARY           = 0x00000100;
        private const uint FILE_ATTRIBUTE_SPARSE_FILE         = 0x00000200;
        private const uint FILE_ATTRIBUTE_REPARSE_POINT       = 0x00000400;
        private const uint FILE_ATTRIBUTE_COMPRESSED          = 0x00000800;
        private const uint FILE_ATTRIBUTE_OFFLINE             = 0x00001000;
        private const uint FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x00002000;
        private const uint FILE_ATTRIBUTE_ENCRYPTED           = 0x00004000;

        // file flags
        private const uint FILE_FLAG_NO_BUFFERING             = 0x20000000;
        private const uint FILE_FLAG_OVERLAPPED               = 0x40000000;


        // file errors
        private const uint ERROR_IO_PENDING = 997;


        // invalid handle constant
        private const int INVALID_HANDLE_VALUE = -1;

        // iocontrol codes as defined in driver's hook.h
        private const uint IOCTL_FILTERHOOK_LIST_ADAPTERS            = 1236996;
        private const uint IOCTL_FILTERHOOK_OPEN_ADAPTER             = 1237000;
        private const uint IOCTL_FILTERHOOK_CLOSE_ADAPTER            = 1237004;
        private const uint IOCTL_FILTERHOOK_UPDATE_ADAPTER_SETTING   = 1237008;
        private const uint IOCTL_FILTERHOOK_GET_PACKETS              = 1237016;
        private const uint IOCTL_FILTERHOOK_SET_PACKETS              = 1237020;
        private const uint IOCTL_FILTERHOOK_PACKET_RECEIVED          = 1237027;
        private const uint IOCTL_FILTERHOOK_SENDALL_PACKETS          = 1237028;
        private const uint IOCTL_FILTERHOOK_USERMODE_SHUTDOWN        = 1237059;
        private const uint IOCTL_FILTERHOOK_RECV_PACKETS             = 1237060;
        private const uint IOCTL_FILTERHOOK_SEND_PACKET              = 1237061;

        // adapter setting flags
        private const uint USER_MODE_SETTINGS_OPEN_ADAPTER  = 1;   // 0000 0001 - indicates if an adapter is open
        private const uint USER_MODE_SETTINGS_DROP_DEFAULT  = 2;   // 0000 0010 - if set, all packets are dropped
        private const uint USER_MODE_SETTINGS_CAPTURE_RECV  = 4;   // 0000 0100 - capture receives
        private const uint USER_MODE_SETTINGS_CAPTURE_SEND  = 8;   // 0000 1000 - capture sends
        private const uint USER_MODE_SETTINGS_CAPTURE_HTTP  = 16;  // 0001 0000 - capture HTTP
        private const uint USER_MODE_SETTINGS_CAPTURE_TCPIP = 32;  // 0010 0000 - capture all TCP/IP not just HTTP
        private const uint USER_MODE_SETTINGS_CAPTURE_UDPIP	= 64;  // 0100 0000 - capture all UDP/IP packets
        private const uint USER_MODE_SETTINGS_CAPTURE_ALL	= 128; // 1000 0000 - capture all IP traffic
		        										           //             if IP is not TCP or UDP, no socket
												                   //             information is captured

        private const byte PACKET_ACTION_UNSET    = 0;  // 0000 0000 - indicates a packet that has not beed filtered
        private const byte PACKET_ACTION_DROP     = 1;  // 0000 0001 - indicates to drop the packet
        private const byte PACKET_ACTION_ALLOW	  = 2;  // 0000 0010 - indicates to allow the packet
        private const byte PACKET_ACTION_INBOUND  = 4;  // 0000 0100 - indicates to recv the packet
        private const byte PACKET_ACTION_OUTBOUND = 8;  // 0000 1000 - indicates to send the packet

        // used for WaitForSingleObject DLL Import
        private const uint INFINITE = 0xffff;

        #endregion CONSTANTS

        #region IMPORTS

        // Import definitions taken from MSDN library

        [DllImport("kernel32", SetLastError = true)]
        static extern unsafe IntPtr CreateFile(
              string FileName,                    // file name
              uint DesiredAccess,                 // access mode
              uint ShareMode,                     // share mode
              uint SecurityAttributes,            // Security Attributes
              uint CreationDisposition,           // how to create
              uint FlagsAndAttributes,            // file attributes
              int hTemplateFile                   // handle to template file
              );

        [DllImport("kernel32", SetLastError = true)]
        static extern unsafe bool ReadFile(
              IntPtr hFile,                       // handle to file
              void* pBuffer,                      // data buffer
              int NumberOfBytesToRead,            // number of bytes to read
              int* pNumberOfBytesRead,            // number of bytes read
              int Overlapped                      // overlapped buffer
              );

        [DllImport("kernel32", SetLastError = true)]
        private static extern unsafe bool WriteFile(
            IntPtr hFile,					// handle to file
            void* pBuffer,				// pointer to the buffer to write
            uint NumberOfBytesToWrite,	// number of bytes to write from the buffer
            uint* NumberOfBytesWritten,	// [out] number of byes written to the file
            uint Overlapped);			// used for async reading and writing

        [DllImport("kernel32", SetLastError = true)]
        static extern unsafe bool CloseHandle(
              IntPtr hObject   // handle to object
              );

        [DllImport("kernel32", SetLastError = true)]
        private static extern unsafe uint DeviceIoControl(
            IntPtr hDevice,				// handle of the device
            uint IoControlCode,			// IO control code to execute
            void* pBuffer,				// Input buffer for the execution
            uint InBufferSize,			// size of the input buffer
            void* OutBuffer,				// [out] output buffer for the execution
            uint OutBufferSize,			// [size of the output buffer
            uint* BytesReturned,			// [out] number of bytes returned
            uint Overlapped);			// used for async reading and writing

        [DllImport("kernel32", SetLastError = true)] 
        private static extern unsafe int WaitForSingleObject( 
            IntPtr hHandle,
            int dwMilliseconds);

        [DllImport("kernel32", SetLastError = true)]
        private static extern unsafe uint WaitForSingleObject(
            uint hHandle,
            uint dwMilliseconds);

        #endregion IMPORTS

        #region MEMBERS

        /// <summary>
        /// IntegerPointer to hold the handle of the driver
        /// </summary>
        private IntPtr driverHandle;
        /// <summary>
        /// IntegerPointer to hold the handle to the PacketReceived IRP
        /// </summary>
        private IntPtr packetReceivedIRP;
        /// <summary>
        /// Bool to hold whether we have a connection to the driver
        /// </summary>
        private volatile bool driverOpened;
        /// <summary>
        /// Stores all adapters loaded as indicated from
        /// the filter driver.
        /// </summary>
        private Adapter[] adapters;

        #region Threads
        /// <summary>
        /// An independent thread that loops until the filter
        /// class is closing. Waits until the filter driver
        /// indicates that a packet has been received. Once
        /// triggered it initiates a single filter processing
        /// cycle.
        /// </summary>
        private Thread threadReceivedPacketIRPHandler;
        private Thread threadConfigurationLoader = null;
        private Thread threadListUpdater = null;
        #endregion

        #region Locking Objects
        private object lockListUpdating = new object();
        #endregion

        private bool IsConfigurationLoaded = false;
        private bool IsListLoadComplete = false;

        private FilterActionList filterBlackList;
        private FilterActionList filterWhiteList;
        private PredictivePacketList predictivePacketBlackList;
        private PredictivePacketList predictivePacketWhiteList;

        private RemotingServer remotingServer;

        #endregion MEMBERS

        # region CONSTRUCTOR_DESTRUCTOR

        /// <summary>
        /// Initializes a new instance of the Filter class.
        /// </summary>
        public Filter()
        {
            Sift.Resources.Globals.Log.Write("New Filter instance created", Sift.Resources.Types.LogType.Information, Sift.Resources.Types.LogGroupType.ServiceDebug, Sift.Resources.Types.LogDetailType.Verbose);
            // initialize default member values            
            this.driverHandle = IntPtr.Zero;
            this.packetReceivedIRP = IntPtr.Zero; 
            this.driverOpened = false;
            this.adapters = null;
            threadReceivedPacketIRPHandler = new Thread(new ThreadStart(this.ReceivedPacketIRPThreadHandler));
            
            // setup and start the list updater
            threadListUpdater = new Thread(new ThreadStart(ListUpdatesAutomatedDownload));
            threadListUpdater.Start();            

            filterBlackList = new FilterActionList();
            filterWhiteList = new FilterActionList();
            predictivePacketBlackList = new PredictivePacketList();
            predictivePacketWhiteList = new PredictivePacketList();

            remotingServer = new RemotingServer(Sift.Resources.Constants.RemotingPort);
        }

        /// <summary>
        /// Closes all open connections.
        /// </summary>
        ~Filter()
        {    
            // if the driver is opened, close it before we
            // destroy the Sift instance

            if (driverOpened)
            {                
                CloseConnection();
            }

            //if(threadReceivedPacketIRPHandler.IsAlive)
            //    threadReceivedPacketIRPHandler.Abort();
        }

        #endregion CONSTRUCTOR_DESTRUCTOR

        #region METHODS        

        #region Driver Communication

        #region Connection Handlers

        /// <summary>
        /// Opens a connection to the Sift filter driver.
        /// </summary>
        /// <returns>True if a valid file handle was opened with the filter driver.
        /// returns False if there was any problem with the connection attempt.</returns>
        public bool OpenConnection()
        {
            // open the file handle            
            this.driverHandle = CreateFile(Sift.Resources.Constants.DriverName,
                 GENERIC_READ | GENERIC_WRITE, 0, 0, OPEN_EXISTING, FILE_FLAG_OVERLAPPED, 0);

            // error opening the file handle, return false to
            // indicate the failure                     
            if ((int)this.driverHandle <= 0)
            {
                Sift.Resources.Globals.Log.Write("Unable to establish communication with filter driver:\n\n" + Marshal.GetLastWin32Error(), Sift.Resources.Types.LogType.Error, Sift.Resources.Types.LogGroupType.DriverDebug, Sift.Resources.Types.LogDetailType.Minimal);
                this.driverOpened = false;
                return false;
            }

            // we have a valid handle to the opened driver
            this.driverOpened = true;
            // retrieve a list of all available network adapters
            // connected to the filter driver
            Adapter tempAdapter = new Adapter();
            adapters = tempAdapter.UnicodeToAdapter(RetrieveAdapterList());

            // open the file handle used by the received packets thread
            this.packetReceivedIRP = CreateFile(Sift.Resources.Constants.DriverName,
                                                GENERIC_READ | GENERIC_WRITE, 0, 0,
                                                OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, 0);

            // check to see if we were successful with creating the connection
            if ((int)this.packetReceivedIRP <= 0)
            {
                // we were unable to create a valid IRP                
                Sift.Resources.Globals.Log.Write("Unable to establish communication with filter driver:\n\n" + Marshal.GetLastWin32Error(), Sift.Resources.Types.LogType.Error, Sift.Resources.Types.LogGroupType.DriverDebug, Sift.Resources.Types.LogDetailType.Minimal);
                this.packetReceivedIRP = IntPtr.Zero;                
                return false;
            }
            else
            {
                // valid IRP created, start the thread
                threadReceivedPacketIRPHandler.Start();
            }

            Sift.Resources.Globals.Log.Write("OpenConnection successfull" + Sift.Resources.Constants.DriverName, Sift.Resources.Types.LogType.Information, Sift.Resources.Types.LogGroupType.DriverDebug, Sift.Resources.Types.LogDetailType.Moderate);
            return this.driverOpened;
        }

        /// <summary>
        /// Closes any open connection to the Sift filter driver.
        /// </summary>
        /// <returns>True if the operation was successful.</returns>
        public bool CloseConnection()
        {
            if (this.driverOpened)
            {
                // the driver is open
                // close any open adapters
                foreach (Adapter adapter in adapters)
                {
                    if (adapter.IsOpen)
                    {
                        this.CloseAdapter(adapter);
                    }
                }

                driverOpened = false;

                // if the thread IRP has been created, close the connection
                if (packetReceivedIRP != IntPtr.Zero)
                {
                    uint kernelLength;
                    // signal to the driver to complete our thread's pending IRP
                    unsafe
                    {
                        DeviceIoControl(this.driverHandle, IOCTL_FILTERHOOK_USERMODE_SHUTDOWN,
                                        null, 0, null, 0,
                                        &kernelLength, 0);
                    }
                    // close the thread handle
                    CloseHandle(packetReceivedIRP);
                    packetReceivedIRP = IntPtr.Zero;
                }

                // close the handle
                CloseHandle(this.driverHandle);

                return true;
            }
            else
            {
                // the driver is already closed
                return true;
            }
        }

        #endregion

        #region Packet Handlers

        /// <summary>
        /// Gets all packets pending filtering from the filter driver.
        /// </summary>
        /// <param name="adapter">The adapter to pull pending packets
        /// from.</param>
        public void GetPackets(Adapter adapter)
        {
            // stores the information sent to the kernel mode driver
            // in this case, the adapter name we are requesting for
            byte[] outputBuffer;

            // temporary buffer to store adapter information returned
            // from kernel mode driver
            byte[] kernelBuffer = new byte[2048];
            // length of information returned from kernel mode driver
            uint kernelLength = 0;

            // make sure the driver is opened before
            // attempting to query from it
            if (!this.driverOpened)
            {
                return;
            }

            outputBuffer = adapter.UnicodeName;

            unsafe
            {
                fixed (void* vpOutputBuffer = outputBuffer, vpKernelBuffer = kernelBuffer)
                {
                    DeviceIoControl(this.driverHandle, IOCTL_FILTERHOOK_GET_PACKETS,
                                           vpOutputBuffer, (uint)outputBuffer.Length, vpKernelBuffer, (uint)kernelBuffer.Length,
                                           &kernelLength, 0);
                }
            }

            if (kernelLength > 0)
            {
                // the call to GetPackets was successfull
                // process the returned information
                ParseGetPackets(kernelBuffer, adapter);
            }
        }

        /// <summary>
        /// Sends a raw packet.
        /// </summary>
        /// <param name="packet">The raw byte data comprising the packet.</param>
        /// <returns></returns>
        private void SendPacket(byte[] packet)
        {
            // length of information returned from kernel mode driver
            uint kernelLength = 0;

            unsafe
            {
                fixed (void* vpOutputBuffer = packet)
                {
                    DeviceIoControl(this.driverHandle, IOCTL_FILTERHOOK_SEND_PACKET,
                                    vpOutputBuffer, (uint)packet.Length, null, 0,
                                    &kernelLength, 0);
                }
            }

            // otherwise the packet was sent
            Sift.Resources.Globals.Log.Write("Packet sent: " + kernelLength.ToString() + "bytes written", Sift.Resources.Types.LogType.Information, Sift.Resources.Types.LogGroupType.DriverDebug, Sift.Resources.Types.LogDetailType.Verbose);
        }

        /// <summary>
        /// Sends a ACK RCV packet to the source IP to close the
        /// TCP connection.
        /// </summary>
        /// <param name="adapter"></param>
        public void SetRecvDrop(Adapter adapter)
        {
            // process any packets in that have been
            // added to the adapter
            if (adapter.PacketCount > 0)
            {
                byte[] tempPacket;
                byte[] adapterName;  //byte[] of adapter name
                byte[] outputBuffer;
                int currentStart = 0;
                int packetDataLength = 0;
                ArrayList packetData = new ArrayList();
                Packet packet;

                // length of information returned from kernel mode driver
                uint kernelLength = 0;

                // make sure the driver is opened before
                // attempting to query from it
                if (!this.driverOpened)
                {
                    return;
                }

                // send HTTP response to the requestor
                for (int i = 0; i < adapter.PacketCount; i++)
                {
                    packet = adapter.GetPacket(i);
                    if (packet.DropStatus == PACKET_ACTION_DROP)
                    {
                        if (packet.IPProtocol == 6 && packet.Content != null)
                        {
                            //if(loggingType = web page)
                            //{
                            //tempPacket = packet.TCPACKPacket;
                            //packetDataLength += tempPacket.Length;
                            //packetData.Add(tempPacket);

                            //tempPacket = packet.HTTPPacket;
                            //packetDataLength += tempPacket.Length;
                            //packetData.Add(tempPacket);


                            //// create a new predictive packet to capture the ACK response
                            //// from the client. if we did not capture and drop the packet, 
                            //// the remote server would view this as a TCP connection 
                            //// attempt and would respond, causing a TCP ACK loop
                            //cPredictivePacket predictivePacket = new cPredictivePacket();

                            //// match the TCP/IP ACK return
                            //byte[] ipVersion = new byte[1];
                            //ipVersion[0] = 0x45;
                            //predictivePacket.AddElement(new cPredictivePacketElement(14, ipVersion));
                            //byte[] ipProtocol = new byte[1];
                            //ipProtocol[0] = 0x06;
                            //predictivePacket.AddElement(new cPredictivePacketElement(23, ipProtocol));

                            //// match the source/destination IP and ports
                            //byte[] ipPort = new byte[12];
                            //Array.Copy(packet.RawData,26,ipPort,0,12);
                            //predictivePacket.AddElement(new cPredictivePacketElement(26, ipPort));

                            //// match the ACK response
                            //byte[] tcpFlags = new byte[1];
                            //tcpFlags[0] = 0x10;
                            ////predictivePacket.AddElement(new cPredictivePacketElement(47, tcpFlags));

                            //// allow for 100 comparisons, then the packet should
                            //// be removed from the blacklist, in reality it will
                            //// be matched in the first few comparisons
                            //predictivePacket.TTL = 100;

                            //// allow for one match against a packet
                            //predictivePacket.MatchTime = 1;

                            //// add the predictive packet to the black list
                            //m_cPredictivePacketBlackList.AddPacket(predictivePacket);
                            //}
                            //else
                            //{

                            // packet is TCP/IP, send a TCP ACK/RST
                            // return packet to close the socket connection
                            tempPacket = packet.TCPACKRSTPacket;
                            packetDataLength += tempPacket.Length;
                            packetData.Add(tempPacket);
                        }
                        else
                        {
                            Sift.Resources.Globals.Log.Write("Sending ICMP packet", Sift.Resources.Types.LogType.Information, Sift.Resources.Types.LogGroupType.DriverDebug, Sift.Resources.Types.LogDetailType.Verbose);
                            // packet is not TCP/IP, send a ICMP
                            // destination host unreachable packet
                            tempPacket = packet.ICMPPacket;
                            packetDataLength += tempPacket.Length;
                            packetData.Add(tempPacket);
                        }
                    }
                }

                // exit if we have no packets to drop
                if (packetData.Count == 0)
                {
                    return;
                }

                // get the adapter name in byte[]
                adapterName = adapter.UnicodeName;

                // create a buffer to store the adapter name, the packet bytes
                outputBuffer = new byte[sizeof(uint) +
                                        adapterName.Length +
                                        packetDataLength +
                                        (packetData.Count * sizeof(uint)) +
                                        sizeof(uint)];

                // populate the output buffer with the adapter name, packet action,
                // and the packet action end indicating trailer
                BitConverter.GetBytes(adapterName.Length).CopyTo(outputBuffer, 0);
                adapterName.CopyTo(outputBuffer, sizeof(int));
                byte[] httpPacket;
                currentStart = sizeof(int) + adapterName.Length;
                for (int i = 0; i < packetData.Count; i++)
                {
                    httpPacket = (byte[])packetData[i];
                    BitConverter.GetBytes((uint)httpPacket.Length).CopyTo(outputBuffer, currentStart);
                    currentStart += sizeof(uint);
                    httpPacket.CopyTo(outputBuffer, currentStart);
                    currentStart += httpPacket.Length;
                }

                Sift.Resources.Globals.Log.Write("Connecting to Recv Packet", Sift.Resources.Types.LogType.Information, Sift.Resources.Types.LogGroupType.DriverDebug, Sift.Resources.Types.LogDetailType.Verbose);

                unsafe
                {
                    fixed (void* vpOutputBuffer = outputBuffer)
                    {
                        DeviceIoControl(this.driverHandle, IOCTL_FILTERHOOK_RECV_PACKETS,
                                        vpOutputBuffer, (uint)outputBuffer.Length, null, 0,
                                        &kernelLength, 0);
                    }
                }
            }
        }

        /// <summary>
        /// TESTING ONLY Sends all packets regardless of filter status.
        /// </summary>
        /// <param name="adapter">The adapter to send all packets on.</param>
        public void CompletePackets(Adapter adapter)
        {
            if (adapter.PacketCount > 0)
            {
                byte[] packetAction; //byte[] of packet action
                byte[] adapterName;  //byte[] of adapter name
                byte[] trailer;      //byte[] of trailer byte(0)int(0)
                byte[] outputBuffer;

                // length of information returned from kernel mode driver
                uint kernelLength = 0;

                // get the adapter name in byte[]
                adapterName = adapter.UnicodeName;

                // get the packet drop/pass action and the packet id byte[]
                packetAction = GetPacketAction(adapter);

                trailer = new byte[sizeof(byte) + sizeof(int)];
                // fill trailer[] with zeros
                for (int i = 0; i < trailer.Length; i++)
                {
                    trailer[i] = 0;
                }

                // create a buffer to store the adapter name, the packet bytes
                outputBuffer = new byte[sizeof(int) + adapterName.Length + packetAction.Length + trailer.Length];

                // populate the output buffer with the adapter name, packet action,
                // and the packet action end indicating trailer
                BitConverter.GetBytes(adapterName.Length).CopyTo(outputBuffer, 0);
                adapterName.CopyTo(outputBuffer, sizeof(int));
                packetAction.CopyTo(outputBuffer, sizeof(int) + adapterName.Length);
                trailer.CopyTo(outputBuffer, sizeof(int) + adapterName.Length + packetAction.Length);

                unsafe
                {
                    fixed (void* vpOutputBuffer = outputBuffer)
                    {
                        DeviceIoControl(this.driverHandle, IOCTL_FILTERHOOK_SET_PACKETS,
                                        vpOutputBuffer, (uint)outputBuffer.Length, null, 0,
                                        &kernelLength, 0);
                    }
                }

            }
        }

        /// <summary>
        /// Loops until the filter is closed. Waits until a packet
        /// has been received by the filter driver.
        /// </summary>
        private void ReceivedPacketIRPThreadHandler()
        {
            uint kernelLength = 0;

            while (this.driverOpened)
            {
                // DeviceIOControl on IRP
                unsafe
                {
                    DeviceIoControl(this.packetReceivedIRP, IOCTL_FILTERHOOK_PACKET_RECEIVED,
                                    null, 0, null, 0,
                                    &kernelLength, 0);
                    if (this.driverOpened)
                    {
                        ProcessPackets();
                    }
                }
            }
        }

        #endregion

        #region Adapter Handlers

        /// <summary>
        /// Retreives a unicode sting containing a list of all
        /// adapters currently bound to the filter driver.
        /// </summary>
        /// <returns></returns>
        public string RetrieveAdapterList()
        {
            // make sure the driver is opened before
            // attempting to query from it
            if (!this.driverOpened)
            {
                return null;
            }

            // temporary buffer to store adapter information returned
            // from kernel mode driver
            byte[] kernelBuffer = new byte[1024];
            // length of information returned from kernel mode driver
            uint kernelLength = 0;

            unsafe
            {
                fixed (void* vpKernelBuffer = kernelBuffer)
                {
                    DeviceIoControl(this.driverHandle, IOCTL_FILTERHOOK_LIST_ADAPTERS,
                                            null, 0, vpKernelBuffer, (uint)kernelBuffer.Length,
                                            &kernelLength, 0);
                }
            }

            return Encoding.Unicode.GetString(kernelBuffer);
        }

        /// <summary>
        /// Opens an adapter with the filter driver.
        /// </summary>
        /// <param name="adapter">The adapter to open.</param>
        /// <returns>Return true if the adapter was opened.</returns>
        public bool OpenAdapter(Adapter adapter)
        {
            byte[] deviceName;
            uint kernelLength = 0;

            deviceName = adapter.UnicodeName;

            unsafe
            {
                fixed (void* vpDeviceName = deviceName)
                {
                    DeviceIoControl(this.driverHandle, IOCTL_FILTERHOOK_OPEN_ADAPTER,
                                            vpDeviceName, (uint)deviceName.Length, null, 0,
                                            &kernelLength, 0);
                }
            }

            adapter.Open();

            return true;
        }

        /// <summary>
        /// Closes an adapter with the filter driver.
        /// </summary>
        /// <param name="adapter">The adapter to close.</param>
        /// <returns>Returns true if the close operation was successful.</returns>
        public bool CloseAdapter(Adapter adapter)
        {
            byte[] deviceName;
            uint kernelLength = 0;

            deviceName = adapter.UnicodeName;

            unsafe
            {
                fixed (void* vpDeviceName = deviceName)
                {
                    DeviceIoControl(this.driverHandle, IOCTL_FILTERHOOK_CLOSE_ADAPTER,
                                            vpDeviceName, (uint)deviceName.Length, null, 0,
                                            &kernelLength, 0);
                }
            }

            adapter.Close();

            return true;
        }

        /// <summary>
        /// Sets the adapter flags.
        /// </summary>
        /// <param name="adapter">The adapter to set the flags on.</param>
        /// <param name="setting">The flag settings to set on the adapter.</param>
        /// <returns>The results from DeviceIoControl()</returns>
        public uint UpdateAdapterSetting(Adapter adapter)
        {
            byte[] deviceName;
            byte[] outputBuffer;
            uint kernelLength = 0;

            // see if there is a more elegant way to handle conversion to a
            // byte array
            deviceName = adapter.UnicodeName;
            outputBuffer = new byte[sizeof(uint) + deviceName.Length];

            BitConverter.GetBytes(adapter.Settings).CopyTo(outputBuffer, 0);            
            deviceName.CopyTo(outputBuffer, sizeof(uint));
            Sift.Resources.Globals.Log.Write("UpdateAdapterSettings : " + adapter.Settings, Sift.Resources.Types.LogType.Information, Sift.Resources.Types.LogGroupType.DriverDebug, Sift.Resources.Types.LogDetailType.Moderate);

            //for (int i = 0; i < outputBuffer.Length; i++)
            //{
            //    Console.Write(outputBuffer[i] + " ");
            //}

            unsafe
            {
                fixed (void* vpOutputBuffer = outputBuffer)
                {
                    return DeviceIoControl(this.driverHandle, IOCTL_FILTERHOOK_UPDATE_ADAPTER_SETTING,
                                            vpOutputBuffer, (uint)outputBuffer.Length, null, 0,
                                            &kernelLength, 0);
                }
            }
        }

        #endregion


        #endregion

        #region Packet Processing
        /// <summary>
        /// Parses the information returned from GetPackets() and
        /// breaks it into its constituent parts.
        /// </summary>
        /// <param name="input">The byte array of data returned from
        /// the filter driver.</param>
        /// <param name="adapter">The adapter to add the packets to.</param>
        public void ParseGetPackets(byte[] input, Adapter adapter)
        {
            uint packetID = 0;
            int amountRead = 0;
            int packetLength = 0;
            int currentStart = 0;

            packetID = BitConverter.ToUInt32(input, currentStart);
            amountRead += sizeof(uint);
            currentStart += sizeof(uint);

            // loop for all valid packets, or until we reach the end
            // of the byte array
            while (packetID != 0 && currentStart < input.Length)
            {
                Packet currentPacket = new Packet();
                currentPacket.ID = packetID;

                // copy the packet length
                packetLength = BitConverter.ToInt32(input, currentStart);

                amountRead += sizeof(uint);
                currentStart += sizeof(uint);

                // store the raw packet data
                int rawDataLength = 0;

                if (packetLength > 54)
                {
                    rawDataLength = 54;
                }
                else
                {
                    rawDataLength = packetLength;
                }

                byte[] rawData = new byte[rawDataLength];
                Array.Copy(input, currentStart, rawData, 0, rawDataLength);
                currentPacket.RawData = rawData;

                // copy the IP Protocol               
                currentPacket.IPProtocol = input[currentStart + 23];

                // copy the source IP                
                currentPacket.SourceIP = (uint)System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input, currentStart + 26));
                // copy the destination IP
                currentPacket.DestinationIP = (uint)System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input, currentStart + 30));

                // copy the source Port
                currentPacket.SourcePort = (ushort)System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt16(input, currentStart + 34));

                // copy the destination Port
                currentPacket.DestinationPort = (ushort)System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt16(input, currentStart + 36));

                // copy any available content
                if ((packetLength > 54) && (currentStart + packetLength < input.Length))
                {
                    currentPacket.Content = ASCIIEncoding.ASCII.GetString(input, (int)currentStart + 54, (int)(packetLength - 54));
                }

                // add the packet to the adapter packet list
                adapter.AddPacket(currentPacket);

                currentStart += packetLength;
                amountRead += packetLength;

                // check out the next packet
                packetID = BitConverter.ToUInt32(input, currentStart);
                amountRead += sizeof(uint);
                currentStart += sizeof(uint);
            }
        }

        /// <summary>
        /// Sets the filter flag for all packets in the adapter.
        /// </summary>
        /// <param name="adapter">The adapter to process.</param>
        public void FilterPackets(Adapter adapter)
        {
            Packet packet;

            // filter all packets in the adapter queue
            for (int i = 0; i < adapter.PacketCount; i++)
            {
                packet = adapter.GetPacket(i);

                // drop packets if the configuration information, or lists are not loaded
                // skip all other processing
                if (!this.IsConfigurationLoaded || !this.IsListLoadComplete)
                {
                    Resources.Globals.Log.Write("Configuration not loaded, dropping packet", Sift.Resources.Types.LogType.Information, Sift.Resources.Types.LogGroupType.FilterDecision, Sift.Resources.Types.LogDetailType.Verbose);

                    // if the user has requested we pass packets by default, then override our default, however, drop the packet if the lists are not loaded
                    if (Resources.Settings.AdapterSettings.Settings.Adapters.DefaultSettings.DefaultAction == Sift.Resources.Types.DefaultActionType.Allow && this.IsListLoadComplete)
                        packet.DropStatus = PACKET_ACTION_ALLOW;
                    else
                        packet.DropStatus = PACKET_ACTION_DROP;
                }
                else
                {
                    // filter blacklists
                    if (filterBlackList.Search(packet) != null)
                    {
                        packet.DropStatus = PACKET_ACTION_DROP;
                    }

                    // filter against any predictive packets to drop
                    if (predictivePacketBlackList.Search(packet.RawData))
                    {
                        packet.DropStatus = PACKET_ACTION_DROP;
                    }

                    // the packet is not on any blacklist, allow the packet
                    if (packet.DropStatus == PACKET_ACTION_UNSET)
                    {
                        packet.DropStatus = PACKET_ACTION_ALLOW;
                    }

                    // filter against any predictive packets to allow
                    if (predictivePacketWhiteList.Search(packet.RawData))
                    {
                        packet.DropStatus = PACKET_ACTION_ALLOW;
                    }

                    // filter whitelists
                    if (filterWhiteList.Search(packet) != null)
                    {
                        packet.DropStatus = PACKET_ACTION_ALLOW;
                    }
                }
            }
        }

        /// <summary>
        /// Sends the packet action for all packets that have been
        /// processed by FilterPackets()
        /// </summary>
        /// <param name="adapter">The adapter to set packets for.</param>
        public void SetPackets(Adapter adapter)
        {

            // process any packets in that have been
            // added to the adapter
            if (adapter.PacketCount > 0)
            {
                // make sure the driver is opened before
                // attempting process the packets
                if (!this.driverOpened)
                {
                    return;
                }

                // if the packet is TCP/IP, send a ACK,RST
                // to stop the TCP request. if the packet is
                // not TCP/IP, then an ICMP Destionation Host
                // Unreachable is set
                SetRecvDrop(adapter);

                // complete the packets based on the drop/pass
                // status. if the status = pass, the packet is
                // allowed to process through the passthru driver,
                // otherwise the packet is ndis completed to allow
                // for cleanup, but is not sent through to the NIC
                CompletePackets(adapter);

                // remove any processed packets
                RemoveProcessedPackets(adapter);
            }
        }

        /// <summary>
        /// Gets the packet action
        /// </summary>
        /// <param name="adapter">The adapter to get the
        /// packet action.</param>
        /// <returns>Returns a byte array of all PacketID/PacetAction
        /// combinations to sent to the filter driver.</returns>
        public byte[] GetPacketAction(Adapter adapter)
        {
            ArrayList actionList = new ArrayList();
            Packet packet;
            byte action;
            byte[] packetID = new byte[sizeof(int)];

            for (int i = 0; i < adapter.PacketCount; i++)
            {
                packet = adapter.GetPacket(i);
                action = packet.DropStatus;
                // do not process packets that have not been
                // filtered yet
                if (action != PACKET_ACTION_UNSET)
                {

                    // set the direction flag to the action byte
                    if (packet.IsOutbound)
                    {
                        action |= PACKET_ACTION_OUTBOUND;
                        packet.Processed = true;
                    }
                    else
                    {
                        action |= PACKET_ACTION_INBOUND;
                        packet.Processed = true;
                    }

                    actionList.Add(action);
                    actionList.AddRange(BitConverter.GetBytes(packet.ID));
                }
            }

            return (byte[])actionList.ToArray(typeof(byte));
        }

        /// <summary>
        /// Removes all processed packets from the adapter.
        /// </summary>
        /// <param name="adapter">The adapter to remove all processed
        /// packets from.</param>
        public void RemoveProcessedPackets(Adapter adapter)
        {
            Packet packet;
            for (int i = 0; i < adapter.PacketCount; i++)
            {
                packet = adapter.GetPacket(i);
                if (packet.Processed)
                {
                    adapter.RemovePacket(packet);
                    // keep i from incrementing since we have
                    // removed the element from index i
                    i--;
                }
            }
        }

        /// <summary>
        /// Completes a single filter cycle.
        /// </summary>
        public void ProcessPackets()
        {
            Resources.Globals.Log.Write("ProcessPackets()", Resources.Types.LogType.Information, Resources.Types.LogGroupType.ServiceDebug, Resources.Types.LogDetailType.Verbose);
            foreach (Adapter adapter in adapters)
            {
                // process all enabled adapters
                if (adapter.Enabled && adapter.IsOpen)
                {
                    GetPackets(adapter);
                    FilterPackets(adapter);
                    SetPackets(adapter);
                }
            }
        }
        #endregion

        #region Adapter Processing

        /// <summary>
        /// Loads all adapter settings from the adapter XML file.
        /// </summary>
        public void LoadAdapters()
        {
            Resources.Globals.Log.Write("LoadAdapters()", Resources.Types.LogType.Information, Resources.Types.LogGroupType.ServiceDebug, Resources.Types.LogDetailType.Verbose);

            if (adapters == null) // error communicating adapter list from driver
            {
                Resources.Globals.Log.Write("Error communicating adapter list from filter driver.", Resources.Types.LogType.Error, Sift.Resources.Types.LogGroupType.DriverDebug, Sift.Resources.Types.LogDetailType.Minimal);
                return;
            }

            #region Get the adpater default settings
            Resources.Settings.AdapterSetting defaults = Resources.Settings.AdapterSettings.Settings.Adapters.DefaultSettings;

            if (defaults == null)
            {
                Resources.Globals.Log.Write("Default settings not found, adding them back in.", Resources.Types.LogType.Warning, Resources.Types.LogGroupType.ServiceDebug, Resources.Types.LogDetailType.Moderate);

                // recover from the missing defaults by creating a new default entry
                defaults = new Resources.Settings.AdapterSetting();
                defaults.Id = Resources.Constants.AdapterDefaultSettingId;
                defaults.Enabled = true;
                defaults.FilterInbound = false;
                defaults.FilterOutbound = true;
                defaults.FilterHTTP = true;
                defaults.FilterTCP = false;
                defaults.FilterUDP = false;
                defaults.FilterAll = false;
                defaults.DefaultAction = Resources.Types.DefaultActionType.Drop;                

                Resources.Settings.AdapterSettings.Settings.Adapters.Add(defaults);
            }
            #endregion

            foreach (Adapter adapter in adapters)
            {
                #region Get the adapter settings if they exist, otherwise create them
                Resources.Settings.AdapterSetting adapterSetting = Resources.Settings.AdapterSettings.Settings.Adapters.GetByAdapterId(adapter.Id);
               
                if (adapterSetting == null && adapter.IsValid)
                {
                    // adapter not found, add it using the default settings
                    Resources.Globals.Log.Write("New adapter " + adapter.Description + " found, adding default configuration.", Resources.Types.LogType.Information, Resources.Types.LogGroupType.ServiceDebug, Resources.Types.LogDetailType.Moderate);

                    adapterSetting = new Sift.Resources.Settings.AdapterSetting();

                    adapterSetting.Id = adapter.Id;
                    adapterSetting.Description = adapter.Description;
                    adapterSetting.Enabled = true;
                    adapterSetting.FilterInbound = defaults.FilterInbound;
                    adapterSetting.FilterOutbound = defaults.FilterOutbound;
                    adapterSetting.FilterHTTP = defaults.FilterHTTP;
                    adapterSetting.FilterTCP = defaults.FilterTCP;
                    adapterSetting.FilterUDP = defaults.FilterUDP;
                    adapterSetting.FilterAll = defaults.FilterAll;
                    adapterSetting.DefaultAction = defaults.DefaultAction;
                    adapterSetting.UseDefaults = true;

                    Resources.Settings.AdapterSettings.Settings.Adapters.Add(adapterSetting);
                }
                else if (!adapter.IsValid)
                {
                    // the adapter is not a valid MAC assigned network adapter, we don't care about these and should allow all packets through
                    Resources.Globals.Log.Write("Non-network adapter " + adapter.Description + " found, applying passthrough rules.", Resources.Types.LogType.Information, Resources.Types.LogGroupType.ServiceDebug, Resources.Types.LogDetailType.Verbose);
                    adapterSetting = new Sift.Resources.Settings.AdapterSetting();

                    adapterSetting.FilterInbound = false;
                    adapterSetting.FilterOutbound = false;
                    adapterSetting.FilterHTTP = false;
                    adapterSetting.FilterTCP = false;
                    adapterSetting.FilterUDP = false;
                    adapterSetting.FilterAll = false;
                    adapterSetting.DefaultAction = Resources.Types.DefaultActionType.Allow;                    
                }
                #endregion

                #region Open and close the adapter based on its settings
                if (adapterSetting.Enabled)
                {                    
                    Resources.Globals.Log.Write("Adapter " + adapter.Name + " is enabled", Resources.Types.LogType.Information, Resources.Types.LogGroupType.ServiceDebug, Resources.Types.LogDetailType.Verbose);

                    if (!adapter.IsOpen)
                        this.OpenAdapter(adapter);
                }
                else
                {
                    if (adapter.IsOpen)
                        this.CloseAdapter(adapter);
                }
                #endregion

                #region Apply the driver settings to the adapter
                uint userModeSettings = 0;

                if (adapterSetting.UseDefaults)
                {
                    if (defaults.FilterInbound)
                        userModeSettings |= USER_MODE_SETTINGS_CAPTURE_RECV;

                    if (defaults.FilterOutbound)
                        userModeSettings |= USER_MODE_SETTINGS_CAPTURE_SEND;

                    if (defaults.FilterHTTP)
                        userModeSettings |= USER_MODE_SETTINGS_CAPTURE_HTTP;

                    if (defaults.FilterTCP)
                        userModeSettings |= USER_MODE_SETTINGS_CAPTURE_TCPIP;

                    if (defaults.FilterUDP)
                        userModeSettings |= USER_MODE_SETTINGS_CAPTURE_UDPIP;

                    if (defaults.FilterAll)
                        userModeSettings |= USER_MODE_SETTINGS_CAPTURE_ALL;

                    if (defaults.DefaultAction == Sift.Resources.Types.DefaultActionType.Drop)
                        userModeSettings |= USER_MODE_SETTINGS_DROP_DEFAULT;                    
                }
                else
                {
                    if (adapterSetting.FilterInbound)
                        userModeSettings |= USER_MODE_SETTINGS_CAPTURE_RECV;

                    if (adapterSetting.FilterOutbound)
                        userModeSettings |= USER_MODE_SETTINGS_CAPTURE_SEND;

                    if (adapterSetting.FilterHTTP)
                        userModeSettings |= USER_MODE_SETTINGS_CAPTURE_HTTP;

                    if (adapterSetting.FilterTCP)
                        userModeSettings |= USER_MODE_SETTINGS_CAPTURE_TCPIP;

                    if (adapterSetting.FilterUDP)
                        userModeSettings |= USER_MODE_SETTINGS_CAPTURE_UDPIP;

                    if (adapterSetting.FilterAll)
                        userModeSettings |= USER_MODE_SETTINGS_CAPTURE_ALL;

                    if (adapterSetting.DefaultAction == Sift.Resources.Types.DefaultActionType.Drop)
                        userModeSettings |= USER_MODE_SETTINGS_DROP_DEFAULT;
                    
                }
                adapter.Settings = userModeSettings;

                this.UpdateAdapterSetting(adapter);
                #endregion
            }

            // write any changes back to the config file
            Resources.Settings.AdapterSettings.Save();
        }

        #endregion

        #region Configuration / List Processing

        /// <summary>
        /// Downloads any available list updates on a determined schedule.
        /// </summary>
        private void ListUpdatesAutomatedDownload()
        {
            lock (Resources.Globals.Statistics)
            {
                Resources.Globals.Statistics.LastListUpdate = Resources.Settings.ServiceSettings.Settings.LastAutomatedListUpdate;
            }

            TimeSpan waitLength = new TimeSpan(0, 10, 0); // wait 10 minutes between each loop
            while (true)
            {
                if (Sift.Resources.Settings.ServiceSettings.Settings.LastAutomatedListUpdate < DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0)))
                {                    
                    // the last update was over a day ago, check for any updates
                    bool updateSuccessful = true;
                    
                    try
                    {                       
                        #region Download the lists to update
                        XmlDocument document = Sift.Resources.WebServiceClient.GetListUpdatesXMLByLists(Sift.Resources.Settings.ListSettings.Settings.FlattenLists(Sift.Resources.Settings.ListSettings.Settings.ListGroups));

                        document.Save(Sift.Resources.Settings.ListUpdateSettings.ConfigurationFilename);

                        Sift.Resources.Settings.ListUpdateSettings.Close();
                        LoadListUpdates();
                        #endregion
                    }
                    catch
                    {
                        updateSuccessful = false;
                    }

                    if (updateSuccessful)
                    {
                        // the update was successful
                        Sift.Resources.Settings.ServiceSettings.Settings.LastAutomatedListUpdate = DateTime.Now;
                        Sift.Resources.Settings.ServiceSettings.Save();

                        // update the remoting statistic so the gui can notify the user when the last update was
                        lock (Resources.Globals.Statistics)
                        {
                            Resources.Globals.Statistics.LastListUpdate = Resources.Settings.ServiceSettings.Settings.LastAutomatedListUpdate;
                        }
                    }
                }

                // wait before next loop
                System.Threading.Thread.Sleep(waitLength);
            }
        }

        /// <summary>
        /// Loads all list information from a list group and places it in the correctsponding black/white list.
        /// </summary>
        /// <param name="listGroup">A list group object to load.</param>
        public void LoadListGroup(Resources.Settings.ListGroup listGroup)
        {            
            if (listGroup.Enabled)
            {
                #region Process Lists
                foreach (Resources.Settings.List list in listGroup.Lists)
                {
                    // only process enabled lists
                    if (list.Enabled && System.IO.File.Exists(list.Path))
                    {
                        switch (list.Content)
                        {
                            case Resources.Types.ContentType.Domain:
                                if (list.MatchAction == Sift.Resources.Types.MatchActionType.Block)
                                    Resources.Globals.Statistics.DomainBlockCount += filterBlackList.LoadDomainList(list.Path, listGroup.Description, listGroup.Log);
                                else if (list.MatchAction == Sift.Resources.Types.MatchActionType.Allow)
                                    Resources.Globals.Statistics.DomainAllowCount += filterWhiteList.LoadDomainList(list.Path, listGroup.Description, listGroup.Log);
                                break;
                            case Resources.Types.ContentType.IP:
                                if (list.MatchAction == Sift.Resources.Types.MatchActionType.Block)
                                    Resources.Globals.Statistics.IpBlockCount += filterBlackList.LoadIPList(list.Path, listGroup.Description, listGroup.Log);
                                else if (list.MatchAction == Sift.Resources.Types.MatchActionType.Allow)
                                    Resources.Globals.Statistics.IpAllowCount += filterWhiteList.LoadIPList(list.Path, listGroup.Description, listGroup.Log);
                                break;
                            case Resources.Types.ContentType.URL:
                                if (list.MatchAction == Sift.Resources.Types.MatchActionType.Block)
                                    Resources.Globals.Statistics.UrlBlockCount += filterBlackList.LoadURLList(list.Path, listGroup.Description, listGroup.Log);
                                else if (list.MatchAction == Sift.Resources.Types.MatchActionType.Allow)
                                    Resources.Globals.Statistics.UrlAllowCount += filterWhiteList.LoadURLList(list.Path, listGroup.Description, listGroup.Log);
                                break;
                        }
                    }
                }
                #endregion

                #region Process Subgroups
                foreach (Resources.Settings.ListGroup subListGroup in listGroup.ListGroups)                
                    LoadListGroup(subListGroup);
                #endregion
            }
        }

        /// <summary>
        /// Loads all blacklists and whitelists specified as enabled in the
        /// configuration file
        /// </summary>
        public void LoadLists()
        {            
            Resources.Globals.Log.Write("LoadLists()", Resources.Types.LogType.Information, Resources.Types.LogGroupType.ServiceDebug, Resources.Types.LogDetailType.Verbose);

            Resources.Globals.Log.Disable();
            IsListLoadComplete = false;

            // close the settings file if it has been previously loaded, this forces a reload of all configuraiton data
            Resources.Settings.ListSettings.Close();

            // flush any data already in the blacklist
            filterBlackList.Clear();
            filterWhiteList.Clear();

            lock (Resources.Globals.Statistics)
            {
                Resources.Globals.Statistics.IpAllowCount = 0;
                Resources.Globals.Statistics.UrlAllowCount = 0;
                Resources.Globals.Statistics.DomainAllowCount = 0;
                Resources.Globals.Statistics.IpBlockCount = 0;
                Resources.Globals.Statistics.UrlBlockCount = 0;
                Resources.Globals.Statistics.DomainBlockCount = 0;
            }

            foreach (Resources.Settings.ListGroup group in Resources.Settings.ListSettings.Settings.ListGroups)
            {
                Resources.Globals.Log.Write("Major list group Enabled " + group.Enabled.ToString(), Resources.Types.LogType.Information, Resources.Types.LogGroupType.ServiceDebug, Resources.Types.LogDetailType.Verbose);
                LoadListGroup(group);
            }

            // we have loaded all our list elements, sort them
            filterBlackList.Sort();
            filterWhiteList.Sort();

            Resources.Settings.ListSettings.Close();

            IsListLoadComplete = true;
            Resources.Globals.Log.Enable();
        }

        /// <summary>
        /// Loads a list update file and modifies the list entries accordingly. If the blacklist or whitelist has been
        /// loaded into memory, this is also modified.
        /// </summary>
        public void LoadListUpdates()
        {
            // process list updates, for now, ignore entries where the listId does not exist in the config file
            // later, when the list update service is developed, these entries would possibly be added and files generated            
            lock (lockListUpdating)
            {
                Resources.Globals.Log.Write("LoadListUpdates - Enter", Sift.Resources.Types.LogType.Information, Sift.Resources.Types.LogGroupType.ServiceDebug, Sift.Resources.Types.LogDetailType.Verbose);

                foreach (Resources.Settings.ListUpdate listUpdate in Resources.Settings.ListUpdateSettings.Settings.ListsUpdates)
                {
                    Resources.Globals.Log.Write("ListUpdate " + listUpdate.ListId + " Action " + listUpdate.Action.ToString(), Sift.Resources.Types.LogType.Information, Sift.Resources.Types.LogGroupType.ServiceDebug, Sift.Resources.Types.LogDetailType.Verbose);

                    if (listUpdate.Action == Resources.Types.ListUpdateType.Remove)
                    {
                        Resources.Globals.Log.Write("Remove " + listUpdate.ListId, Sift.Resources.Types.LogType.Information, Sift.Resources.Types.LogGroupType.ServiceDebug, Sift.Resources.Types.LogDetailType.Verbose);
                        // remove the list file if it exists
                        if (System.IO.File.Exists(Resources.Settings.List.GetPathByListId(listUpdate.ListId)))
                            System.IO.File.Delete(Resources.Settings.List.GetPathByListId(listUpdate.ListId));
                    }
                    else
                    {
                        Resources.Settings.List list = Resources.Settings.ListSettings.Settings.GetByListId(listUpdate.ListId);

                        if (list != null)
                        {

                            Resources.Globals.Log.Write("Add or modify " + list.Id, Sift.Resources.Types.LogType.Information, Sift.Resources.Types.LogGroupType.ServiceDebug, Sift.Resources.Types.LogDetailType.Verbose);
                            // the file is either being added or modified
                            System.Collections.Generic.List<string> listLines = new System.Collections.Generic.List<string>();

                            #region Load the list file into memory, create it if it doesn't exist

                            Resources.Globals.Log.Write("List file " + list.Path + " exists? " + System.IO.File.Exists(list.Path), Sift.Resources.Types.LogType.Information, Sift.Resources.Types.LogGroupType.ServiceDebug, Sift.Resources.Types.LogDetailType.Verbose);
                            if (!System.IO.File.Exists(list.Path))
                            {
                                // Create the list file if it doesn't exist, this is the case if a file has been added
                                System.IO.File.CreateText(list.Path).Close();
                                Resources.Globals.Log.Write("Created list file " + list.Path, Sift.Resources.Types.LogType.Information, Sift.Resources.Types.LogGroupType.ServiceDebug, Sift.Resources.Types.LogDetailType.Moderate);
                            }

                            /// TODO: find a faster and/or memory friendly alternative                                                            
                            System.IO.TextReader fileReader = new System.IO.StreamReader(list.Path);
                            string line = fileReader.ReadLine();

                            while (line != null)
                            {
                                if (!listLines.Contains(line))
                                {
                                    Resources.Globals.Log.Write("Adding list update line " + line + " to " + list.Id.ToString(), Sift.Resources.Types.LogType.Information, Sift.Resources.Types.LogGroupType.ServiceDebug, Sift.Resources.Types.LogDetailType.Verbose);
                                    listLines.Add(line);
                                }

                                line = fileReader.ReadLine();
                            }

                            fileReader.Close();
                            #endregion

                            #region Process the list updates to file and in memory
                            foreach (string value in listUpdate.Updates.Values)
                            {
                                Resources.Globals.Log.Write("Processing list value " + value, Sift.Resources.Types.LogType.Information, Sift.Resources.Types.LogGroupType.ServiceDebug, Sift.Resources.Types.LogDetailType.Verbose);
                                // only process the most recent entry, which takes precendence
                                Resources.Settings.ListEntryUpdate update = listUpdate.Updates.GetMostRecentByValue(value);

                                switch (update.Action)
                                {
                                    case Sift.Resources.Types.ListUpdateType.Add:
                                        // add the entry to the list file if it doesn't exist
                                        if (!listLines.Contains(value))
                                            listLines.Add(value);

                                        // add the entry to the appropriate in memory list
                                        switch (list.MatchAction)
                                        {
                                            case Sift.Resources.Types.MatchActionType.Allow:
                                                #region Add new entry to the whitelist
                                                switch (list.Content)
                                                {
                                                    case Sift.Resources.Types.ContentType.Domain:
                                                        Resources.Globals.Statistics.DomainAllowCount++;
                                                        filterWhiteList.AddDomain(update.Value);
                                                        break;
                                                    case Sift.Resources.Types.ContentType.IP:
                                                        Resources.Globals.Statistics.IpAllowCount++;
                                                        filterWhiteList.AddIP(update.Value);
                                                        break;
                                                    case Sift.Resources.Types.ContentType.URL:
                                                        Resources.Globals.Statistics.UrlAllowCount++;
                                                        filterWhiteList.AddURL(update.Value);
                                                        break;
                                                }
                                                #endregion
                                                break;

                                            case Sift.Resources.Types.MatchActionType.Block:
                                                #region Add new entry to the blacklist
                                                switch (list.Content)
                                                {
                                                    case Sift.Resources.Types.ContentType.Domain:
                                                        Resources.Globals.Statistics.DomainBlockCount++;
                                                        filterBlackList.AddDomain(update.Value);
                                                        break;
                                                    case Sift.Resources.Types.ContentType.IP:
                                                        Resources.Globals.Statistics.IpBlockCount++;
                                                        filterBlackList.AddIP(update.Value);
                                                        break;
                                                    case Sift.Resources.Types.ContentType.URL:
                                                        Resources.Globals.Statistics.UrlBlockCount++;
                                                        filterBlackList.AddURL(update.Value);
                                                        break;
                                                }
                                                #endregion
                                                break;

                                        }
                                        break;
                                    case Sift.Resources.Types.ListUpdateType.Remove:
                                        // remove the entry from the list file if it doesn't exist
                                        if (listLines.Contains(value))
                                            listLines.Remove(value);

                                        // remove the entry from the appropriate in memory list
                                        switch (list.MatchAction)
                                        {
                                            case Sift.Resources.Types.MatchActionType.Allow:
                                                #region Remove the entry from the whitelist
                                                switch (list.Content)
                                                {
                                                    case Sift.Resources.Types.ContentType.Domain:
                                                        Resources.Globals.Statistics.DomainAllowCount--;
                                                        filterWhiteList.RemoveDomain(update.Value);
                                                        break;
                                                    case Sift.Resources.Types.ContentType.IP:
                                                        Resources.Globals.Statistics.IpAllowCount--;
                                                        filterWhiteList.RemoveIP(update.Value);
                                                        break;
                                                    case Sift.Resources.Types.ContentType.URL:
                                                        Resources.Globals.Statistics.UrlAllowCount--;
                                                        filterWhiteList.RemoveURL(update.Value);
                                                        break;
                                                }
                                                #endregion
                                                break;
                                            case Sift.Resources.Types.MatchActionType.Block:
                                                #region Remove the entry from the whitelist
                                                switch (list.Content)
                                                {
                                                    case Sift.Resources.Types.ContentType.Domain:
                                                        Resources.Globals.Statistics.DomainBlockCount--;
                                                        filterBlackList.RemoveDomain(update.Value);
                                                        break;
                                                    case Sift.Resources.Types.ContentType.IP:
                                                        Resources.Globals.Statistics.IpBlockCount--;
                                                        filterBlackList.RemoveIP(update.Value);
                                                        break;
                                                    case Sift.Resources.Types.ContentType.URL:
                                                        Resources.Globals.Statistics.UrlBlockCount--;
                                                        filterBlackList.RemoveURL(update.Value);
                                                        break;
                                                }
                                                #endregion
                                                break;
                                        }
                                        break;
                                }
                            }
                            #endregion

                            #region Commit the file changes
                            if (System.IO.File.Exists(list.Path))
                            {
                                /// save the changes back to the list file                                                  
                                System.IO.TextWriter fileWriter = new System.IO.StreamWriter(list.Path, false);

                                foreach (string listLine in listLines)
                                    fileWriter.WriteLine(listLine);

                                fileWriter.Close();
                            }
                            #endregion
                        }
                    }
                }

                /// TODO: Pull the date of the last update and store it in the sift configuration file
                /// to be used by future incremental update requests

                // finally, delete the configuration file, it is no longer needed
                if (System.IO.File.Exists(Resources.Settings.ListUpdateSettings.ConfigurationFilename))
                    System.IO.File.Delete(Resources.Settings.ListUpdateSettings.ConfigurationFilename);

                Resources.Settings.ListUpdateSettings.Close();
            }
        }

        /// <summary>
        /// Loads the log settings from the configuration file.
        /// </summary>
        public void LoadLogSettings()
        {
            Resources.Globals.Log.LogGroups.Clear();

            if (Resources.Settings.LogSettings.Settings.LogDriver)
                Resources.Globals.Log.LogGroups.Add(Resources.Types.LogGroupType.DriverDebug);
            if (Resources.Settings.LogSettings.Settings.LogListMatch)
                Resources.Globals.Log.LogGroups.Add(Resources.Types.LogGroupType.FilterMatch);
            if (Resources.Settings.LogSettings.Settings.LogListBlock)
                Resources.Globals.Log.LogGroups.Add(Resources.Types.LogGroupType.FilterBlock);
            if (Resources.Settings.LogSettings.Settings.LogListAllow)
                Resources.Globals.Log.LogGroups.Add(Resources.Types.LogGroupType.FilterAllow);
            if (Resources.Settings.LogSettings.Settings.LogDescisionBranch)
                Resources.Globals.Log.LogGroups.Add(Resources.Types.LogGroupType.FilterDecision);
            if (Resources.Settings.LogSettings.Settings.LogService)
                Resources.Globals.Log.LogGroups.Add(Resources.Types.LogGroupType.ServiceDebug);

            Resources.Globals.Log.LogResource = Resources.Types.LogResourceType.File;
            Resources.Globals.Log.LogDetailLevel = Resources.Settings.LogSettings.Settings.LogLevel;
        }

        /// <summary>
        /// Forks off a thread to load the configuration information. This is needed to allow the sift service
        /// to start quickly, because loading lists can take a very long time.
        /// </summary>
        private void LoadConfigurationInBackground()
        {
            IsConfigurationLoaded = false; // disable packet handling until all configuration data is loaded
            Resources.Globals.Statistics.IsEnabled = false;

            LoadLogSettings();

            LoadAdapters();

            // don't log the list entries to file
            //Resources.Globals.Log.Disable();
            LoadLists();
            //Resources.Globals.Log.Enable();

            IsConfigurationLoaded = true; // allow packet handling to resume

            if (this.driverOpened)
                Resources.Globals.Statistics.IsEnabled = true;
        }

        /// <summary>
        /// Loads all filter configuration information
        /// from the XML configuration files.
        /// </summary>
        public void LoadConfig()
        {
            // close out any previous configuration loading
            if (threadConfigurationLoader != null && threadConfigurationLoader.ThreadState == ThreadState.Running)
                threadConfigurationLoader.Abort();

            // load up the configuration in the background
            threadConfigurationLoader = new Thread(new ThreadStart(this.LoadConfigurationInBackground));
            threadConfigurationLoader.Start();
        }

        /// <summary>
        /// Disables the filter driver by unsetting all filter flags.
        /// </summary>
        public void Disable()
        {
            // disables the filter functionality of the service
            // to restore/enable the filter, a call to LoadAdapters()
            // is made
            foreach (Adapter adapter in adapters)
            {
                // unset any settings, this will cause all packets to pass
                // through the filter        
                adapter.Settings = 0;
                UpdateAdapterSetting(adapter);
                // close the adapter
                CloseAdapter(adapter);
            }
            Resources.Globals.Statistics.IsEnabled = false;
        }

        /// <summary>
        /// Enabled the filter process and reloads any modified adapter configuration.
        /// </summary>
        public void Enable()
        {            
            LoadAdapters();
            
            if (this.driverOpened)
                Resources.Globals.Statistics.IsEnabled = true;
        }

        #endregion
       
        #region Debugging Methods

        public bool     SearchIP(string ip)
        {
            return (filterBlackList.SearchIP(ip) != null);
        }

        public bool     SearchURL(string url)
        {
            return (filterBlackList.SearchURL(url) != null);
        }

        public bool     SearchDomain(string domain)
        {
            return (filterBlackList.SearchDomain(domain) != null);
        }

        /// <summary>
        /// Prints all adapters.
        /// </summary>
        public void PrintAdapters()
        {
            for (int i = 0; i < adapters.Length; i++)
            {
                adapters[i].PrintAdapter();
            }
        }

        #endregion DEBUGGING_METHODS

        #endregion METHODS
    }
}
