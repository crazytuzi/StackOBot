#pragma once

#include "GarbageCollection/FGarbageCollectionHandle.h"

class FInputImplementation
{
public:
	static void Input_GetDynamicBindingObjectImplementation(const FGarbageCollectionHandle InThisClass,
	                                                        const FGarbageCollectionHandle InBindingClass,
	                                                        MonoObject** OutValue);

	static void Input_BindActionImplementation(const FGarbageCollectionHandle InInputActionDelegateBinding,
	                                           const FGarbageCollectionHandle InInputComponent,
	                                           const FGarbageCollectionHandle InObjectToBindTo);

	static void Input_BindAxisImplementation(const FGarbageCollectionHandle InInputAxisDelegateBinding,
	                                         const FGarbageCollectionHandle InInputComponent,
	                                         const FGarbageCollectionHandle InObjectToBindTo);

	static void Input_BindAxisKeyImplementation(const FGarbageCollectionHandle InInputAxisKeyDelegateBinding,
	                                            const FGarbageCollectionHandle InInputComponent,
	                                            const FGarbageCollectionHandle InObjectToBindTo);

	static void Input_BindKeyImplementation(const FGarbageCollectionHandle InInputKeyDelegateBinding,
	                                        const FGarbageCollectionHandle InInputComponent,
	                                        const FGarbageCollectionHandle InObjectToBindTo);

	static void Input_BindTouchImplementation(const FGarbageCollectionHandle InInputTouchDelegateBinding,
	                                          const FGarbageCollectionHandle InInputComponent,
	                                          const FGarbageCollectionHandle InObjectToBindTo);

	static void Input_BindVectorAxisImplementation(const FGarbageCollectionHandle InInputVectorAxisDelegateBinding,
	                                               const FGarbageCollectionHandle InInputComponent,
	                                               const FGarbageCollectionHandle InObjectToBindTo);
};
