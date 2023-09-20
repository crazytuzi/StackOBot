#include "FInputImplementation.h"
#include "Binding/Class/FClassBuilder.h"
#include "Engine/DynamicBlueprintBinding.h"
#include "Engine/InputActionDelegateBinding.h"
#include "Engine/InputAxisDelegateBinding.h"
#include "Engine/InputAxisKeyDelegateBinding.h"
#include "Engine/InputKeyDelegateBinding.h"
#include "Engine/InputTouchDelegateBinding.h"
#include "Engine/InputVectorAxisDelegateBinding.h"
#include "Environment/FCSharpEnvironment.h"
#include "Macro/NamespaceMacro.h"

struct FRegisterInput
{
	FRegisterInput()
	{
		FClassBuilder(TEXT("Input"), NAMESPACE_LIBRARY)
			.Function("GetDynamicBindingObject", FInputImplementation::Input_GetDynamicBindingObjectImplementation)
			.Function("BindAction", FInputImplementation::Input_BindActionImplementation)
			.Function("BindAxis", FInputImplementation::Input_BindAxisImplementation)
			.Function("BindAxisKey", FInputImplementation::Input_BindAxisKeyImplementation)
			.Function("BindKey", FInputImplementation::Input_BindKeyImplementation)
			.Function("BindTouch", FInputImplementation::Input_BindTouchImplementation)
			.Function("BindVectorAxis", FInputImplementation::Input_BindVectorAxisImplementation)
			.Register();
	}
};

static FRegisterInput RegisterInput;

void FInputImplementation::Input_GetDynamicBindingObjectImplementation(const FGarbageCollectionHandle InThisClass,
                                                                       const FGarbageCollectionHandle InBindingClass,
                                                                       MonoObject** OutValue)
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

void FInputImplementation::Input_BindActionImplementation(const FGarbageCollectionHandle InInputActionDelegateBinding,
                                                          const FGarbageCollectionHandle InInputComponent,
                                                          const FGarbageCollectionHandle InObjectToBindTo)
{
	const auto InputActionDelegateBinding = FCSharpEnvironment::GetEnvironment().GetObject<UInputActionDelegateBinding>(
		InInputActionDelegateBinding);

	const auto InputComponent = FCSharpEnvironment::GetEnvironment().GetObject<UInputComponent>(InInputComponent);

	const auto ObjectToBindTo = FCSharpEnvironment::GetEnvironment().GetObject<UObject>(InObjectToBindTo);

	InputActionDelegateBinding->BindToInputComponent(InputComponent, ObjectToBindTo);
}

void FInputImplementation::Input_BindAxisImplementation(const FGarbageCollectionHandle InInputAxisDelegateBinding,
                                                        const FGarbageCollectionHandle InInputComponent,
                                                        const FGarbageCollectionHandle InObjectToBindTo)
{
	const auto InputAxisDelegateBinding = FCSharpEnvironment::GetEnvironment().GetObject<UInputAxisDelegateBinding>(
		InInputAxisDelegateBinding);

	const auto InputComponent = FCSharpEnvironment::GetEnvironment().GetObject<UInputComponent>(InInputComponent);

	const auto ObjectToBindTo = FCSharpEnvironment::GetEnvironment().GetObject<UObject>(InObjectToBindTo);

	InputAxisDelegateBinding->BindToInputComponent(InputComponent, ObjectToBindTo);
}


void FInputImplementation::Input_BindAxisKeyImplementation(
	const FGarbageCollectionHandle InInputAxisKeyDelegateBinding,
	const FGarbageCollectionHandle InInputComponent,
	const FGarbageCollectionHandle InObjectToBindTo)
{
	const auto InputAxisKeyDelegateBinding = FCSharpEnvironment::GetEnvironment().GetObject<
		UInputAxisKeyDelegateBinding>(
		InInputAxisKeyDelegateBinding);

	const auto InputComponent = FCSharpEnvironment::GetEnvironment().GetObject<UInputComponent>(InInputComponent);

	const auto ObjectToBindTo = FCSharpEnvironment::GetEnvironment().GetObject<UObject>(InObjectToBindTo);

	InputAxisKeyDelegateBinding->BindToInputComponent(InputComponent, ObjectToBindTo);
}

void FInputImplementation::Input_BindKeyImplementation(const FGarbageCollectionHandle InInputKeyDelegateBinding,
                                                       const FGarbageCollectionHandle InInputComponent,
                                                       const FGarbageCollectionHandle InObjectToBindTo)
{
	const auto InputKeyDelegateBinding = FCSharpEnvironment::GetEnvironment().GetObject<UInputKeyDelegateBinding>(
		InInputKeyDelegateBinding);

	const auto InputComponent = FCSharpEnvironment::GetEnvironment().GetObject<UInputComponent>(InInputComponent);

	const auto ObjectToBindTo = FCSharpEnvironment::GetEnvironment().GetObject<UObject>(InObjectToBindTo);

	InputKeyDelegateBinding->BindToInputComponent(InputComponent, ObjectToBindTo);
}

void FInputImplementation::Input_BindTouchImplementation(const FGarbageCollectionHandle InInputTouchDelegateBinding,
                                                         const FGarbageCollectionHandle InInputComponent,
                                                         const FGarbageCollectionHandle InObjectToBindTo)
{
	const auto InputTouchDelegateBinding = FCSharpEnvironment::GetEnvironment().GetObject<UInputTouchDelegateBinding>(
		InInputTouchDelegateBinding);

	const auto InputComponent = FCSharpEnvironment::GetEnvironment().GetObject<UInputComponent>(InInputComponent);

	const auto ObjectToBindTo = FCSharpEnvironment::GetEnvironment().GetObject<UObject>(InObjectToBindTo);

	InputTouchDelegateBinding->BindToInputComponent(InputComponent, ObjectToBindTo);
}

void FInputImplementation::Input_BindVectorAxisImplementation(
	const FGarbageCollectionHandle InInputVectorAxisDelegateBinding, const FGarbageCollectionHandle InInputComponent,
	const FGarbageCollectionHandle InObjectToBindTo)
{
	const auto InputVectorAxisDelegateBinding = FCSharpEnvironment::GetEnvironment().GetObject<
		UInputVectorAxisDelegateBinding>(
		InInputVectorAxisDelegateBinding);

	const auto InputComponent = FCSharpEnvironment::GetEnvironment().GetObject<UInputComponent>(InInputComponent);

	const auto ObjectToBindTo = FCSharpEnvironment::GetEnvironment().GetObject<UObject>(InObjectToBindTo);

	InputVectorAxisDelegateBinding->BindToInputComponent(InputComponent, ObjectToBindTo);
}
