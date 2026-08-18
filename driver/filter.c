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
/		Filter functions are defined here. 
/
/
/	Change log :
/		07.28.05	Original file creation
/
*/

#include "precomp.h"

BOOLEAN
FilterGetPacketValue(IN PNDIS_PACKET packet,IN PVOID value,IN UINT valueBegin,IN UINT valueLength)
{
	PNDIS_BUFFER		currentBuffer;
	PUCHAR				pValue = (PUCHAR)value;
	PUCHAR				VirtualAddress = NULL;
	BOOLEAN				capturePacket = 0;
	UINT				amountRead = 0;
	UINT				bufferLength;
	UINT				bufferCount;
	UINT				packetLength;
	UINT				amountCopied = 0;
	UINT				currentAmtCopied = 0;
	UINT				currentBegin = 0;
	UINT				currentLength = 0;

	if (packet == NULL)
		return FALSE;

	NdisQueryPacket(
		packet, // current packet
		NULL,		// physical buff count
		&bufferCount,	// buffer count
		&currentBuffer,
		&packetLength
		);

	currentBegin = valueBegin;
	currentLength = valueLength;

	while(currentBuffer != NULL)
	{		
#if (defined(NDIS50) || defined(NDIS51))
		NdisQueryBufferSafe(
			currentBuffer,
			&VirtualAddress,
			&bufferLength,
			NormalPagePriority
			);
#else
		NdisQueryBuffer(
			currentBuffer,
			&VirtualAddress,
			&bufferLength
			);
#endif
		
		currentAmtCopied = 0;

		//DBGPRINT(("++currentBegin %u, currentLength %u, amountRead %u, bufferLength %u, currentAmtCopied %u amountCopied %u",
		//			currentBegin,
		//			currentLength,
		//			amountRead,
		//			bufferLength,
		//			currentAmtCopied,
		//			amountCopied));

		if (currentBegin >= amountRead && currentBegin < (amountRead + bufferLength))
		{
			// there is data in the current buffer that we need to
			// copy into the value pointer

			// get the remaining amount to copy
			currentAmtCopied = valueLength - amountCopied;
			if (currentBegin + currentLength > amountRead + bufferLength)
			{
				// adjust the amount to how much of the remaining data
				// that actually resides in the current buffer
				currentAmtCopied = bufferLength - (currentBegin - amountRead);
			}

			if (currentAmtCopied > 0)
			{
				//DBGPRINT(("FilterGetPacketValue: Copying %u bytes",currentAmtCopied));
				// copy the requested data
				
				NdisMoveMemory(	pValue,
								&VirtualAddress[currentBegin - amountRead],
								currentAmtCopied);
	
				//DBGPRINT(("FilterGetPacketValue: Copy complete"));
				// add to our running total to our total amount copied
				amountCopied += currentAmtCopied;

				if (amountCopied == valueLength)
				{
					// we have copied all requested data into value
					// return TRUE to indicate success
					//DBGPRINT(("FilterGetPacketValue-- TRUE"));
					return TRUE;
				}

				// there is more data to copy, move ahead the amount copied
				// and decriment the amount of data we have remaining to copy
				currentBegin += currentAmtCopied;
				currentLength -= currentAmtCopied;
				pValue += currentAmtCopied;
			}
		}

		amountRead += bufferLength;

		//DBGPRINT(("--currentBegin %u, currentLength %u, amountRead %u, bufferLength %u, currentAmtCopied %u amountCopied %u",
		//			currentBegin,
		//			currentLength,
		//			amountRead,
		//			bufferLength,
		//			currentAmtCopied,
		//			amountCopied));

		NdisGetNextBuffer(
			currentBuffer,
			&currentBuffer
			);
	}

	//DBGPRINT(("FilterGetPacketValue-- FALSE"));
	// we've reached the end of the packet and did
	// not copy all the requested data
	// return FALSE to indicate failure
	return FALSE;
}

USHORT 
FilterGetEthernetType(IN PNDIS_PACKET packet)
{
	USHORT ethernetType;

	//DBGPRINT(("FilterGetEthernetType++"));
	if(
		FilterGetPacketValue(packet,
							 (PVOID)&ethernetType,
							 FIELD_OFFSET(ETHERNET_HEADER,type),
							 sizeof(USHORT))
	  )
	{
		//DBGPRINT(("FilterGetEthernetType-- %u",ethernetType));
		return convertEndianShort(ethernetType);
	}

	//DBGPRINT(("FilterGetEthernetType-- return 0"));
	return 0;
}

UCHAR
FilterGetIPVersion(IN PNDIS_PACKET packet)
{	
	UCHAR	ipVersion;

	//DBGPRINT(("FilterGetIPVersion++"));
	if(
		FilterGetPacketValue(packet,
							 (PVOID)&ipVersion,
							 sizeof(ETHERNET_HEADER),
							 sizeof(UCHAR))
	  )
	{

		//DBGPRINT(("FilterGetIPVersion--A %u",ipVersion));
		// adjust for the byte order
#if BYTE_ORDER == LITTLE_ENDIAN
		ipVersion &= 240;  // 1111 0000
		ipVersion = ipVersion >> 4;
#elif BYTE_ORDER == BIG_ENDIAN
		ipVersion &= 15;  //  0000 1111
#endif
		//DBGPRINT(("FilterGetIPVersion--B %u",ipVersion));
		return ipVersion;
	}

	//DBGPRINT(("FilterGetIPVersion-- return 0"));
	return 0;
}

UCHAR
FilterGetIPProtocol(IN PNDIS_PACKET packet)
{
	UCHAR	ipProtocol;

	//DBGPRINT(("FilterGetIPProtocol++"));
	if(
		FilterGetPacketValue(packet,
							 (PVOID)&ipProtocol,
							 sizeof(ETHERNET_HEADER)+FIELD_OFFSET(IP_HEADER,protocol),
							 sizeof(UCHAR))
	  )
	{
		//DBGPRINT(("FilterGetIPProtocol-- %u",ipProtocol));
		return ipProtocol;
	}

	//DBGPRINT(("FilterGetIPProtocol-- return 0"));
	return 0;
}

ULONG
FilterGetSourceIP(IN PNDIS_PACKET packet)
{
	ULONG	sourceIP;

	//DBGPRINT(("FilterGetSourceIP++"));
	if(
		FilterGetPacketValue(packet,
							 (PVOID)&sourceIP,
							 sizeof(ETHERNET_HEADER)+FIELD_OFFSET(IP_HEADER,source),
							 sizeof(ULONG))
	  )
	{
		//DBGPRINT(("FilterGetSourceIP-- %u.%u.%u.%u",
					//*((PUCHAR)(&sourceIP)),
					//*((PUCHAR)(&sourceIP)+1),
					//*((PUCHAR)(&sourceIP)+2),
					//*((PUCHAR)(&sourceIP)+3)));
		return convertEndianLong(sourceIP);
	}

	//DBGPRINT(("FilterGetSourceIP-- return 0"));
	return 0;
}

ULONG
FilterGetDestinationIP(IN PNDIS_PACKET packet)
{
	ULONG	destinationIP;

	//DBGPRINT(("FilterGetDestinationIP++"));
	if(
		FilterGetPacketValue(packet,
							 (PVOID)&destinationIP,
							 sizeof(ETHERNET_HEADER)+FIELD_OFFSET(IP_HEADER,destination),
							 sizeof(ULONG))
	  )
	{
		//DBGPRINT(("FilterGetDestinationIP-- %u.%u.%u.%u",
					//*((PUCHAR)(&destinationIP)),
					//*((PUCHAR)(&destinationIP)+1),
					//*((PUCHAR)(&destinationIP)+2),
					//*((PUCHAR)(&destinationIP)+3)));
		return convertEndianLong(destinationIP);
	}

	//DBGPRINT(("FilterGetDestinationIP-- return 0"));
	return 0;
}

USHORT
FilterGetSourcePort(IN PNDIS_PACKET packet)
{
	USHORT	sourcePort;

	//DBGPRINT(("FilterGetSourcePort++"));
	if(
		// FIELD_OFFSET for TCP and UDP is the same, so we use
		// the TCP_HEADER definition for both
		FilterGetPacketValue(packet,
							 (PVOID)&sourcePort,
							 sizeof(ETHERNET_HEADER)+sizeof(IP_HEADER)+FIELD_OFFSET(TCP_HEADER,sourcePort),
							 sizeof(USHORT))
	  )
	{
		//DBGPRINT(("FilterGetSourcePort-- %u",sourcePort));
		return convertEndianShort(sourcePort);
	}

	//DBGPRINT(("FilterGetSourcePort-- return 0"));
	return 0;
}

USHORT
FilterGetDestinationPort(IN PNDIS_PACKET packet)
{
	USHORT	destinationPort;

	//DBGPRINT(("FilterGetDestinationPort++"));
	if(
		// FIELD_OFFSET for TCP and UDP is the same, so we use
		// the TCP_HEADER definition for both
		FilterGetPacketValue(packet,
							 (PVOID)&destinationPort,
							 sizeof(ETHERNET_HEADER)+sizeof(IP_HEADER)+FIELD_OFFSET(TCP_HEADER,destinationPort),
							 sizeof(USHORT))
	  )
	{

		//DBGPRINT(("FilterGetDestinationPort-- %u",destinationPort));
		return convertEndianShort(destinationPort);
	}

	//DBGPRINT(("FilterGetDestinationPort-- return 0"));
	return 0;
}

UINT
FilterGetPacketLength(IN PNDIS_PACKET packet)
{
	PNDIS_BUFFER		currentBuffer;
	PUCHAR				VirtualAddress = NULL;
	UINT				contentSize;
	UINT				bufferCount;
	UINT				length;

	NdisQueryPacket(
		packet, // current packet
		NULL,		// physical buff count
		&bufferCount,	// buffer count
		&currentBuffer,
		&length
		);

	return length;
}


UINT
FilterGetContentSize(IN PNDIS_PACKET packet)
{
	// should only be called when the packet type is TCP/IP
	// we could add checks to verify these values, however, due to
	// the time critical nature of the packet handling functions, we
	// will perform these checks prior to calling FilterGetContentSize

	
	// the content is all that remains after we remove the encapsulating headers
	return (UINT)(FilterGetPacketLength(packet) - 
		          (sizeof(ETHERNET_HEADER) + sizeof(IP_HEADER) + sizeof(TCP_HEADER)));
}

BOOLEAN
FilterGetContent(IN PNDIS_PACKET packet,IN PVOID content,IN UINT size)
{
	PNDIS_BUFFER		currentBuffer;
	UINT				bufferCount;

	// check to see if the requested size if valid
	if((size + sizeof(ETHERNET_HEADER)+sizeof(IP_HEADER)+sizeof(TCP_HEADER)) > FilterGetPacketLength(packet))
	{
		DBGPRINT(("FilterGetContent: Requested size is invalid %u > %u",
				(size + sizeof(ETHERNET_HEADER)+sizeof(IP_HEADER)+sizeof(TCP_HEADER)),
				FilterGetPacketLength(packet)));

		return FALSE;
	}

	//DBGPRINT(("FilterGetContent--: %p %u",content,size));

	return FilterGetPacketValue(packet,
								(PVOID)content,
								sizeof(ETHERNET_HEADER)+sizeof(IP_HEADER)+sizeof(TCP_HEADER),
								size);
}

UINT
FilterGetPacketSize(IN PNDIS_PACKET packet)
{
	if(packet != NULL)
	{
		PNDIS_BUFFER	currentBuffer;
		UINT			bufferCount;
		ULONG			packetLength;
		
		NdisQueryPacket(
			packet, // current packet
			NULL,		// physical buff count
			&bufferCount,	// buffer count
			&currentBuffer,
			&packetLength
			);		

		return packetLength;
	}
	else
	{
		return 0;
	}
}


BOOLEAN
FilterCapturePacketType(IN PADAPT pAdapt,IN PNDIS_PACKET packet)
{
	UCHAR	ipVersion;
	UCHAR	ipProtocol;

	// returns true if we should capture this type of packet
	// currently we deal only with TCP/IP HTTP requests and
	// the adapter is open

	// capture only IP ethernet packets
	if(FilterGetEthernetType(packet) != ETHERNET_HEADER_TYPE_IP)
	{
		return FALSE;
	}

	ipVersion = FilterGetIPVersion(packet);
	ipProtocol = FilterGetIPProtocol(packet);

	// capture the packet if we are capturing global UDP or TCP
	// and the protocol matches the flag setting, or if all IP
	// packets are to be captured
	if (ipVersion == IP_VERSION_4 && 
	   ((ipProtocol == IP_TCP_PROTOCOL && (pAdapt->UserModeSettings & USER_MODE_SETTINGS_CAPTURE_TCPIP)) ||
	    (ipProtocol == IP_UDP_PROTOCOL && (pAdapt->UserModeSettings & USER_MODE_SETTINGS_CAPTURE_UDPIP)) ||
		(pAdapt->UserModeSettings & USER_MODE_SETTINGS_CAPTURE_ALL)
		))
	{		
		DBGPRINT(("CAPTURING GLOBAL PROTOCOL"));
		return TRUE;
	}

	if (ipVersion != IP_VERSION_4 || ipProtocol != IP_TCP_PROTOCOL)
	{
		// the packet is not IP version 4 and is not encapsulated by the TCP protocol
		// so we will not filter this packet type
		return FALSE;
	}

	// check the TCP header
	// Destination port must = 80 (HTTP) or 443 (HTTPS)
	// and contain packet contents, such as in the
	// case with a GET/POST request packet
	
	if((FilterGetDestinationPort(packet) == FILTER_HTTP_PORT || FilterGetDestinationPort(packet) == FILTER_HTTPS_PORT) && 
	   (FilterGetPacketLength(packet) > sizeof(ETHERNET_HEADER) + sizeof(IP_HEADER) + sizeof(TCP_HEADER)) &&
	   (pAdapt->UserModeSettings & USER_MODE_SETTINGS_CAPTURE_HTTP))
	{
		DBGPRINT(("CAPTURING HTTP(S)"));
		return TRUE;
	}

	return FALSE;
}

VOID
FilterSend(IN PADAPT pAdapt,IN PNDIS_PACKET Packet)
{
	PNDIS_PACKET    MyPacket;
	NDIS_STATUS		Status;
	PVOID			MediaSpecificInfo = NULL;
	UINT			MediaSpecificInfoSize = 0;

	//DBGPRINT(("FilterSend()++"));
	//
    // The driver should fail the send if the virtual miniport is in low 
	// power state
	//
	if (pAdapt->MPDeviceState > NdisDeviceStateD0)
	{
		NdisMSendComplete(ADAPT_MINIPORT_HANDLE(pAdapt),
							Packet,
							NDIS_STATUS_FAILURE);
		return;
	}

#ifdef NDIS51

        //
        // Use NDIS 5.1 packet stacking:
        //
        {
			PNDIS_PACKET_STACK        pStack;
			BOOLEAN                   Remaining;

			//
			// Packet stacks: Check if we can use the same packet for sending down.
			//
			pStack = NdisIMGetCurrentPacketStack(Packet, &Remaining);
			if (Remaining)
			{
            //
            // We can reuse "Packet".
            //
            // NOTE: if we needed to keep per-packet information in packets
            // sent down, we can use pStack->IMReserved[].
            //
            ASSERT(pStack);
            //
            // If the below miniport is going to low power state, stop sending down any packet.
            //
            NdisAcquireSpinLock(&pAdapt->Lock);
            if (pAdapt->PTDeviceState > NdisDeviceStateD0)
            {
                NdisReleaseSpinLock(&pAdapt->Lock);
                NdisMSendComplete(ADAPT_MINIPORT_HANDLE(pAdapt),
                                    Packet,
                                    NDIS_STATUS_FAILURE);
            }
            else
            {
                pAdapt->OutstandingSends++;
                NdisReleaseSpinLock(&pAdapt->Lock);
            
                NdisSend(&Status,
                            pAdapt->BindingHandle,
                            Packet);
    
                if (Status != NDIS_STATUS_PENDING)
                {
                    NdisMSendComplete(ADAPT_MINIPORT_HANDLE(pAdapt),
                                        Packet,
                                        Status);
                
                    ADAPT_DECR_PENDING_SENDS(pAdapt);
                }
            }
            return;
        }
    }
#endif
        do 
        {
            NdisAcquireSpinLock(&pAdapt->Lock);
            //
            // If the below miniport is going to low power state, stop sending down any packet.
            //
            if (pAdapt->PTDeviceState > NdisDeviceStateD0)
            {
                NdisReleaseSpinLock(&pAdapt->Lock);
                Status = NDIS_STATUS_FAILURE;
                break;
            }
            pAdapt->OutstandingSends++;
            NdisReleaseSpinLock(&pAdapt->Lock);
            
            NdisAllocatePacket(&Status,
                               &MyPacket,
                               pAdapt->SendPacketPoolHandle);

            if (Status == NDIS_STATUS_SUCCESS)
            {
                PSEND_RSVD        SendRsvd;

                SendRsvd = (PSEND_RSVD)(MyPacket->ProtocolReserved);
                SendRsvd->OriginalPkt = Packet;

                NdisGetPacketFlags(MyPacket) = NdisGetPacketFlags(Packet);

                NDIS_PACKET_FIRST_NDIS_BUFFER(MyPacket) = NDIS_PACKET_FIRST_NDIS_BUFFER(Packet);
                NDIS_PACKET_LAST_NDIS_BUFFER(MyPacket) = NDIS_PACKET_LAST_NDIS_BUFFER(Packet);
#ifdef WIN9X
                //
                // Work around the fact that NDIS does not initialize this
                // to FALSE on Win9x.
                //
                NDIS_PACKET_VALID_COUNTS(MyPacket) = FALSE;
#endif // WIN9X

                //
                // Copy the OOB data from the original packet to the new
                // packet.
                //
                NdisMoveMemory(NDIS_OOB_DATA_FROM_PACKET(MyPacket),
                            NDIS_OOB_DATA_FROM_PACKET(Packet),
                            sizeof(NDIS_PACKET_OOB_DATA));
                //
                // Copy relevant parts of the per packet info into the new packet
                //
#ifndef WIN9X
                NdisIMCopySendPerPacketInfo(MyPacket, Packet);
#endif

                //
                // Copy the Media specific information
                //
                NDIS_GET_PACKET_MEDIA_SPECIFIC_INFO(Packet,
                                                    &MediaSpecificInfo,
                                                    &MediaSpecificInfoSize);

                if (MediaSpecificInfo || MediaSpecificInfoSize)
                {
                    NDIS_SET_PACKET_MEDIA_SPECIFIC_INFO(MyPacket,
                                                        MediaSpecificInfo,
                                                        MediaSpecificInfoSize);
                }

                NdisSend(&Status,
                         pAdapt->BindingHandle,
                         MyPacket);

                if (Status != NDIS_STATUS_PENDING)
                {
#ifndef WIN9X
                    NdisIMCopySendCompletePerPacketInfo (Packet, MyPacket);
#endif
                    NdisFreePacket(MyPacket);
                    ADAPT_DECR_PENDING_SENDS(pAdapt);
                }
            }
            else
            {
                //
                // The driver cannot allocate a packet.
                // 
                ADAPT_DECR_PENDING_SENDS(pAdapt);
            }
        }
        while (FALSE);

        if (Status != NDIS_STATUS_PENDING)
        {
            NdisMSendComplete(ADAPT_MINIPORT_HANDLE(pAdapt),
                              Packet,
                              Status);
        }


	//DBGPRINT(("FilterSend()--"));
}

VOID
FilterStartupAdapter(OUT PNDIS_STATUS Status, IN PADAPT pAdapt)
{
	// returns TRUE if all setups are successful
	// returns FALSE if any failure was encountered, 
	// causes PtBindAdapter to execute a break statement
	NDIS_STATUS ndisStatus;

	DBGPRINT(("FilterSetupAdapter()++"));	
	// Initialize packet lists to NULL
	pAdapt->FilterSendList = NULL;
	pAdapt->FilterRecvList = NULL;
	pAdapt->FilterSendPendingAction = NULL;
	pAdapt->FilterRecvPendingAction = NULL;
		
	pAdapt->UserModeSettings = USER_MODE_SETTINGS_DROP_DEFAULT |
		                       USER_MODE_SETTINGS_CAPTURE_SEND |
							   USER_MODE_SETTINGS_CAPTURE_HTTP; // open adapter, capture sends, capture HTTP

	// allocate the lock used to raise IRQL for IO requests
	NdisAllocateSpinLock(&pAdapt->FilterLock);

	// allocate recv buffer pool
	NdisAllocateBufferPool(&ndisStatus,&pAdapt->FilterRecvBufferPool,100);
	NdisAllocateBufferPool(&ndisStatus,&pAdapt->FilterSendBufferPool,100);

	//DBGPRINT(("FilterSetupAdapter()--"));
}
VOID
FilterShutdownAdapter(IN PADAPT pAdapt)
{
	DBGPRINT(("FilterShutdownAdapter()++"));
	if (pAdapt != NULL)
	{
		//
		// Free all packets placed into the filter packet pools
		// and free any memory allocated to our reference lists
		//
		if (pAdapt->FilterSendList != NULL)
		{
			// lockdown the adapter while we free the packets
			NdisAcquireSpinLock(&pAdapt->FilterLock);

			// Free the memory allocated to the FilterSend list			
			FilterSendCompleteList(&pAdapt->FilterSendList);
			FilterFreePacketList(&pAdapt->FilterSendList);
			
			// free adapter lock
			NdisReleaseSpinLock(&pAdapt->FilterLock);
		}

		if (pAdapt->FilterRecvList != NULL)
		{
			// lockdown the adapter while we free the packets
			NdisAcquireSpinLock(&pAdapt->FilterLock);

			// Free the memory allocated to the FilterRecvUnhandled list
			FilterSendCompleteList(&pAdapt->FilterRecvList);
			FilterFreePacketList(&pAdapt->FilterRecvList);

			// free adapter lock
			NdisReleaseSpinLock(&pAdapt->FilterLock);
		}
		if (pAdapt->FilterSendPendingAction != NULL)
		{
			// lockdown the adapter while we free the packets
			NdisAcquireSpinLock(&pAdapt->FilterLock);

			// Free the memory allocated to the FilterSend list			
			FilterSendCompleteList(&pAdapt->FilterSendPendingAction);
			FilterFreePacketList(&pAdapt->FilterSendPendingAction);

			// free adapter lock
			NdisReleaseSpinLock(&pAdapt->FilterLock);
		}

		if (pAdapt->FilterRecvPendingAction != NULL)
		{
			// lockdown the adapter while we free the packets
			NdisAcquireSpinLock(&pAdapt->FilterLock);

			// Free the memory allocated to the FilterRecvUnhandled list
			FilterSendCompleteList(&pAdapt->FilterRecvPendingAction);
			FilterFreePacketList(&pAdapt->FilterRecvPendingAction);

			// free adapter lock
			NdisReleaseSpinLock(&pAdapt->FilterLock);		
		}

		// free the lock used to raise IRQL for IO requests
		NdisFreeSpinLock(&pAdapt->FilterLock);


	}
	else
	{
		DBGPRINT(("FilterShutdownAdapter: pAdapt == NULL"));
	}
	DBGPRINT(("FilterShutdownAdapter()--"));
}