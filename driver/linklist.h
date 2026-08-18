#ifndef __LINKLIST__H
#define __LISKLIST__H

typedef struct _NDIS_PACKET_LIST_ELEMENT {
	PNDIS_PACKET						Packet;
	PADAPT								Adapter;
	struct _NDIS_PACKET_LIST_ELEMENT	*Next;
} NDIS_PACKET_LIST_ELEMENT, *PNDIS_PACKET_LIST_ELEMENT, **PPNDIS_PACKET_LIST_ELEMENT;

VOID
FilterPrintPacketContents(
	IN PPNDIS_PACKET packet
);

FilterPrintPacketList (
	IN PPNDIS_PACKET_LIST_ELEMENT	list
);

VOID
FilterFreeFirstElement (
	IN PPNDIS_PACKET_LIST_ELEMENT list
);

VOID
FilterSendCompleteList (
	IN PPNDIS_PACKET_LIST_ELEMENT	list
);

BOOLEAN
FilterFreeElement(
	IN PPNDIS_PACKET_LIST_ELEMENT	list,
	IN PPNDIS_PACKET				packet
);

// Used to free the memory allocated by the Filter Packet Lists
VOID
FilterFreePacketList (
	IN PPNDIS_PACKET_LIST_ELEMENT	list
	);

UINT
FilterPacketListLength (
	IN PPNDIS_PACKET_LIST_ELEMENT list
);

VOID
FilterPacketListAdd (
	IN PPNDIS_PACKET_LIST_ELEMENT	list,
	IN PPNDIS_PACKET				Packet,
	IN PADAPT						Adapter
);

BOOLEAN
FilterPacketListSend(
	IN PADAPT	Adapter,
	IN UINT		Packet,
	IN UCHAR	Action
);

BOOLEAN
FilterMoveElement (
	IN PPNDIS_PACKET_LIST_ELEMENT	sourceList,
	IN PPNDIS_PACKET				packet,
	IN PPNDIS_PACKET_LIST_ELEMENT	targetList
);

#endif __LINKLIST__H