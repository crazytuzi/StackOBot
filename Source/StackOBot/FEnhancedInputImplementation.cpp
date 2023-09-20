#include "FEnhancedInputImplementation.h"
#include "EnhancedInputActionDelegateBinding.h"
#include "EnhancedInputComponent.h"
#include "Binding/Class/FClassBuilder.h"
#include "Environment/FCSharpEnvironment.h"
#include "Macro/NamespaceMacro.h"

struct FRegisterEnhancedInput
{
	FRegisterEnhancedInput()
	{
		FClassBuilder(TEXT("EnhancedInput"), NAMESPACE_LIBRARY)
			.Function("GetDynamicBindingObject",
			          FEnhancedInputImplementation::EnhancedInput_GetDynamicBindingObjectImplementation)
			.Function("BindAction",
			          FEnhancedInputImplementation::EnhancedInput_BindActionImplementation)
			.Register();
	}
};

static FRegisterEnhancedInput RegisterEnhancedInput;

void FEnhancedInputImplementation::EnhancedInput_GetDynamicBindingObjectImplementation(
	const FGarbageCollectionHandle InThisClass, const FGarbageCollectionHandle InBindingClass, MonoObject** OutValue)
{
	const auto ThisClass = FCSharpEnvironment::GetEnvironment().GetObject<UBlueprintGeneratedClass>(InThisClass);

	const auto BindingClass = FCSharpEnvironment::GetEnvironment().GetObject<UClass>(InBindingClass);

	if (ThisClass != nullptr && BindingClass != nullptr)
	{
		UObject* DynamicBindingObject = UBlueprintGeneratedClass::GetDynamicBindingObject(ThisClass, BindingClass);

		if (DynamicBindingObject == nullptr)
		{
			DynamicBindingObject = NewObject<UObject>(GetTransientPackage(), BindingClass);

			ThisClass->DynamicBindingObjects.Add(reinterpret_cast<UDynamicBlueprintBinding*>(DynamicBindingObject));
		}

		*OutValue = FCSharpEnvironment::GetEnvironment().Bind(DynamicBindingObject);
	}
}

void FEnhancedInputImplementation::EnhancedInput_BindActionImplementation(
	const FGarbageCollectionHandle InEnhancedInputActionDelegateBinding,
	const FGarbageCollectionHandle InEnhancedInputComponent,
	const FGarbageCollectionHandle InObjectToBindTo)
{
	const auto EnhancedInputActionDelegateBinding = FCSharpEnvironment::GetEnvironment().GetObject<
		UEnhancedInputActionDelegateBinding>(InEnhancedInputActionDelegateBinding);

	const auto EnhancedInputComponent = FCSharpEnvironment::GetEnvironment().GetObject<UEnhancedInputComponent>(
		InEnhancedInputComponent);

	const auto ObjectToBindTo = FCSharpEnvironment::GetEnvironment().GetObject<UObject>(InObjectToBindTo);

	EnhancedInputActionDelegateBinding->BindToInputComponent(EnhancedInputComponent, ObjectToBindTo);
}
