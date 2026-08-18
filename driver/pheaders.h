
#ifndef __PHEADERS__H
#define __PHEADERS__H


// endian conversion macros

#if BYTE_ORDER == LITTLE_ENDIAN
// convert from/to big endian to/from little endian
// use bit shifting to swap bytes
#define convertEndianShort(A)  ((((USHORT)(A) & 0xff00) >> 8) | \
					            (((USHORT)(A) & 0x00ff) << 8))
#define convertEndianLong(A)   ((((ULONG)(A) & 0xff000000) >> 24) | \
					            (((ULONG)(A) & 0x00ff0000) >> 8)  | \
                                (((ULONG)(A) & 0x0000ff00) << 8)  | \
                                (((ULONG)(A) & 0x000000ff) << 24))

#elif BYTE_ORDER == BIG_ENDIAN
// do nothing if big endian
#define convertEndianShort(A)  (A)
#define convertEndianLong(A)   (A)

#endif

// ethernet header stuct

#define ETHERNET_HEADER_TYPE_IP	0x0800 // IP
#define IP_TCP_PROTOCOL			6
#define IP_UDP_PROTOCOL		   17
#define IP_VERSION_4			4

typedef struct _ETHERNET_HEADER {
	UCHAR	destinationHost[6];
	UCHAR	sourceHost[6];
	USHORT	type;
} ETHERNET_HEADER, *PETHERNET_HEADER;

typedef struct _ETHERNET_ADDRESS {
	UCHAR	octet[6];
} ETHERNET_ADDRESS, *PETHERNET_ADDRESS;

// ip header struct

typedef struct _IP_HEADER 
{
#if BYTE_ORDER == LITTLE_ENDIAN
	UCHAR	headerLength:4;
	UCHAR	version:4;
#elif BYTE_ORDER == BIG_ENDIAN
	UCHAR	version:4;
	UCHAR	headerLength:4;
#endif
	UCHAR	type;
	USHORT	packetLength;
	USHORT	id;
	USHORT	offset;
	UCHAR	timeToLive;
	UCHAR	protocol;
	USHORT	checksum;
	ULONG	source;
	ULONG	destination;
} IP_HEADER, *PIP_HEADER;

// tcp header struct

typedef struct _TCP_HEADER 
{
	USHORT	sourcePort;
	USHORT	destinationPort;
	ULONG	sequenceNum;
	ULONG	acknowledgementNum;
#if BYTE_ORDER == LITTLE_ENDIAN
	UCHAR	unused:4;
	UCHAR	offset:4;
#elif BYTE_ORDER == BIG_ENDIAN 
	UCHAR	offset:4;
	UCHAR	unused:4;
#endif 
	UCHAR	flags;
	USHORT	window;
	USHORT	checksum;
	USHORT	urgentPointer;
} TCP_HEADER, *PTCP_HEADER;

typedef struct _UDP_HEADER
{
	USHORT	sourcePort;
	USHORT	destinationPort;
	USHORT	length;
	USHORT	checksum;
} UDP_HEADER, *PUDP_HEADER;

#endif __PHEADERS__H