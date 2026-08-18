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
/		The filter hook functions are defined in this file. 
/
/		The user mode application attaches to the following functions -
/
/			HookCreate(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp) 
/			HookDispatch(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp)
/			HookClose(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp)
/			HookDeviceControl(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp)
/		
/			The headers and portions of the body are inhertited from the
/			PtDispatch function that was replaced in PASSTHRU.C
/		
/		
/
/	Change log :
/		06.14.05	Original file creation
/
*/

#include "precomp.h"

PIRP FilterPacketReceivedIRP = NULL;

NTSTATUS ListAdapters(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp)
{
	NTSTATUS		status = STATUS_SUCCESS;	

	PIO_STACK_LOCATION	IrpStack = NULL;
	ULONG				returnSize = 0;
	PUCHAR				userSpaceBuffer = NULL;
	ULONG				userSpaceInputLength = 0;
	ULONG				userSpaceOutputLength = 0;
	ULONG				userSpaceAvailableLength = 0;
	PADAPT				*adapterList;
	ULONG				count = 0;
	
	//DBGPRINT (("Enter ListAdapters()"));

	IrpStack = IoGetCurrentIrpStackLocation(Irp);

	// attach to userspace buffer file
	userSpaceBuffer = Irp->AssociatedIrp.SystemBuffer;
	
	// populate buffer lengths so we can avoid overfilling the buffer
	userSpaceInputLength = IrpStack->Parameters.DeviceIoControl.InputBufferLength;
	userSpaceOutputLength = IrpStack->Parameters.DeviceIoControl.OutputBufferLength;

	// copy the maximum output length so we can decriment the available amount
	// as we write items to the buffer
	userSpaceAvailableLength = userSpaceOutputLength;

	// we must lock down the adapter list before parsing. this helps us avoid
	// the situation where an adapter is added/removed while we are parsing
	// the list
	
	NdisAcquireSpinLock(&GlobalLock);
	
	// before we write any item, we must first check to see if there is space
	// availabe. if we have run out of space, we return a buffer overflow
	// return status. place 0 into Information to indicate that any information
	// copied should be ignored and considered blank
	
	if (sizeof(UNICODE_NULL) > userSpaceAvailableLength)
	{
		DBGPRINT(("Userspace too small: %i %i",sizeof(UNICODE_NULL),userSpaceAvailableLength));
		status = NDIS_STATUS_BUFFER_OVERFLOW;		
		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = 0;
		NdisReleaseSpinLock(&GlobalLock);	
		IoCompleteRequest(Irp, IO_NO_INCREMENT);
		
		return status;
	}

	// establish list end with a UNICODE NULL

	*((PWCHAR)userSpaceBuffer) = UNICODE_NULL;

	returnSize += sizeof(UNICODE_NULL);
	userSpaceAvailableLength -= sizeof(UNICODE_NULL);

	// attach to the head of the linked list
	adapterList = &pAdaptList;
	
	__try
	{
		while(*adapterList != NULL)
		{
			count += 1;
			
			// see if there is space for the adapter string plus a UNICODE NULL
			if ((*adapterList)->DeviceName.Length + sizeof(UNICODE_NULL) > userSpaceAvailableLength)
			{
				status = NDIS_STATUS_BUFFER_OVERFLOW;
			
				Irp->IoStatus.Status = status;
				Irp->IoStatus.Information = 0;
				
				NdisReleaseSpinLock(&GlobalLock);				
				IoCompleteRequest(Irp, IO_NO_INCREMENT);
				
				return status;
			}
			
			// copy the string contents of the item into the user space buffer
			NdisMoveMemory(
				userSpaceBuffer,
				(*adapterList)->DeviceName.Buffer,
				(*adapterList)->DeviceName.Length
				);

			// decriment the available user space amount by the item length
			userSpaceAvailableLength -= ((*adapterList)->DeviceName.Length + sizeof(UNICODE_NULL));
			returnSize += ((*adapterList)->DeviceName.Length + sizeof(UNICODE_NULL));
				
			// move ahead the user space buffer the length of the item to allow for the
			// insertion of a UNICODE NULL
			userSpaceBuffer += (*adapterList)->DeviceName.Length;

			// insert the string terminating UNICODE NULL
			*((PWCHAR)userSpaceBuffer) = UNICODE_NULL;

			// move head the user space buffer the length of the inserted UNICODE NULL
			userSpaceBuffer += sizeof(UNICODE_NULL);

			// re-add the list terminator
			*((PWCHAR)userSpaceBuffer) = UNICODE_NULL;
			
			// move to the next item in the linked list
			adapterList = &(*adapterList)->Next;

		}
	}
	__except ( EXCEPTION_EXECUTE_HANDLER)
	{
		DBGPRINT (("EXCEPTION HANDLER : Probably accessing user mode memory incorrectly"));
		status = STATUS_SUCCESS;
		
		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = returnSize;
		NdisReleaseSpinLock(&GlobalLock);
		IoCompleteRequest(Irp, IO_NO_INCREMENT);

		return status;
	}

	// release the lock, now that we are done with the adapter list
	
	NdisReleaseSpinLock(&GlobalLock);

	status = STATUS_SUCCESS;
		
	Irp->IoStatus.Status = status;
	Irp->IoStatus.Information = returnSize;
	IoCompleteRequest(Irp, IO_NO_INCREMENT);

	//DBGPRINT (("Exit: ListAdapters()"));	
	return status;
}

NTSTATUS OpenAdapter(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp)
{
	NTSTATUS		status = STATUS_SUCCESS;	

	PIO_STACK_LOCATION	IrpStack = NULL;
	ULONG				returnSize = 0;
	PUCHAR				userSpaceBuffer = NULL;
	ULONG				userSpaceInputLength = 0;
	ULONG				userSpaceOutputLength = 0;
	ULONG				userSpaceAvailableLength = 0;
	PADAPT				*adapterList;
	PADAPT				Adapter = NULL;
	
	DBGPRINT (("OpenAdapter()++"));

	IrpStack = IoGetCurrentIrpStackLocation(Irp);

	// attach to userspace buffer file, user mode application
	// should already populate buffer file with the name of
	// the adapter we want to open
	userSpaceBuffer = Irp->AssociatedIrp.SystemBuffer;
	
	if (userSpaceBuffer == NULL)
	{
		DBGPRINT(("No file handle."));
		// !!!consider changing the return status to indicate
		// !!!the type of failure condition
		status = STATUS_SUCCESS;
			
		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = returnSize;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);

		return status;

	}

	// populate buffer lengths so we can avoid overfilling the buffer
	userSpaceInputLength = IrpStack->Parameters.DeviceIoControl.InputBufferLength;
	userSpaceOutputLength = IrpStack->Parameters.DeviceIoControl.OutputBufferLength;

	// we must lock down the adapter list before parsing. this helps us avoid
	// the situation where an adapter is added/removed while we are parsing
	// the list
	NdisAcquireSpinLock(&GlobalLock);
	
	// before we write any item, we must first check to see if there is space
	// availabe. if we have run out of space, we return a buffer overflow
	// return status. place 0 into Information to indicate that any information
	// copied should be ignored and considered blank

	// attach to the head of the linked list
	adapterList = &pAdaptList;
	
	while(*adapterList != NULL && Adapter == NULL)
	{
		if ((*adapterList)->DeviceName.Length == userSpaceInputLength)
		{
			if (NdisEqualMemory((*adapterList)->DeviceName.Buffer,userSpaceBuffer,userSpaceInputLength))
			{
				DBGPRINT(("Adapter found."));
				// store the pointer to our adapter
				Adapter = (*adapterList);
			}
		}

		// move to the next item in the linked list
		adapterList = &(*adapterList)->Next;

	}

	// release the lock, now that we are done with the adapter list
	NdisReleaseSpinLock(&GlobalLock);

	if (Adapter == NULL)
	{
		DBGPRINT(("No adapter found."));

		// no match has been made with the adapter name
		// !!! check to see if there is a better status
		// !!! to return
		status = STATUS_SUCCESS;
		
		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = returnSize;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);

		return status;
	}

	// check if the adapter is being closed
	NdisAcquireSpinLock(&Adapter->Lock);
	if (Adapter->UnbindingInProcess)
	{
		NdisReleaseSpinLock(&Adapter->Lock);
		DBGPRINT(("Adapter is being closed."));

		// !!! check to see if there is a better status
		// !!! to return
		status = STATUS_SUCCESS;
		
		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = returnSize;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);

		return status;
	}

	if(Adapter->UserModeSettings & USER_MODE_SETTINGS_OPEN_ADAPTER)
	{
		// the adapter is already open, don't open it again
		NdisReleaseSpinLock(&Adapter->Lock);
		DBGPRINT(("Adapter is already open."));

		status = STATUS_SUCCESS;

		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = returnSize;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);

		return status;
	}

	// set the UserModeSettings flag for USER_MODE_SETTINGS_OPEN_ADAPTER
	// when set packets for this adapter will be captured
	Adapter->UserModeSettings |= USER_MODE_SETTINGS_OPEN_ADAPTER;

	NdisReleaseSpinLock(&Adapter->Lock);

	// Attach our context to the desired adapter
	// This esstentially 'opens' the adapter
	IrpStack->FileObject->FsContext = Adapter;

	status = STATUS_SUCCESS;
		
	Irp->IoStatus.Status = status;
	Irp->IoStatus.Information = returnSize;
	IoCompleteRequest(Irp, IO_NO_INCREMENT);

	//DBGPRINT (("OpenAdapter()--"));	
	return status;
}

NTSTATUS CloseAdapter(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp)
{
	NTSTATUS		status = STATUS_SUCCESS;	

	PIO_STACK_LOCATION	IrpStack = NULL;
	ULONG				returnSize = 0;
	PUCHAR				userSpaceBuffer = NULL;
	ULONG				userSpaceInputLength = 0;
	ULONG				userSpaceOutputLength = 0;
	ULONG				userSpaceAvailableLength = 0;
	PADAPT				*adapterList;
	PADAPT				Adapter = NULL;
	
	DBGPRINT (("CloseAdapter()++"));

	IrpStack = IoGetCurrentIrpStackLocation(Irp);

	// attach to userspace buffer file, user mode application
	// should already populate buffer file with the name of
	// the adapter we want to open
	userSpaceBuffer = Irp->AssociatedIrp.SystemBuffer;
	
	if (userSpaceBuffer == NULL)
	{
		DBGPRINT(("No file handle."));
		// !!!consider changing the return status to indicate
		// !!!the type of failure condition
		status = STATUS_SUCCESS;
			
		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = returnSize;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);

		return status;

	}

	// populate buffer lengths so we can avoid overfilling the buffer
	userSpaceInputLength = IrpStack->Parameters.DeviceIoControl.InputBufferLength;
	userSpaceOutputLength = IrpStack->Parameters.DeviceIoControl.OutputBufferLength;

	// we must lock down the adapter list before parsing. this helps us avoid
	// the situation where an adapter is added/removed while we are parsing
	// the list
	NdisAcquireSpinLock(&GlobalLock);
	
	// before we write any item, we must first check to see if there is space
	// availabe. if we have run out of space, we return a buffer overflow
	// return status. place 0 into Information to indicate that any information
	// copied should be ignored and considered blank

	// attach to the head of the linked list
	adapterList = &pAdaptList;
	
	while(*adapterList != NULL && Adapter == NULL)
	{
		if ((*adapterList)->DeviceName.Length == userSpaceInputLength)
		{
			if (NdisEqualMemory((*adapterList)->DeviceName.Buffer,userSpaceBuffer,userSpaceInputLength))
			{
				DBGPRINT(("Adapter found."));
				// store the pointer to our adapter
				Adapter = (*adapterList);
			}
		}

		// move to the next item in the linked list
		adapterList = &(*adapterList)->Next;

	}

	// release the lock, now that we are done with the adapter list
	NdisReleaseSpinLock(&GlobalLock);

	if (Adapter == NULL)
	{
		DBGPRINT(("No adapter found."));

		// no match has been made with the adapter name
		// !!! check to see if there is a better status
		// !!! to return
		status = STATUS_SUCCESS;
		
		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = returnSize;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);

		return status;
	}

	// check if the adapter is being closed
	NdisAcquireSpinLock(&Adapter->Lock);
	if (Adapter->UnbindingInProcess)
	{
		NdisReleaseSpinLock(&Adapter->Lock);
		DBGPRINT(("Adapter is being closed."));

		// !!! check to see if there is a better status
		// !!! to return
		status = STATUS_SUCCESS;
		
		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = returnSize;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);

		return status;
	}

	// unset the UserModeSettings flag for USER_MODE_SETTINGS_OPEN_ADAPTER
	// when not set packets for this adapter will be dropped if the 
	// USER_MODE_SETTINGS_DROP_DEFAULT flag is set
	Adapter->UserModeSettings = Adapter->UserModeSettings | (ULONG)USER_MODE_SETTINGS_OPEN_ADAPTER; // in case close adapter is called before the adapter is opened
	Adapter->UserModeSettings ^= USER_MODE_SETTINGS_OPEN_ADAPTER; // turn off the flag

	NdisReleaseSpinLock(&Adapter->Lock);

	// Remove our context from the adapter
	IrpStack->FileObject->FsContext = NULL;

	status = STATUS_SUCCESS;
		
	Irp->IoStatus.Status = status;
	Irp->IoStatus.Information = returnSize;
	IoCompleteRequest(Irp, IO_NO_INCREMENT);

	//DBGPRINT (("CloseAdapter()--"));	
	return status;
}

NTSTATUS UpdateAdapterSetting(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp)
{
	NTSTATUS		status = STATUS_SUCCESS;	

	PIO_STACK_LOCATION	IrpStack = NULL;
	ULONG				returnSize = 0;
	PUCHAR				userSpaceBuffer;
	PUCHAR				pAdapterName = NULL;
	PUCHAR				pSettings = NULL;	
	UINT				openFlag = 0;
	ULONG				userSpaceInputLength = 0;
	ULONG				userSpaceOutputLength = 0;
	ULONG				userSpaceAvailableLength = 0;
	PADAPT				*adapterList;
	PADAPT				Adapter = NULL;
	PUCHAR				temp;
	UINT				i;
	
	DBGPRINT (("SetAdapterSetting()++"));

	IrpStack = IoGetCurrentIrpStackLocation(Irp);

	// attach to userspace buffer file, user mode application
	// should already populate buffer file with the name of
	// the adapter we want to open offset by an unsigned integer
	// containing the flags to set to the UserSpaceSettings
	userSpaceBuffer = Irp->AssociatedIrp.SystemBuffer;
	pSettings = userSpaceBuffer;
	userSpaceBuffer += sizeof(ULONG);		
	pAdapterName = userSpaceBuffer;

	// if either pointer is NULL, then there is a problem with the 
	// data passed by the file handle
	if ((pSettings == NULL) || (pAdapterName == NULL))
	{
		DBGPRINT(("SetAdapterSetting: No file handle."));
		// !!!consider changing the return status to indicate
		// !!!the type of failure condition
		status = STATUS_SUCCESS;
			
		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = returnSize;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);

		return status;
	}

	// populate buffer lengths so we can avoid overfilling the buffer
	userSpaceInputLength = IrpStack->Parameters.DeviceIoControl.InputBufferLength;
	userSpaceOutputLength = IrpStack->Parameters.DeviceIoControl.OutputBufferLength;

	if (userSpaceInputLength < sizeof(UINT))
	{
		DBGPRINT(("SetAdapterSetting: User mode size is invalid"));
		// !!!consider changing the return status to indicate
		// !!!the type of failure condition
		status = STATUS_SUCCESS;
			
		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = returnSize;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);

		return status;
	}


	// we must lock down the adapter list before parsing. this helps us avoid
	// the situation where an adapter is added/removed while we are parsing
	// the list
	NdisAcquireSpinLock(&GlobalLock);
	
	// before we write any item, we must first check to see if there is space
	// availabe. if we have run out of space, we return a buffer overflow
	// return status. place 0 into Information to indicate that any information
	// copied should be ignored and considered blank

	// attach to the head of the linked list
	adapterList = &pAdaptList;
	
	while(*adapterList != NULL && Adapter == NULL)
	{
		if ((*adapterList)->DeviceName.Length == (userSpaceInputLength-sizeof(ULONG)-sizeof(USHORT)))
		{
			if (NdisEqualMemory((*adapterList)->DeviceName.Buffer,pAdapterName,userSpaceInputLength-sizeof(ULONG)-sizeof(USHORT)))
			{
				//DBGPRINT(("SetAdapterSetting: Adapter found."));
				// store the pointer to our adapter
				Adapter = (*adapterList);
			}
		}

		// move to the next item in the linked list
		adapterList = &(*adapterList)->Next;
	}

	// release the lock, now that we are done with the adapter list
	NdisReleaseSpinLock(&GlobalLock);

	if (Adapter == NULL)
	{
		DBGPRINT(("SetAdapterSetting: No matching adapter found."));

		// no match has been made with the adapter name
		// !!! check to see if there is a better status
		// !!! to return
		status = STATUS_SUCCESS;
		
		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = returnSize;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);

		return status;
	}

	// check if the adapter is being closed
	NdisAcquireSpinLock(&Adapter->Lock);
	if (Adapter->UnbindingInProcess)
	{
		NdisReleaseSpinLock(&Adapter->Lock);
		DBGPRINT(("SetAdapterSetting: Adapter is being closed."));

		// !!! check to see if there is a better status
		// !!! to return
		status = STATUS_SUCCESS;
		
		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = returnSize;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);

		return status;
	}

	// store the open flag so we don't overwrite it
	openFlag = Adapter->UserModeSettings | USER_MODE_SETTINGS_OPEN_ADAPTER;
	openFlag |= ~USER_MODE_SETTINGS_OPEN_ADAPTER;
	openFlag ^= ~USER_MODE_SETTINGS_OPEN_ADAPTER;
	// set any flags passed by the user mode
	Adapter->UserModeSettings = *((PUINT)pSettings) | openFlag;

	NdisReleaseSpinLock(&Adapter->Lock);

	// Remove our context from the adapter
	IrpStack->FileObject->FsContext = NULL;

	status = STATUS_SUCCESS;
		
	Irp->IoStatus.Status = status;
	Irp->IoStatus.Information = returnSize;
	IoCompleteRequest(Irp, IO_NO_INCREMENT);

	//DBGPRINT (("SetAdapterSetting()--"));
	return status;
}


NTSTATUS RecvPackets(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp)
{
	NTSTATUS		status = STATUS_SUCCESS;	

	PIO_STACK_LOCATION	IrpStack = NULL;
	ULONG				returnSize = 0;
	ULONG				userSpaceInputLength = 0;
	ULONG				userSpaceOutputLength = 0;
	ULONG				userSpaceAvailableLength = 0;
	PADAPT				*adapterList;
	PADAPT				Adapter = NULL;

	PUINT				pAdapterNameLength = NULL;
	PUCHAR				currentBuffer = NULL;
	PUCHAR				pAdapterName = NULL;
	PUINT				pPacketLength = NULL;
	PUCHAR				pPacketData = NULL;
	UINT				amountRead = 0;

	PNDIS_PACKET        pNdisPacket;
	PNDIS_BUFFER        pNdisBuffer;

	DBGPRINT (("RecvPackets()++"));

	IrpStack = IoGetCurrentIrpStackLocation(Irp);

	// attach to userspace buffer file, user mode application
	// should already populate buffer file with the length of the
	// adapter name as a uint, followed by the adapter name, without
	// a UNICODE null. this is followed by a byte+uint pair for the
	// length of the buffer
	//
	// [adapterNameLength][adapterName][action1][packetID1]..[actionN][packetIDN]

	currentBuffer = Irp->AssociatedIrp.SystemBuffer;

	// if systemBuffer is NULL, then there is a problem with the 
	// data passed by the file handle
	if (currentBuffer == NULL)
	{
		DBGPRINT(("RecvPackets: No file handle."));
		// !!!consider changing the return status to indicate
		// !!!the type of failure condition
		status = STATUS_SUCCESS;
			
		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = returnSize;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);

		return status;
	}

	// acquire adapter name length
	pAdapterNameLength = (PUINT)currentBuffer;	
	currentBuffer += sizeof(UINT);
	amountRead += sizeof(UINT);

	// preform valid buffer length check
	if ((userSpaceInputLength - (*pAdapterNameLength + sizeof(UINT))) % 
		(sizeof(UCHAR) + sizeof(UINT)) != 0)
	{
		DBGPRINT(("RecvPackets: Invalid buffer or adapter name lengths"));
		// !!!consider changing the return status to indicate
		// !!!the type of failure condition
		status = STATUS_SUCCESS;
			
		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = returnSize;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);

		return status;
	}

	// perform valid adapter name length check
	if (userSpaceInputLength - (sizeof(UINT)*2 + sizeof(UCHAR)) < *pAdapterNameLength)
	{
		DBGPRINT(("RecvPackets: Invalid buffer or adapter name lengths"));
		// !!!consider changing the return status to indicate
		// !!!the type of failure condition
		status = STATUS_SUCCESS;
			
		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = returnSize;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);

		return status;
	}

	// populate buffer lengths so we can avoid overfilling the buffer
	userSpaceInputLength = IrpStack->Parameters.DeviceIoControl.InputBufferLength;
	userSpaceOutputLength = IrpStack->Parameters.DeviceIoControl.OutputBufferLength;

	// point to the adapter name
	pAdapterName = currentBuffer;
	currentBuffer += *pAdapterNameLength;
	amountRead += *pAdapterNameLength;

	// we must lock down the adapter list before parsing. this helps us avoid
	// the situation where an adapter is added/removed while we are parsing
	// the list
	NdisAcquireSpinLock(&GlobalLock);
	
	// before we write any item, we must first check to see if there is space
	// availabe. if we have run out of space, we return a buffer overflow
	// return status. place 0 into Information to indicate that any information
	// copied should be ignored and considered blank

	// attach to the head of the linked list
	adapterList = &pAdaptList;
	
	while(*adapterList != NULL && Adapter == NULL)
	{
		if ((*adapterList)->DeviceName.Length == *pAdapterNameLength)
		{
			if (NdisEqualMemory((*adapterList)->DeviceName.Buffer,pAdapterName,*pAdapterNameLength))
			{
				//DBGPRINT(("RecvPackets: Adapter found."));
				// store the pointer to our adapter
				Adapter = (*adapterList);
			}
		}

		// move to the next item in the linked list
		adapterList = &(*adapterList)->Next;
	}

	// release the lock, now that we are done with the adapter list
	NdisReleaseSpinLock(&GlobalLock);

	if (Adapter == NULL)
	{
		DBGPRINT(("RecvPackets: No matching adapter found."));

		// no match has been made with the adapter name
		// !!! check to see if there is a better status
		// !!! to return
		status = STATUS_SUCCESS;
		
		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = returnSize;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);

		return status;
	}

	// check if the adapter is being closed
	NdisAcquireSpinLock(&Adapter->Lock);
	if (Adapter->UnbindingInProcess)
	{
		NdisReleaseSpinLock(&Adapter->Lock);
		DBGPRINT(("RecvPackets: Adapter is being closed."));

		// !!! check to see if there is a better status
		// !!! to return
		status = STATUS_SUCCESS;
		
		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = returnSize;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);

		return status;
	}

	NdisReleaseSpinLock(&Adapter->Lock);

	// lock down the adapter while we add the packets
	NdisAcquireSpinLock(&Adapter->FilterLock);

	pPacketLength = (PUINT)currentBuffer;
	currentBuffer += sizeof(UINT);
	amountRead +=sizeof(UINT);

	while((*pPacketLength != 0) && (*pPacketLength < (userSpaceInputLength - amountRead)))
	{		

		pPacketData = (PUCHAR)currentBuffer;
		

        NdisAllocatePacket(
            &status,
            &pNdisPacket,
            Adapter->RecvPacketPoolHandle);		

        if (status != NDIS_STATUS_SUCCESS)
        {
			DBGPRINT(("RecvPackets: Could not allocate packet"));
			// error handling code here
			status = STATUS_SUCCESS;
			
			Irp->IoStatus.Status = status;
			Irp->IoStatus.Information = returnSize;
			IoCompleteRequest(Irp, IO_NO_INCREMENT);

			return status;
		}

		// copy the packet data to newly allocated memory
		NdisAllocateMemoryWithTag(&pPacketData,*pPacketLength,TAG);
		NdisMoveMemory(pPacketData,currentBuffer,*pPacketLength);

        NdisAllocateBuffer(
            &status,
            &pNdisBuffer,
            Adapter->FilterRecvBufferPool,
            pPacketData,
            *pPacketLength);

        if (status != NDIS_STATUS_SUCCESS)
        {
			DBGPRINT(("RecvPackets: Could not allocate buffer"));
			// error handling code here
			status = STATUS_SUCCESS;
			
			Irp->IoStatus.Status = status;
			Irp->IoStatus.Information = returnSize;
			IoCompleteRequest(Irp, IO_NO_INCREMENT);

			return status;
		}

		NDIS_SET_PACKET_STATUS(pNdisPacket, NDIS_STATUS_RESOURCES);
        pNdisBuffer->Next = NULL;
        NdisChainBufferAtFront(pNdisPacket, pNdisBuffer);

		// recv the allocated packet
        PtQueueReceivedPacket(Adapter, pNdisPacket, TRUE);

		// free the allocated buffer
		NdisFreeBuffer(pNdisBuffer);
		// free the allocated packet
		NdisFreePacket(pNdisPacket);
		// free the allocated memory that holds
		// the packet data
		NdisFreeMemory(pPacketData,*pPacketLength,0);

		// packet handled, move to the next packet
		currentBuffer += *pPacketLength;
		pPacketLength = (PUINT)currentBuffer;
		currentBuffer += sizeof(UINT);
	}
	// free adapter lock
	NdisReleaseSpinLock(&Adapter->FilterLock);
	// Remove our context from the adapter
	IrpStack->FileObject->FsContext = NULL;
	status = STATUS_SUCCESS;
	Irp->IoStatus.Status = status;
	Irp->IoStatus.Information = returnSize;
	IoCompleteRequest(Irp, IO_NO_INCREMENT);

	//DBGPRINT (("RecvPackets()--"));
	return status;
}


NTSTATUS SetPackets(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp)
{
	NTSTATUS		status = STATUS_SUCCESS;	

	PIO_STACK_LOCATION	IrpStack = NULL;
	ULONG				returnSize = 0;
	ULONG				userSpaceInputLength = 0;
	ULONG				userSpaceOutputLength = 0;
	ULONG				userSpaceAvailableLength = 0;
	PADAPT				*adapterList;
	PADAPT				Adapter = NULL;

	PUINT				pAdapterNameLength = NULL;
	PUCHAR				currentBuffer = NULL;
	PUCHAR				pAdapterName = NULL;
	PUCHAR				pAction = NULL;
	PUINT				pPacketID = NULL;
	
	DBGPRINT (("SetPackets()++"));

	IrpStack = IoGetCurrentIrpStackLocation(Irp);

	// attach to userspace buffer file, user mode application
	// should already populate buffer file with the length of the
	// adapter name as a uint, followed by the adapter name, without
	// a UNICODE null. this is followed by a byte+uint pair for the
	// length of the buffer
	//
	// [adapterNameLength][adapterName][action1][packetID1]..[actionN][packetIDN]

	currentBuffer = Irp->AssociatedIrp.SystemBuffer;

	// if systemBuffer is NULL, then there is a problem with the 
	// data passed by the file handle
	if (currentBuffer == NULL)
	{
		DBGPRINT(("SetPackets :No file handle."));
		// !!!consider changing the return status to indicate
		// !!!the type of failure condition
		status = STATUS_SUCCESS;
			
		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = returnSize;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);

		return status;
	}

	// acquire adapter name length
	pAdapterNameLength = (PUINT)currentBuffer;	
	currentBuffer += sizeof(UINT);

	// preform valid buffer length check
	if ((userSpaceInputLength - (*pAdapterNameLength + sizeof(UINT))) % 
		(sizeof(UCHAR) + sizeof(UINT)) != 0)
	{
		DBGPRINT(("SetPackets :Invalid buffer or adapter name lengths"));
		// !!!consider changing the return status to indicate
		// !!!the type of failure condition
		status = STATUS_SUCCESS;
			
		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = returnSize;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);

		return status;
	}

	// perform valid adapter name length check
	if (userSpaceInputLength - (sizeof(UINT)*2 + sizeof(UCHAR)) < *pAdapterNameLength)
	{
		DBGPRINT(("SetPackets :Invalid buffer or adapter name lengths"));
		// !!!consider changing the return status to indicate
		// !!!the type of failure condition
		status = STATUS_SUCCESS;
			
		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = returnSize;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);

		return status;
	}

	// populate buffer lengths so we can avoid overfilling the buffer
	userSpaceInputLength = IrpStack->Parameters.DeviceIoControl.InputBufferLength;
	userSpaceOutputLength = IrpStack->Parameters.DeviceIoControl.OutputBufferLength;

	// point to the adapter name
	pAdapterName = currentBuffer;
	currentBuffer += *pAdapterNameLength;

	// we must lock down the adapter list before parsing. this helps us avoid
	// the situation where an adapter is added/removed while we are parsing
	// the list
	NdisAcquireSpinLock(&GlobalLock);
	
	// before we write any item, we must first check to see if there is space
	// availabe. if we have run out of space, we return a buffer overflow
	// return status. place 0 into Information to indicate that any information
	// copied should be ignored and considered blank

	// attach to the head of the linked list
	adapterList = &pAdaptList;
	
	while(*adapterList != NULL && Adapter == NULL)
	{
		if ((*adapterList)->DeviceName.Length == *pAdapterNameLength)
		{
			if (NdisEqualMemory((*adapterList)->DeviceName.Buffer,pAdapterName,*pAdapterNameLength))
			{
				//DBGPRINT(("Adapter found."));
				// store the pointer to our adapter
				Adapter = (*adapterList);
			}
		}

		// move to the next item in the linked list
		adapterList = &(*adapterList)->Next;
	}

	// release the lock, now that we are done with the adapter list
	NdisReleaseSpinLock(&GlobalLock);

	if (Adapter == NULL)
	{
		DBGPRINT(("SetPackets :No matching adapter found."));

		// no match has been made with the adapter name
		// !!! check to see if there is a better status
		// !!! to return
		status = STATUS_SUCCESS;
		
		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = returnSize;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);

		return status;
	}

	// check if the adapter is being closed
	NdisAcquireSpinLock(&Adapter->Lock);
	if (Adapter->UnbindingInProcess)
	{
		NdisReleaseSpinLock(&Adapter->Lock);
		DBGPRINT(("SetPackets :Adapter is being closed."));

		// !!! check to see if there is a better status
		// !!! to return
		status = STATUS_SUCCESS;
		
		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = returnSize;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);

		return status;
	}

	NdisReleaseSpinLock(&Adapter->Lock);


	// lock down the adapter while we process the packets
	NdisAcquireSpinLock(&Adapter->FilterLock);

	pAction = (PUCHAR)currentBuffer;
	currentBuffer += sizeof(UCHAR);
	pPacketID = (PUINT)currentBuffer;
	currentBuffer += sizeof(UINT);

	while(*pPacketID != 0)
	{
		DBGPRINT(("SetPackets: Action %u PacketID %u",*pAction,*pPacketID));
		if(PACKET_ACTION_OUTBOUND & *pAction)
		{
			//DBGPRINT(("SetPackets: FilterPacketListSend()"));
			// the packet is outbound
			FilterPacketListSend(Adapter,*pPacketID,*pAction);
			// complete all packets that have not been handled
			// we should never get to this point, since all packets
			// should be set when SetPackets() is called. this is 
			// here strictly as a failsafe prevention of infinitely
			// pending packets
		}

		//if(PACKET_ACTION_INBOUND & *pAction)
		//{
		//    FilterPacketListRecv(Adapter,*pPacketID,*pAction);
		//}

		pAction = (PUCHAR)currentBuffer;
		currentBuffer += sizeof(UCHAR);
		pPacketID = (PUINT)currentBuffer;
		currentBuffer += sizeof(UINT);
	}	

	if(Adapter->FilterSendPendingAction != NULL)
	{
		DBGPRINT(("SetPackets: Not all packets handled. Flushing remaining"));
		FilterSendCompleteList(&(Adapter->FilterSendPendingAction));
	}

	// free adapter lock
	NdisReleaseSpinLock(&Adapter->FilterLock);

	// Remove our context from the adapter
	IrpStack->FileObject->FsContext = NULL;

	status = STATUS_SUCCESS;
		
	Irp->IoStatus.Status = status;
	Irp->IoStatus.Information = returnSize;
	IoCompleteRequest(Irp, IO_NO_INCREMENT);

	//DBGPRINT (("SetPackets()--"));
	return status;
}


NTSTATUS GetPackets(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp)
{
	NTSTATUS		status = STATUS_SUCCESS;	

	PIO_STACK_LOCATION			IrpStack = NULL;
	ULONG						returnSize = 0;
	PUCHAR						userSpaceBuffer = NULL;
	ULONG						userSpaceInputLength = 0;
	ULONG						userSpaceOutputLength = 0;
	ULONG						userSpaceAvailableLength = 0;
	PUCHAR						pAdapterName = NULL;
	PADAPT						*adapterList;
	PADAPT						Adapter = NULL;
	PNDIS_PACKET_LIST_ELEMENT	packetList;
	PNDIS_PACKET				currentPacket;
	ULONG						packetLength;
	ULONG						count = 0;

	// temporary variables to hold the data we write
	// to the user mode buffer
	UINT						packetID;
	UCHAR						ipProtocol;
	ULONG						sourceIP;
	ULONG						destinationIP;
	USHORT						sourcePort;
	USHORT						destinationPort;	
	UINT						contentSize;
	PUCHAR						content;

	DBGPRINT (("GetPackets()++"));

	IrpStack = IoGetCurrentIrpStackLocation(Irp);

	// attach to userspace buffer file, user mode application
	// should already populate buffer file with the name of
	// the adapter we want to open 
	pAdapterName = Irp->AssociatedIrp.SystemBuffer;
	
	// if either pointer is NULL, then there is a problem with the 
	// data passed by the file handle
	if (pAdapterName == NULL)
	{
		DBGPRINT(("GetPackets :No file handle."));
		// !!!consider changing the return status to indicate
		// !!!the type of failure condition
		status = STATUS_SUCCESS;
			
		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = 0;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);

		return status;
	}

	// populate buffer lengths so we can avoid overfilling the buffer
	userSpaceInputLength = IrpStack->Parameters.DeviceIoControl.InputBufferLength;
	userSpaceOutputLength = IrpStack->Parameters.DeviceIoControl.OutputBufferLength;

	
	userSpaceAvailableLength = userSpaceOutputLength;	
	if (sizeof(UINT) > userSpaceAvailableLength)
	{
		DBGPRINT(("GetPackets :User mode size is invalid"));
		// !!!consider changing the return status to indicate
		// !!!the type of failure condition
		status = STATUS_SUCCESS;
			
		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = 0;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);

		return status;
	}

	// we must lock down the adapter list before parsing. this helps us avoid
	// the situation where an adapter is added/removed while we are parsing
	// the list
	NdisAcquireSpinLock(&GlobalLock);
	// before we write any item, we must first check to see if there is space
	// availabe. if we have run out of space, we return a buffer overflow
	// return status. place 0 into Information to indicate that any information
	// copied should be ignored and considered blank

	// attach to the head of the linked list
	adapterList = &pAdaptList;
	
	while(*adapterList != NULL && Adapter == NULL)
	{
		if ((*adapterList)->DeviceName.Length == (userSpaceInputLength))
		{
			if (NdisEqualMemory((*adapterList)->DeviceName.Buffer,pAdapterName,userSpaceInputLength))
			{
				//DBGPRINT(("GetPackets: Adapter found."));
				// store the pointer to our adapter
				Adapter = (*adapterList);
			}
		}

		// move to the next item in the linked list
		adapterList = &(*adapterList)->Next;
	}

	// release the lock, now that we are done with the adapter list
	NdisReleaseSpinLock(&GlobalLock);
	if (Adapter == NULL || Adapter->FilterSendList == NULL)
	{
		if (Adapter == NULL)
		{
			DBGPRINT(("GetPackets: No matching adapter found"));
		}

		if (Adapter->FilterSendList == NULL)
		{
			DBGPRINT(("GetPackets: No packets to handle"));
		}

		// no match has been made with the adapter name
		// or there are no packets to handle
		status = STATUS_SUCCESS;
		
		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = 0;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);

		return status;
	}
	// check if the adapter is being closed
	NdisAcquireSpinLock(&Adapter->Lock);
	if (Adapter->UnbindingInProcess)
	{
		NdisReleaseSpinLock(&Adapter->Lock);
		DBGPRINT(("GetPackets :Adapter is being closed."));

		// !!! check to see if there is a better status
		// !!! to return
		status = STATUS_SUCCESS;
		
		Irp->IoStatus.Status = status;
		Irp->IoStatus.Information = 0;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);

		return status;
	}

	// free adapter lock
	NdisReleaseSpinLock(&Adapter->Lock);


	// lockdown the adapter while we pull the packets
	NdisAcquireSpinLock(&Adapter->FilterLock);

	// fill user space buffer with the stored packets 
	// in FilterSendList once we pass the
	// packet contents, we move the packet to the
	// FilterSendHandledList
	packetList = Adapter->FilterSendList;
	packetID = (UINT)packetList->Packet;
	// attach to userspace buffer file
	userSpaceBuffer = Irp->AssociatedIrp.SystemBuffer;

	while (packetList != NULL && packetID != 0)
	{
		currentPacket = packetList->Packet;		
		
		packetLength = FilterGetPacketSize(currentPacket);

		packetID = (UINT)currentPacket;	

		content = NULL;
			
		// check to see if we have enough free userSpaceBuffer
		// to write the packets contents plus the ID(uint)
		// packet length and the list terminator (uint)
		if(userSpaceAvailableLength < packetLength + 12)
		{
			// not enough space, flagging packetID as 0 indicates
			// we have run out of space and need to finish writing
			packetID = 0;
		}
		else
		{
			// if the packet is an HTTP packet with content beyond the
			// ETHERNET/IP/TCP headers, copy the ETHERNET,IP,TCP Headers plus
			// the additional content, otherwise copy the ETHERNET,IP header
			// plus the first 8 bytes from the datagram
			if((FilterGetIPProtocol(currentPacket) == IP_TCP_PROTOCOL) && 
			   (FilterGetDestinationPort(currentPacket) == FILTER_HTTP_PORT)&&
			   (packetLength > sizeof(ETHERNET_HEADER) + sizeof(IP_HEADER) + sizeof(TCP_HEADER)))
			{
				// copy the complete IP/TCP headers and contents
				contentSize = packetLength;
			}
			else
			{
				// capture the IP Header + 64 bits, as required for ICMP returns
				contentSize = sizeof(ETHERNET_HEADER) + sizeof(IP_HEADER) + 8;
			}


			// there is enough available space, write the packet contents
			// allocate space for content
			if (NdisAllocateMemoryWithTag(&content,contentSize,TAG) == STATUS_SUCCESS)
			{						
				// attempt to get the contents
				if (!FilterGetPacketValue(currentPacket,(PVOID)content,0,contentSize))
				{
					DBGPRINT(("GetPackets: Failed to get content"));
					// unable to retreive the contents as expected

					// free the allocated resources
					NdisFreeMemory(content,packetLength,0);
					// we failed to retreive the expected contents
					content = NULL;
				}			
			}
			else
			{
				DBGPRINT(("GetPackets: Could not allocate resources for content"));
				content = NULL;
				packetID = 0;
			}

			if(content != NULL)
			{
				// write packet ID		
				*((PUINT)userSpaceBuffer) = packetID;			
				userSpaceBuffer += sizeof(UINT);

				// write packet Length
				*((PUINT)userSpaceBuffer) = contentSize;
				userSpaceBuffer += sizeof(UINT);

				// write the content to the user space buffer
				NdisMoveMemory(	userSpaceBuffer,
								content,
								contentSize);					

				// free the resources allocated
				NdisFreeMemory(content,contentSize,0);
				userSpaceBuffer += contentSize;				

				// reduce our available length the amount we wrote
				// to the user space buffer
				userSpaceAvailableLength -= 8 + contentSize;

				// increase our return amount by the amount we wrote
				// to the user space buffer
				returnSize += 8 + contentSize;
			}
		}

		// move the packets to the pending action list
		if(packetID != 0)
		{
			FilterMoveElement(&Adapter->FilterSendList,&currentPacket,&Adapter->FilterSendPendingAction);
			packetList = Adapter->FilterSendList;
		}
	}

	// free adapter lock
	NdisReleaseSpinLock(&Adapter->FilterLock);

	// write packet ID (0) to the user space buffer, this indicates the end
	// of the packet list
	packetID = 0;
	*((PUINT)userSpaceBuffer) = packetID;
	userSpaceBuffer += sizeof(UINT);
	returnSize += sizeof(UINT);

	// Remove our context from the adapter
	IrpStack->FileObject->FsContext = NULL;

	status = STATUS_SUCCESS;
		
	Irp->IoStatus.Status = status;
	Irp->IoStatus.Information = returnSize;
	IoCompleteRequest(Irp, IO_NO_INCREMENT);

	//DBGPRINT (("GetPackets()--"));	
	return status;
}


NTSTATUS HookCreate(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp)
{
	PIO_STACK_LOCATION  irpStack;
	NTSTATUS			status = STATUS_SUCCESS;

	DBGPRINT(("HookCreate()++"));

	irpStack = IoGetCurrentIrpStackLocation(Irp);

	Irp->IoStatus.Status = status;
	IoCompleteRequest(Irp, IO_NO_INCREMENT);

	//DBGPRINT(("HookCreate()--"));

	return status;
}

NTSTATUS HookCleanup(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp)
{
	PIO_STACK_LOCATION  irpStack;
	NTSTATUS			status = STATUS_SUCCESS;

	DBGPRINT(("HookCleanup()++"));

	irpStack = IoGetCurrentIrpStackLocation(Irp);

	Irp->IoStatus.Status = status;
	IoCompleteRequest(Irp, IO_NO_INCREMENT);

	//DBGPRINT(("HookCleanup()--"));

	return status;	
}

NTSTATUS HookClose(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp)
{
	PIO_STACK_LOCATION  irpStack;
	NTSTATUS			status = STATUS_SUCCESS;

	DBGPRINT(("HookClose()++"));

	irpStack = IoGetCurrentIrpStackLocation(Irp);

	Irp->IoStatus.Status = status;
	IoCompleteRequest(Irp, IO_NO_INCREMENT);

	//DBGPRINT(("HookClose()--"));

	return status;	
}

NTSTATUS SendPacket(IN PDEVICE_OBJECT pDeviceObject, IN PIRP pIrp)
/*++

Routine Description:

    Dispatch routine to handle IRP_MJ_WRITE. 

Arguments:

    pDeviceObject - pointer to our device object
    pIrp - Pointer to request packet

Return Value:

    NT status code.

--*/
{
    PIO_STACK_LOCATION      IrpStack;
    ULONG                   DataLength;
    NTSTATUS                NtStatus;
    NDIS_STATUS             Status;
    //PNDISPROT_OPEN_CONTEXT   pOpenContext;
	PADAPT					pOpenContext;
    PNDIS_PACKET            pNdisPacket;
    PNDIS_BUFFER            pNdisBuffer;
    ETHERNET_HEADER UNALIGNED *pEthHeader;
#ifdef NDIS51
    PVOID                   CancelId;
#endif

    UNREFERENCED_PARAMETER(pDeviceObject);

    IrpStack = IoGetCurrentIrpStackLocation(pIrp);
    pOpenContext = IrpStack->FileObject->FsContext;

    pNdisPacket = NULL;

    do
    {
        if (pOpenContext == NULL)
        {
            DBGPRINT(("Write: FileObject %p not yet associated with a device\n",
                IrpStack->FileObject));
            NtStatus = STATUS_INVALID_HANDLE;
            break;
        }
          
		// Not sure if this is needed?
        //NPROT_STRUCT_ASSERT(pOpenContext, oc);

        if (pIrp->MdlAddress == NULL)
        {
            DBGPRINT(("Write: NULL MDL address on IRP %p\n", pIrp));
            NtStatus = STATUS_INVALID_PARAMETER;
            break;
        }
        //
        // Try to get a virtual address for the MDL.
        //
#ifndef WIN9X
        pEthHeader = MmGetSystemAddressForMdlSafe(pIrp->MdlAddress, NormalPagePriority);

        if (pEthHeader == NULL)
        {
            DBGPRINT(("Write: MmGetSystemAddr failed for"
                    " IRP %p, MDL %p\n",
                    pIrp, pIrp->MdlAddress));
            NtStatus = STATUS_INSUFFICIENT_RESOURCES;
            break;
        }
#else
        pEthHeader = MmGetSystemAddressForMdl(pIrp->MdlAddress);   // for Win9X
#endif

        //
        // Sanity-check the length.
        //
        DataLength = MmGetMdlByteCount(pIrp->MdlAddress);
        if (DataLength < sizeof(ETHERNET_HEADER))
        {
            DBGPRINT(("Write: too small to be a valid packet (%d bytes)\n",
                DataLength));
            NtStatus = STATUS_BUFFER_TOO_SMALL;
            break;
        }

        if (DataLength > (pOpenContext->MaxFrameSize + sizeof(ETHERNET_HEADER)))
        {
            DBGPRINT(("Write: Open %p: data length (%d)"
                    " larger than max frame size (%d)\n",
                    pOpenContext, DataLength, pOpenContext->MaxFrameSize));

            NtStatus = STATUS_INVALID_BUFFER_SIZE;
            break;
        }
		/* Allow spoofed address
        //
        // To prevent applications from sending packets with spoofed
        // mac address, we will do the following check to make sure the source 
        // address in the packet is same as the current MAC address of the NIC.
        //
        if ((pIrp->RequestorMode == UserMode) && 
            !NPROT_MEM_CMP(pEthHeader->SrcAddr, pOpenContext->CurrentAddress, NPROT_MAC_ADDR_LEN))
        {
            DEBUGP(DL_WARN, ("Write: Failing with invalid Source address"));
            NtStatus = STATUS_INVALID_PARAMETER;
            break;
        }
		*/
                
        NdisAcquireSpinLock(&pOpenContext->Lock);

	// ?? Do we need to check for this condition?
        //if (!NPROT_TEST_FLAGS(pOpenContext->Flags, NUIOO_BIND_FLAGS, NUIOO_BIND_ACTIVE))
        //{
        //    NdisReleaseSpinLock(&pOpenContext->Lock);

        //    DBGPRINT(("Write: Open %p is not bound"
        //    " or in low power state\n", pOpenContext));

        //    NtStatus = STATUS_INVALID_HANDLE;
        //    break;
        //}

        //
        //  Allocate a send packet.
        //
        ASSERT(pOpenContext->SendPacketPoolHandle != NULL);
        NdisAllocatePacket(
            &Status,
            &pNdisPacket,
            pOpenContext->SendPacketPoolHandle);
        
        if (Status != NDIS_STATUS_SUCCESS)
        {
            NdisReleaseSpinLock(&pOpenContext->Lock);

            DBGPRINT(("Write: open %p, failed to alloc send pkt\n",
                    pOpenContext));
            NtStatus = STATUS_INSUFFICIENT_RESOURCES;
            break;
        }

        //
        //  Allocate a send buffer if necessary.
        //
        
		//  switch from a variable flag to a compiler conditional statement for
		//  Windows9X checking
#ifdef WIN9X	
		//if (pOpenContext->bRunningOnWin9x)
        //{
            //NdisAllocateBuffer(
            //    &Status,
            //    &pNdisBuffer,
            //    pOpenContext->SendBufferPool,
            //    pEthHeader,
            //    DataLength);
	
			NdisAllocateBuffer(
				&Status,
				&pNdisBuffer,
				pOpenContext->FilterSendBufferPool,
				pEthHeader,
				DataLength);

            if (Status != NDIS_STATUS_SUCCESS)
            {
                NdisReleaseSpinLock(&pOpenContext->Lock);

                NdisFreePacket(pNdisPacket);

                DBGPRINT(("Write: open %p, failed to alloc send buf\n",
                        pOpenContext));
                NtStatus = STATUS_INSUFFICIENT_RESOURCES;
                break;
            }
        //}
        //else
#endif // WIN9X
        //{
#ifndef WIN9X
  			pNdisBuffer = pIrp->MdlAddress;
#endif // WIN(X
//        }

        //NdisInterlockedIncrement((PLONG)&pOpenContext->PendedSendCount);
		NdisInterlockedIncrement((PULONG)&pOpenContext->OutstandingSends);			

        // This increases the reference count on the adapter
		// this might need to be increased in order to prevent the adapter
		// from shutting down before sending the packets, TJ
		//NPROT_REF_OPEN(pOpenContext);  // pended send

        IoMarkIrpPending(pIrp);

        //
        //  Initialize the packet ref count. This packet will be freed
        //  when this count goes to zero.
        //
		// This variable might only be used by the NDISPROT example and 
		// hopefully isn't used to free any external resources
        //NPROT_SEND_PKT_RSVD(pNdisPacket)->RefCount = 1;
		

#ifdef NDIS51

        //
        //  NDIS 5.1 supports cancelling sends. We set up a cancel ID on
        //  each send packet (which maps to a Write IRP), and save the
        //  packet pointer in the IRP. If the IRP gets cancelled, we use
        //  NdisCancelSendPackets() to cancel the packet.
        //

		// This would only be needed if we were to want to cancel a packet,
		// which I don't see myself doing
        //CancelId = NPROT_GET_NEXT_CANCEL_ID();
        //NDIS_SET_PACKET_CANCEL_ID(pNdisPacket, CancelId);
        
		pIrp->Tail.Overlay.DriverContext[0] = (PVOID)pOpenContext;
        pIrp->Tail.Overlay.DriverContext[1] = (PVOID)pNdisPacket;

		//??
        //NPROT_INSERT_TAIL_LIST(&pOpenContext->PendedWrites, &pIrp->Tail.Overlay.ListEntry);	

		// Not going to support cancelling, so no reason to setup the cancel routine
        //IoSetCancelRoutine(pIrp, NdisProtCancelWrite);

#endif // NDIS51

        NdisReleaseSpinLock(&pOpenContext->Lock);

        //
        //  Set a back pointer from the packet to the IRP.
        //
		// PassThru doesn't use a back pointer on packets
        //NPROT_IRP_FROM_SEND_PKT(pNdisPacket) = pIrp;		

        NtStatus = STATUS_PENDING;

        pNdisBuffer->Next = NULL;
        NdisChainBufferAtFront(pNdisPacket, pNdisBuffer);

#if SEND_DBG
        {
            PUCHAR      pData;

#ifndef WIN9X
            pData = MmGetSystemAddressForMdlSafe(pNdisBuffer, NormalPagePriority);
            NPROT_ASSERT(pEthHeader == pData);
#else
            pData = MmGetSystemAddressForMdl(pNdisBuffer);  // Win9x
#endif

            DBGPRINT(("Write: MDL %p, MdlFlags %x, SystemAddr %p, %d bytes\n",
                    pIrp->MdlAddress, pIrp->MdlAddress->MdlFlags, pData, DataLength));

            DEBUGPDUMP(DL_VERY_LOUD, pData, MIN(DataLength, 48));
        }
#endif // SEND_DBG

        NdisSendPackets(pOpenContext->BindingHandle, &pNdisPacket, 1);

    }
    while (FALSE);

    if (NtStatus != STATUS_PENDING)
    {
        pIrp->IoStatus.Status = NtStatus;
        IoCompleteRequest(pIrp, IO_NO_INCREMENT);
    }

    return (NtStatus);
}

NTSTATUS HookDeviceControl(IN PDEVICE_OBJECT DeviceObject,IN PIRP Irp)
{
	PIO_STACK_LOCATION  irpStack;
	NTSTATUS			status = STATUS_INVALID_DEVICE_REQUEST;
	ULONG				ioControlCode;
	PUCHAR	ioBuffer = NULL;

	//IOCTL_FILTERHOOK_SENDALL_PACKETS TEST
	PADAPT Adapter;
	PNDIS_PACKET_LIST_ELEMENT currentElement;
	BOOLEAN pendingPackets = FALSE;

	//DBGPRINT(("HookDeviceControl()++"));

	irpStack = IoGetCurrentIrpStackLocation(Irp);
	ioControlCode = irpStack->Parameters.DeviceIoControl.IoControlCode;

	//DBGPRINT(("HookDeviceControl: ioControlCode %u",ioControlCode));
	switch (ioControlCode)
	{
		case IOCTL_FILTERHOOK_USERMODE_SHUTDOWN:
			DBGPRINT(("HookDeviceControl :IOCTL UserMode Shutdown"));
			if (FilterPacketReceivedIRP != NULL)
			{
				DBGPRINT(("HookDeviceControl :IOCTL UserMode ShutDown: complete IRP"));
				FilterPacketReceivedIRP->IoStatus.Status = STATUS_SUCCESS;
				IoCompleteRequest(FilterPacketReceivedIRP,IO_NO_INCREMENT);
				FilterPacketReceivedIRP = NULL;	
			}

			Irp->IoStatus.Status = STATUS_SUCCESS;
			IoCompleteRequest(Irp,IO_NO_INCREMENT);
			return STATUS_SUCCESS;
		case IOCTL_FILTERHOOK_PACKET_RECEIVED:
			DBGPRINT(("HookDeviceControl :User mode call to Packet Received IRP queue"));
			if (FilterPacketReceivedIRP == NULL)
			{
				// lock down adapters so we can modify it's contents
				NdisAcquireSpinLock(&GlobalLock);			

				// see if we have packets pending
				Adapter = pAdaptList;

				pendingPackets = FALSE;
				while(Adapter != NULL && !pendingPackets)
				{
					if(Adapter->FilterSendList != NULL && 
					   USER_MODE_SETTINGS_OPEN_ADAPTER & Adapter->UserModeSettings)
					{
						pendingPackets = TRUE;
					}
					
					Adapter = Adapter->Next;				
				}

				// free our adapters lock
				NdisReleaseSpinLock(&GlobalLock);

				if (pendingPackets)
				{
					Irp->IoStatus.Status = STATUS_SUCCESS;
					IoCompleteRequest(Irp,IO_NO_INCREMENT);
					return STATUS_SUCCESS;
				}

				DBGPRINT(("HookDeviceControl: FilterpacketReceivedIRP == NULL"));
				IoMarkIrpPending(Irp);
				FilterPacketReceivedIRP = Irp;
				return STATUS_PENDING;
			}

			DBGPRINT(("HookDeviceControl: FilterpacketReceivedIRP != NULL"));
			Irp->IoStatus.Status = STATUS_SUCCESS;
			IoCompleteRequest(Irp,IO_NO_INCREMENT);
			return STATUS_SUCCESS;

		case IOCTL_FILTERHOOK_LIST_ADAPTERS:
			
			//DBGPRINT(("HookDeviceControl : User mode call to ListAdapters()"));
			return (ListAdapters(DeviceObject,Irp));

		case IOCTL_FILTERHOOK_OPEN_ADAPTER:

			//DBGPRINT(("HookDeviceControl : User mode call to OpenAdapter()"));
			return (OpenAdapter(DeviceObject,Irp));

		case IOCTL_FILTERHOOK_CLOSE_ADAPTER:

			//DBGPRINT(("HookDeviceControl : User mode call to CloseAdapter()"));
			return (CloseAdapter(DeviceObject,Irp));

		case IOCTL_FILTERHOOK_UPDATE_ADAPTER_SETTING:

			//DBGPRINT(("HookDeviceControl : User mode call to SetAdapterSetting()"));
			return (UpdateAdapterSetting(DeviceObject,Irp));

		case IOCTL_FILTERHOOK_GET_PACKETS:

			//DBGPRINT(("HookDeviceControl : User mode call to GetPackets()"));
			return (GetPackets(DeviceObject,Irp));

		case IOCTL_FILTERHOOK_SET_PACKETS:

			//DBGPRINT(("HookDeviceControl : User mode call to SetPackets()"));
			return (SetPackets(DeviceObject,Irp));

		case IOCTL_FILTERHOOK_RECV_PACKETS:

			//DBGPRINT(("HookDeviceControl : User mode call to RecvPackets()"));
			return (RecvPackets(DeviceObject,Irp));

		case IOCTL_FILTERHOOK_SEND_PACKET:

			//DBGPRINT(("HookDeviceControl : User mode call to SendPacket()"));
			return (SendPacket(DeviceObject,Irp));

		default:
			status = STATUS_NOT_SUPPORTED;
			break;
	}

	Irp->IoStatus.Status = status;
	IoCompleteRequest(Irp, IO_NO_INCREMENT);

	DBGPRINT(("HookDeviceControl()--"));

	return status;		
}