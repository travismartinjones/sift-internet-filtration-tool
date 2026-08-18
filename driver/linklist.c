#include "precomp.h"

UINT				DbgNum = 0;	// debug number TEST CFilter
extern PIRP FilterPacketReceivedIRP;

VOID
FilterPrintPacketContents(
	IN PPNDIS_PACKET packet
)
{
	PNDIS_BUFFER	currentBuffer;
	UINT			bufferCount;
	UINT			packetLength;
	PUCHAR			VirtualAddress = NULL;
	PUCHAR			charBuffer;
	UINT			currentLength;
	UINT			i = 0;
	UINT			c = 0;


	ASSERT(KeGetCurrentIrql() == DISPATCH_LEVEL);
	// lock down adapters so we can modify it's contents
	
	DBGPRINT(("Packet Address: %u",*packet));
	NdisQueryPacket(
		*packet, // current packet
		NULL,		// physical buff count
		&bufferCount,	// buffer count
		&currentBuffer,
		&packetLength
		);
	DBGPRINT (("--New Packet--"));
	DBGPRINT (("bufferCount [%i] packetLength[%i]",bufferCount,packetLength));

	while(currentBuffer != NULL)
	{
		i++;
		DBGPRINT (("Parsing buffers: %i",i));

		NdisQueryBufferSafe(
			currentBuffer,
			&VirtualAddress,
			&currentLength,
			NormalPagePriority
			);

		for(c=0;c < currentLength;c++)
		{
			DBGPRINT(("Packet contents - [%i]%c",c,VirtualAddress[c]));
		}

		NdisGetNextBuffer(
			currentBuffer,
			&currentBuffer
			);
	}
}



FilterPrintPacketList (
	IN PPNDIS_PACKET_LIST_ELEMENT	list
)
{
	PNDIS_PACKET_LIST_ELEMENT currentElement;
	UINT i = 0;

	ASSERT(KeGetCurrentIrql() == DISPATCH_LEVEL);

	currentElement = *list;

	if(currentElement == NULL)
	{
		DBGPRINT(("FilterPrintPacketList: List is empty."));
	}

	while (currentElement != NULL)
	{
		DBGPRINT(("FilterPrintPacketList: [%u] Packet [%u] Adapter [%u]",
			      i,
				  (UINT)currentElement->Packet,
				  (UINT)currentElement->Adapter));
		currentElement = currentElement->Next;
		i++;
	}
}
VOID
FilterFreeFirstElement (
	IN PPNDIS_PACKET_LIST_ELEMENT	list
)
{
	PNDIS_PACKET_LIST_ELEMENT	oldHead;
	NDIS_STATUS		Status = NDIS_STATUS_SUCCESS;

	//DBGPRINT(("FilterFreeFirstElement()++"));

	ASSERT(KeGetCurrentIrql() == DISPATCH_LEVEL);

	oldHead = *list;
	*list = (*list)->Next;

	// free the list element memory
	NdisFreeMemory(oldHead,sizeof(NDIS_PACKET_LIST_ELEMENT),0);
	//DBGPRINT(("FilterFreeFirstElement()--"));
}

VOID
FilterSendCompleteList (
	IN PPNDIS_PACKET_LIST_ELEMENT	list
)
{
	PNDIS_PACKET_LIST_ELEMENT	currentElement;
	NDIS_STATUS		Status = NDIS_STATUS_SUCCESS;

	ASSERT(KeGetCurrentIrql() == DISPATCH_LEVEL);

	currentElement = *list;

	while (currentElement != NULL)
	{
		DBGPRINT(("FilterSendCompleteList: NdisMSendComplete"));
		NdisMSendComplete(ADAPT_MINIPORT_HANDLE(currentElement->Adapter),
							currentElement->Packet,
							Status);

		currentElement = currentElement->Next;
	}
}

BOOLEAN
FilterFreeElement(
	IN PPNDIS_PACKET_LIST_ELEMENT	list,
	IN PPNDIS_PACKET				packet
)
{
	BOOLEAN found = 0;
	PNDIS_PACKET_LIST_ELEMENT	currentElement;
	PNDIS_PACKET_LIST_ELEMENT	previousElement;
	UINT debugNum;

	ASSERT(KeGetCurrentIrql() == DISPATCH_LEVEL);	

	previousElement = NULL;

	currentElement = *list;

	// determine if the packet is in the sourceList
	while ((currentElement != NULL) && (!found))
	{
		if (currentElement->Packet == *packet)
		{
			found = 1;
		}
		else
		{
			previousElement = currentElement;
			currentElement = currentElement->Next;
		}
	}

	// if the packet is in the source list, remove it
	if (found)
	{
		//DBGPRINT(("FilterFreeElement: Element found"));
		if (previousElement == NULL)
		{
			DBGPRINT(("Element is the first element"));
			// the packet was the first element in the source list
			FilterFreeFirstElement(list);
		}
		else
		{
			NDIS_STATUS		Status = NDIS_STATUS_SUCCESS;

			DBGPRINT(("Element is not the first"));

			// the packet was not the first element
			previousElement->Next = currentElement->Next;

			//DBGPRINT(("FilterFreeElement%u: PreviousElement %p->%p CurrentElement %p->%p",
			//			debugNum,
			//			previousElement->Packet,
			//			previousElement->Next,
			//			currentElement->Packet,
			//			currentElement->Next));
			//DBGPRINT(("FilterFreeElement%u: NdisFreeMemory++ %p %p",debugNum,packet,currentElement->Packet));
			// free the list element memory

			NdisFreeMemory(currentElement,sizeof(NDIS_PACKET_LIST_ELEMENT),0);

			//DBGPRINT(("FilterFreeElement%u: NdisFreeMemory-- %p %p",debugNum,packet,currentElement->Packet));
		}

		// the packet has been freed
		packet = NULL;
	}

	//DBGPRINT(("FilterFreeElement: List after%u",debugNum));

	return found;
}

// Frees all packets and allocated memory referenced by the packet list
VOID
FilterFreePacketList (
	IN PPNDIS_PACKET_LIST_ELEMENT	list
)
{
	ASSERT(KeGetCurrentIrql() == DISPATCH_LEVEL);

	//DBGPRINT(("FilterFreePacketList()++"));
	while (*list != NULL)
	{
		// loop until all elements are freed
		FilterFreeFirstElement(list);
	}
	//DBGPRINT(("FilterFreePacketList()++"));
}

UINT
FilterPacketListLength (
	IN PPNDIS_PACKET_LIST_ELEMENT	list
)
{
	UINT		length = 0;
	PNDIS_PACKET_LIST_ELEMENT	CurrentElement;

	ASSERT(KeGetCurrentIrql() == DISPATCH_LEVEL);

	CurrentElement = *list;

	while (CurrentElement != NULL)
	{
		length++;
		//DBGPRINT(("ListLength [%u]: %u",length,(UINT)CurrentElement->Packet));
		CurrentElement = CurrentElement->Next;
	}

	return length;
}


VOID
FilterPacketListAdd (
	IN PPNDIS_PACKET_LIST_ELEMENT		list,
	IN PPNDIS_PACKET					Packet,
	IN PADAPT							Adapter
)
{
	PNDIS_PACKET_LIST_ELEMENT	newElement;

	ASSERT(KeGetCurrentIrql() == DISPATCH_LEVEL);


	// if the adapter is not open, drop or pass the packet
	// based on the USER_MODE_SETTINGS_DROP_DEFAULT setting
	if (!(Adapter->UserModeSettings & USER_MODE_SETTINGS_OPEN_ADAPTER))
	{
		DBGPRINT(("FilterCapturePacketType: Adapter is closed."));
		// the adapter is not open
		if(Adapter->UserModeSettings & USER_MODE_SETTINGS_DROP_DEFAULT)
		{				
			NDIS_STATUS Status = STATUS_SUCCESS;
			DBGPRINT(("FilterCapturePacketType: DROP"));
			// the default is to drop the packet, so drop the packet
			NdisMSendComplete(ADAPT_MINIPORT_HANDLE(Adapter),
							*Packet,
							Status);
			return;
		}
		else
		{
			DBGPRINT(("FilterCapturePacketType: ALLOW"));
			// the default is to allow the packet, send the packet forward
			FilterSend(Adapter,*Packet);
			return;
		}
	}

	NdisAllocateMemoryWithTag(&newElement,sizeof(NDIS_PACKET_LIST_ELEMENT),TAG);

	if(newElement == NULL)
	{
		DBGPRINT(("FilterPacketListAdd: Could not allocate needed memory"));
		return;
	}

	newElement->Packet = *Packet;
	newElement->Adapter = Adapter;
	
	newElement->Next = *list;
	*list = newElement;

	
	//Send message to user mode program that a packet has been received
	if (FilterPacketReceivedIRP != NULL)
	{
		DBGPRINT(("MPSendPackets: IRP signal complete to UserMode app"));
		FilterPacketReceivedIRP->IoStatus.Status = STATUS_SUCCESS;
		IoCompleteRequest(FilterPacketReceivedIRP,IO_NO_INCREMENT);
		FilterPacketReceivedIRP = NULL;
	}
	else
	{
		DBGPRINT(("MPSendPackets: IRP is null - user mode has not setup?"));
	}	
}


BOOLEAN
FilterPacketListSend(
	IN PADAPT	Adapter,
	IN UINT		Packet,
	IN UCHAR	Action
)
{
	PNDIS_PACKET_LIST_ELEMENT	currentElement;
	BOOLEAN						found = 0;

	ASSERT(KeGetCurrentIrql() == DISPATCH_LEVEL);

	currentElement = Adapter->FilterSendPendingAction;

	// determine if the packet is in the FilterSendList
	while ((currentElement != NULL) && (!found))
	{
		//DBGPRINT(("FilterPacketListSend: Searching [%p]%u - %u",(UINT)currentElement->Packet,currentElement->Packet,Packet));
		if ((UINT)currentElement->Packet == Packet)
		{
			found = 1;
		}
		else
		{
			currentElement = currentElement->Next;
		}
	}

	if (found)
	{	
		if(PACKET_ACTION_ALLOW & Action)
		{
			//DBGPRINT(("FilterPacketListSend: Found, FilterSend()"));
			FilterSend(Adapter,currentElement->Packet);
		}
		else if (PACKET_ACTION_DROP & Action)
		{
			NDIS_STATUS Status = NDIS_STATUS_SUCCESS;
			DBGPRINT(("FilterPacketListSend: DROP"));
			NdisMSendComplete(ADAPT_MINIPORT_HANDLE(Adapter),
				              currentElement->Packet,
			                  Status);
		}
		//DBGPRINT(("FilterPacketListSend: Found, FilterFreeElement()"));		
		FilterFreeElement(&Adapter->FilterSendPendingAction,&currentElement->Packet);
	}
	else
	{
		DBGPRINT(("FilterPacketListSend: Not Found"));
	}
	
	//DBGPRINT(("FilterPacketListSend-- return %u",found));

	return found;
}

BOOLEAN
FilterMoveElement (
	IN PPNDIS_PACKET_LIST_ELEMENT	sourceList,
	IN PPNDIS_PACKET				packet,
	IN PPNDIS_PACKET_LIST_ELEMENT	targetList
)
{
	BOOLEAN found = 0;
	PNDIS_PACKET_LIST_ELEMENT	currentElement;
	PNDIS_PACKET_LIST_ELEMENT	previousElement;

	ASSERT(KeGetCurrentIrql() == DISPATCH_LEVEL);

	currentElement = *sourceList;
	previousElement = NULL;

	//DBGPRINT(("FilterMoveElement++"));

	// determine if the packet is in the sourceList
	while ((currentElement != NULL) && (!found))
	{
		if (currentElement->Packet == *packet)
		{
			found = 1;
		}
		else
		{
			previousElement = currentElement;
			currentElement = currentElement->Next;
		}
	}
	
	if (currentElement == NULL)
	{
		DBGPRINT(("FilterMoveElement: Element not found"));
	}

	// if the packet is in the source list, move it to 
	// the targetList
	if (found)
	{
		if (previousElement == NULL)
		{
			//DBGPRINT(("FilterMoveElement: Element is the first element"));
			// the packet was the first element in the source list
			previousElement = currentElement->Next;
			*sourceList = previousElement;
		}
		else
		{
			//DBGPRINT(("FilterMoveElement: Element is not the first"));
			// the packet was not the first element
			previousElement->Next = currentElement->Next;
		}
		
		currentElement->Next = *targetList;
		*targetList = currentElement;
	}
	return found;
}