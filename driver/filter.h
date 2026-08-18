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

/*
/	Author : 
/		Travis Jones
/
/	Description : 
/		Filter function definitions. 
/
/
/	Change log :
/		07.28.05	Original file creation
/
*/

#ifndef __FILTER__H
#define __FILTER__H

#define MAX_PACKET_POOL_SIZE 0x0000FFFF
#define MIN_PACKET_POOL_SIZE 0x000000FF

BOOLEAN
FilterGetPacketValue(
	IN PNDIS_PACKET packet,
	IN PVOID value,
	IN UINT valueBegin,
	IN UINT valueLength
);

USHORT FilterGetEthernetType(
	IN PNDIS_PACKET packet
);

UCHAR
FilterGetIPVersion(
	IN PNDIS_PACKET packet
);

UCHAR
FilterGetIPProtocol(
	IN PNDIS_PACKET packet
);

ULONG
FilterGetSourceIP(
	IN PNDIS_PACKET packet
);

ULONG
FilterGetDestinationIP(
	IN PNDIS_PACKET packet
);

USHORT
FilterGetSourcePort(
	IN PNDIS_PACKET packet
);

USHORT
FilterGetDestinationPort(
	IN PNDIS_PACKET packet
);

UINT
FilterGetPacketLength(
	IN PNDIS_PACKET packet
);

UINT
FilterGetContentSize(	 
	IN PNDIS_PACKET packet
);

BOOLEAN
FilterGetContent(	 
	IN PNDIS_PACKET packet,
	IN PVOID content,
	IN UINT size
);

UINT
FilterGetPacketSize(
	IN PNDIS_PACKET packet
);

BOOLEAN
FilterCapturePacketType(
	IN PADAPT pAdapt,
	IN PNDIS_PACKET packet
);

VOID
FilterSend(
	IN PADAPT pAdapt, 
	IN PNDIS_PACKET Packet
);

VOID
FilterStartupAdapter(
	OUT PNDIS_STATUS Status,	   
	IN  PADAPT pAdapt
);

VOID
FilterShutdownAdapter(
	IN PADAPT pAdapt
);

#endif __FILTER__H