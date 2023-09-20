using System;
using System.Runtime.CompilerServices;
using Script.Engine;

namespace Script.Library
{
    public static class EnhancedInputImplementation
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void EnhancedInput_GetDynamicBindingObjectImplementation(IntPtr InThisClass,
            IntPtr InBindingClass,
            out UDynamicBlueprintBinding OutValue);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void EnhancedInput_BindActionImplementation(IntPtr InEnhancedInputActionDelegateBinding,
            IntPtr InEnhancedInputComponent, IntPtr InObjectToBindTo);
    }
}