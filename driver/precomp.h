#pragma warning(disable:4214)   // bit field types other than int

#pragma warning(disable:4201)   // nameless struct/union
#pragma warning(disable:4115)   // named type definition in parentheses
#pragma warning(disable:4127)   // conditional expression is constant
#pragma warning(disable:4054)   // cast of function pointer to PVOID
#pragma warning(disable:4244)   // conversion from 'int' to 'BOOLEAN', possible loss of data

#include <ndis.h>
#include "passthru.h"

#include "hook.h"	// custom filter hook - TJones 06.14.05
#include "linklist.h" // custom filter linked list
#include "pheaders.h" // protocol header definitions for ethernet,tcp and ip
#include "filter.h" // custom filter functions