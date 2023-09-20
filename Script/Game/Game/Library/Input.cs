using Script.CoreUObject;
using Script.Engine;

namespace Script.Library
{
    public static class Input
    {
        public static T GetDynamicBindingObject<T>(UClass InThisClass) where T : UObject, IStaticClass
        {
            InputImplementation.Input_GetDynamicBindingObjectImplementation(InThisClass.GetHandle(),
                T.StaticClass().GetHandle(), out var OutValue);

            return OutValue != null ? Unreal.Cast<T>(OutValue) : null;
        }

        public static void BindAction(UInputActionDelegateBinding InInputActionDelegateBinding,
            UInputComponent InInInputComponent, UObject InInObjectToBindTo)
        {
            InputImplementation.Input_BindActionImplementation(InInputActionDelegateBinding.GetHandle(),
                InInInputComponent.GetHandle(), InInObjectToBindTo.GetHandle());
        }

        public static void BindAxis(UInputAxisDelegateBinding InInputAxisDelegateBinding,
            UInputComponent InInInputComponent, UObject InInObjectToBindTo)
        {
            InputImplementation.Input_BindAxisImplementation(InInputAxisDelegateBinding.GetHandle(),
                InInInputComponent.GetHandle(), InInObjectToBindTo.GetHandle());
        }

        public static void BindAxisKey(UInputAxisKeyDelegateBinding InInputAxisKeyDelegateBinding,
            UInputComponent InInInputComponent, UObject InInObjectToBindTo)
        {
            InputImplementation.Input_BindAxisKeyImplementation(InInputAxisKeyDelegateBinding.GetHandle(),
                InInInputComponent.GetHandle(), InInObjectToBindTo.GetHandle());
        }

        public static void BindKey(UInputKeyDelegateBinding InInputKeyDelegateBinding,
            UInputComponent InInInputComponent, UObject InInObjectToBindTo)
        {
            InputImplementation.Input_BindKeyImplementation(InInputKeyDelegateBinding.GetHandle(),
                InInInputComponent.GetHandle(), InInObjectToBindTo.GetHandle());
        }

        public static void BindTouch(UInputTouchDelegateBinding InInputTouchDelegateBinding,
            UInputComponent InInInputComponent, UObject InInObjectToBindTo)
        {
            InputImplementation.Input_BindTouchImplementation(InInputTouchDelegateBinding.GetHandle(),
                InInInputComponent.GetHandle(), InInObjectToBindTo.GetHandle());
        }

        public static void BindVectorAxis(UInputVectorAxisDelegateBinding InInputVectorAxisDelegateBinding,
            UInputComponent InInInputComponent, UObject InInObjectToBindTo)
        {
            InputImplementation.Input_BindVectorAxisImplementation(InInputVectorAxisDelegateBinding.GetHandle(),
                InInInputComponent.GetHandle(), InInObjectToBindTo.GetHandle());
        }
    }
}