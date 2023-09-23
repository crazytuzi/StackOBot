#pragma once

#include "GarbageCollection/FGarbageCollectionHandle.h"

class FEnhancedInputImplementation
{
public:
	static void EnhancedInput_GetDynamicBindingObjectImplementation(const FGarbageCollectionHandle InThisClass,
	                                                                const FGarbageCollectionHandle InBindingClass,
	                                                                MonoObject** OutValue);

	static void EnhancedInput_BindActionImplementation(
		const FGarbageCollectionHandle InBlueprintEnhancedInputActionBinding,
		const FGarbageCollectionHandle InEnhancedInputComponent,
		const FGarbageCollectionHandle InObjectToBindTo);
};
