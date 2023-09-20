using System;
using System.Runtime.CompilerServices;
using Script.Engine;

namespace Script.Library
{
    public static class InputImplementation
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Input_GetDynamicBindingObjectImplementation(IntPtr InThisClass, IntPtr InBindingClass,
            out UDynamicBlueprintBinding OutValue);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Input_BindActionImplementation(IntPtr InInputActionDelegateBinding,
            IntPtr InInputComponent, IntPtr InObjectToBindTo);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Input_BindAxisImplementation(IntPtr InInputAxisDelegateBinding,
            IntPtr InInputComponent, IntPtr InObjectToBindTo);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Input_BindAxisKeyImplementation(IntPtr InInputAxisKeyDelegateBinding,
            IntPtr InInputComponent, IntPtr InObjectToBindTo);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Input_BindKeyImplementation(IntPtr InInputKeyDelegateBinding, IntPtr InInputComponent,
            IntPtr InObjectToBindTo);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Input_BindTouchImplementation(IntPtr InInputKeyDelegateBinding,
            IntPtr InInputComponent, IntPtr InObjectToBindTo);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Input_BindVectorAxisImplementation(IntPtr InInputVectorAxisDelegateBinding,
            IntPtr InInputComponent, IntPtr InObjectToBindTo);
    }
}