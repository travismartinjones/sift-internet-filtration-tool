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

#ifndef __HOOK__H
#define __HOOK__H

// Consider changing method to METHOD_IN_DIRECT or METHOD_OUT_DIRECT to handle 
// large amounts of data. See Windows DDK documentation for CTL_CODE

#define IOCTL_FILTERHOOK_LIST_ADAPTERS          CTL_CODE(FILE_DEVICE_NETWORK, 0x801, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS)
#define IOCTL_FILTERHOOK_OPEN_ADAPTER           CTL_CODE(FILE_DEVICE_NETWORK, 0x802, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS)
#define IOCTL_FILTERHOOK_CLOSE_ADAPTER          CTL_CODE(FILE_DEVICE_NETWORK, 0x803, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS)
#define IOCTL_FILTERHOOK_UPDATE_ADAPTER_SETTING    CTL_CODE(FILE_DEVICE_NETWORK, 0x804, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS)
#define IOCTL_FILTERHOOK_GET_PACKETS            CTL_CODE(FILE_DEVICE_NETWORK, 0x806, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS)
#define IOCTL_FILTERHOOK_SET_PACKETS            CTL_CODE(FILE_DEVICE_NETWORK, 0x807, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS)
#define IOCTL_FILTERHOOK_PACKET_RECEIVED		CTL_CODE(FILE_DEVICE_NETWORK, 0x808, METHOD_NEITHER, FILE_READ_ACCESS | FILE_WRITE_ACCESS)
#define IOCTL_FILTERHOOK_USERMODE_SHUTDOWN      CTL_CODE(FILE_DEVICE_NETWORK, 0x810, METHOD_NEITHER, FILE_READ_ACCESS | FILE_WRITE_ACCESS)
#define IOCTL_FILTERHOOK_RECV_PACKETS			CTL_CODE(FILE_DEVICE_NETWORK, 0x811, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS)
#define IOCTL_FILTERHOOK_SEND_PACKET			CTL_CODE(FILE_DEVICE_NETWORK, 0x812, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS)

// UserModeSettings Constant Flags
#define USER_MODE_SETTINGS_OPEN_ADAPTER       1  // 0000 0001 - indicates if an adapter is open
#define USER_MODE_SETTINGS_DROP_DEFAULT       2  // 0000 0010 - if set, all packets are dropped
#define USER_MODE_SETTINGS_CAPTURE_RECV       4  // 0000 0100 - capture receives
#define USER_MODE_SETTINGS_CAPTURE_SEND       8  // 0000 1000 - capture sends
#define USER_MODE_SETTINGS_CAPTURE_HTTP		 16  // 0001 0000 - capture HTTP
#define USER_MODE_SETTINGS_CAPTURE_TCPIP     32  // 0010 0000 - capture all TCP/IP not just HTTP
#define USER_MODE_SETTINGS_CAPTURE_UDPIP	 64  // 0100 0000 - capture all UDP/IP packets
#define USER_MODE_SETTINGS_CAPTURE_ALL		128  // 1000 0000 - capture all IP traffic
												 //             if IP is not TCP or UDP, no socket
												 //             information is captured

#define PACKET_ACTION_DROP					  1  // 0000 0001 - indicates to drop the packet
#define PACKET_ACTION_ALLOW					  2  // 0000 0010 - indicates to allow the packet
#define PACKET_ACTION_INBOUND				  4  // 0000 0100 - indicates to recv the packet
#define PACKET_ACTION_OUTBOUND				  8  // 0000 1000 - indicates to send the packet

#define FILTER_HTTP_PORT					 80  // default HTTP port to filter
#define FILTER_HTTPS_PORT					443  // default HTTPS port to filter

// Function header definitons

NTSTATUS ListAdapters(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp);
NTSTATUS OpenAdapter(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp);
NTSTATUS CloseAdapter(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp);
NTSTATUS UpdateAdapterSetting(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp);
NTSTATUS RecvPackets(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp);
NTSTATUS GetPackets(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp);
NTSTATUS SetPackets(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp);
NTSTATUS HookCreate(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp);
NTSTATUS HookCleanup(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp);
NTSTATUS HookClose(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp);
NTSTATUS HookDeviceControl(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp);

#endif __HOOK__H