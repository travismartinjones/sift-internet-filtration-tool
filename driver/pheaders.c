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
/		The protocol header utility functions are defined here. 
/
/
/	Change log :
/		07.28.05	Original file creation
/
*/

#include "precomp.h"

BOOLEAN
FilterCapturePacketType(PADAPT pAdapt,PNDIS_PACKET packet)
{
	// returns true if we should capture this type of packet
	// currently we deal only with TCP/IP HTTP requests
	PNDIS_BUFFER		currentBuffer;
	PUCHAR				VirtualAddress = NULL;
	BOOLEAN				capturePacket = 0;
	UINT				amountRead = 0;
	UINT				currentLength;
	UINT				bufferCount;
	UINT				packetLength;
	PUCHAR				test;
	UINT				i;
	UCHAR				endianTranslate[2];
	PETHERNET_HEADER	ethernetHeader;
	PIP_HEADER			ipHeader;
	PTCP_HEADER			tcpHeader;


	NdisQueryPacket(
		packet, // current packet
		NULL,		// physical buff count
		&bufferCount,	// buffer count
		&currentBuffer,
		&packetLength
		);
	DBGPRINT(("--- packet mark -- bufferCount: %u",bufferCount));
	while(currentBuffer != NULL)
	{

		DBGPRINT(("--- buffer mark ---"));
#if (defined(NDIS50) || defined(NDIS51))
		NdisQueryBufferSafe(
			currentBuffer,
			&VirtualAddress,
			&currentLength,
			NormalPagePriority
			);
#else
		NdisQueryBuffer(
			currentBuffer,
			&VirtualAddress,
			&currentLength
			);
#endif

		// check the ethernet header
		if (amountRead == 0 && currentLength == 14)
		{
			ethernetHeader = (PETHERNET_HEADER)VirtualAddress;

			DBGPRINT(("Ethernet Type: %u",convertEndianShort(ethernetHeader->type)));

			if (!(ETHERNET_HEADER_TYPE_IP == convertEndianShort(ethernetHeader->type)))
			{
				return 0;
			}
		}

		// check the IP header
		// IP Version must	= 4 (IP Version 4)
		// Type must		= 6 (TCP)
		if (amountRead == 14 && currentLength == 20)
		{
			ipHeader = (PIP_HEADER)VirtualAddress;

			DBGPRINT(("IP Offset : %u IP Version : %u IP Protocol : %u",convertEndianShort(ipHeader->offset),ipHeader->version,ipHeader->protocol));
			DBGPRINT(("Source IP : %u.%u.%u.%u Destination IP : %u.%u.%u.%u",*((PUCHAR)(&ipHeader->source)),
				                                     *((PUCHAR)(&ipHeader->source)+1),
										      	     *((PUCHAR)(&ipHeader->source)+2),
											         *((PUCHAR)(&ipHeader->source)+3),
													 *((PUCHAR)(&ipHeader->destination)),
				                                     *((PUCHAR)(&ipHeader->destination)+1),
											         *((PUCHAR)(&ipHeader->destination)+2),
											         *((PUCHAR)(&ipHeader->destination)+3)));

			if (ipHeader->version == IP_VERSION_4 && 
			   ((ipHeader->protocol == IP_TCP_PROTOCOL && (pAdapt->UserModeSettings & USER_MODE_SETTINGS_CAPTURE_TCPIP)) ||
			    (ipHeader->protocol == IP_UDP_PROTOCOL && (pAdapt->UserModeSettings & USER_MODE_SETTINGS_CAPTURE_UDPIP))))
			{
				DBGPRINT (("Adding global TCP or UDP"));
				// check to see if we are capturing all UDP or TCP
				return 1;
			}


			if (ipHeader->version != IP_VERSION_4 || ipHeader->protocol != IP_TCP_PROTOCOL)
			{
				// the packet is not IP version 4 and is not encapsulated by the TCP protocol
				// so we will not filter this packet type
				return 0;
			}
		}

		// check the TCP header
		// Destination port must = 80 (HTTP) or 443 (HTTPS)
		//
		// *** add logic to adjust for IP offset amount ***
		if (amountRead == 34  && currentLength >= 20)
		{
			tcpHeader = (PTCP_HEADER)VirtualAddress;

			DBGPRINT(("SrcPort : %u DstPort: %u Offset : %u",
					  convertEndianShort(tcpHeader->sourcePort),
					  convertEndianShort(tcpHeader->destinationPort),
					  tcpHeader->offset));

			if (convertEndianShort(tcpHeader->destinationPort) == FILTER_HTTP_PORT)
			{
				DBGPRINT(("Packet is HTTP or HTTPS, capturing"));
				return 1;
			}
			else
			{
				return 0;
			}
		}
		//DBGPRINT(("Length: %u",currentLength));
		//DBGPRINT(("Read: %u",amountRead));
		//if (amountRead > 34)
		//{
		//	// the packets is TCP/IP HTTP
		//	// print out the packet contents
		//	
		//	test = (PUCHAR)VirtualAddress;
		//	for(i=0;i<currentLength;i++)
		//	{
		//		DBGPRINT(("Packet [%u] %u\t%c",i,(UINT)test[i],(CHAR)test[i]));
		//	}
		//}

		amountRead += currentLength;

		NdisGetNextBuffer(
			currentBuffer,
			&currentBuffer
			);
	}

	return capturePacket;
}