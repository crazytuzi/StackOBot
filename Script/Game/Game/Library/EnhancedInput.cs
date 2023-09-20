using Script.CoreUObject;
using Script.EnhancedInput;

namespace Script.Library
{
    public static class EnhancedInput
    {
        public static T GetDynamicBindingObject<T>(UClass InThisClass) where T : UObject, IStaticClass
        {
            EnhancedInputImplementation.EnhancedInput_GetDynamicBindingObjectImplementation(InThisClass.GetHandle(),
                T.StaticClass().GetHandle(), out var OutValue);

            return OutValue != null ? Unreal.Cast<T>(OutValue) : null;
        }

        public static void BindAction(UEnhancedInputActionDelegateBinding InEnhancedInputActionDelegateBinding,
            UEnhancedInputComponent InEnhancedInputComponent, UObject InInObjectToBindTo)
        {
            EnhancedInputImplementation.EnhancedInput_BindActionImplementation(
                InEnhancedInputActionDelegateBinding.GetHandle(),
                InEnhancedInputComponent.GetHandle(),
                InInObjectToBindTo.GetHandle());
        }
    }
}