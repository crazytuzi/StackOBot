using System;
using System.Collections.Generic;
using Script.Engine;
using Script.EnhancedInput;
using Script.Library;
using Script.InputCore;
using Script.Slate;

namespace Script.Game.StackOBot.Blueprints.Character
{
    /*
     * The character has the input implemented directly in the pawn.
     * Normally, this would be done in the player controller, as the controller is the brain and the pawn/character is the body executing commands from the brain.
     * As this is a simple singleplayer game with only one playable character we decided to keep it simple.
     * The values in the movement component have been tweaked and iterated to get the feel we wanted.
     */
    public partial class BP_Bot_C
    {
        void BindAction(string InActionName, EInputEvent InInputEvent, Delegate InDelegate)
        {
            var InputActionDelegateBinding = Input.GetDynamicBindingObject<UInputActionDelegateBinding>(GetClass());

            if (InputActionDelegateBinding != null)
            {
                foreach (var InputActionDelegate in InputActionDelegateBinding.InputActionDelegateBindings)
                {
                    if (InputActionDelegate.FunctionNameToBind.ToString() == InDelegate.Method.Name)
                    {
                        return;
                    }
                }

                var Binding = new FBlueprintInputActionDelegateBinding
                {
                    InputActionName = InActionName,
                    InputKeyEvent = InInputEvent,
                    FunctionNameToBind = InDelegate.Method.Name
                };

                InputActionDelegateBinding.InputActionDelegateBindings.Add(Binding);

                Input.BindAction(InputActionDelegateBinding, InputComponent, this);
            }
        }

        void BindAxis(string InAxisName, Delegate InDelegate)
        {
            var InputAxisDelegateBinding = Input.GetDynamicBindingObject<UInputAxisDelegateBinding>(GetClass());

            if (InputAxisDelegateBinding != null)
            {
                foreach (var InputAxisDelegate in InputAxisDelegateBinding.InputAxisDelegateBindings)
                {
                    if (InputAxisDelegate.FunctionNameToBind.ToString() == InDelegate.Method.Name)
                    {
                        return;
                    }
                }

                var Binding = new FBlueprintInputAxisDelegateBinding
                {
                    InputAxisName = InAxisName,
                    FunctionNameToBind = InDelegate.Method.Name
                };

                InputAxisDelegateBinding.InputAxisDelegateBindings.Add(Binding);

                Input.BindAxis(InputAxisDelegateBinding, InputComponent, this);
            }
        }

        void BindAxisKey(EKeys InKey, Delegate InDelegate)
        {
            var InputAxisKeyDelegateBinding = Input.GetDynamicBindingObject<UInputAxisKeyDelegateBinding>(GetClass());

            if (InputAxisKeyDelegateBinding != null)
            {
                foreach (var InputAxisKeyDelegate in InputAxisKeyDelegateBinding.InputAxisKeyDelegateBindings)
                {
                    if (InputAxisKeyDelegate.FunctionNameToBind.ToString() == InDelegate.Method.Name)
                    {
                        return;
                    }
                }

                var Binding = new FBlueprintInputAxisKeyDelegateBinding
                {
                    AxisKey = new FKey
                    {
                        KeyName = EKeys2Name[InKey]
                    },
                    FunctionNameToBind = InDelegate.Method.Name
                };

                InputAxisKeyDelegateBinding.InputAxisKeyDelegateBindings.Add(Binding);

                Input.BindAxisKey(InputAxisKeyDelegateBinding, InputComponent, this);
            }
        }

        void BindKey(FInputChord InInputChord, EInputEvent InInputEvent, Delegate InDelegate)
        {
            var InputKeyDelegateBinding = Input.GetDynamicBindingObject<UInputKeyDelegateBinding>(GetClass());

            if (InputKeyDelegateBinding != null)
            {
                foreach (var InputKeyDelegate in InputKeyDelegateBinding.InputKeyDelegateBindings)
                {
                    if (InputKeyDelegate.FunctionNameToBind.ToString() == InDelegate.Method.Name)
                    {
                        return;
                    }
                }

                var Binding = new FBlueprintInputKeyDelegateBinding
                {
                    InputChord = InInputChord,
                    InputKeyEvent = InInputEvent,
                    FunctionNameToBind = InDelegate.Method.Name
                };

                InputKeyDelegateBinding.InputKeyDelegateBindings.Add(Binding);

                Input.BindKey(InputKeyDelegateBinding, InputComponent, this);
            }
        }

        void BindKey(EKeys InKey, EInputEvent InInputEvent, Delegate InDelegate)
        {
            BindKey(new FInputChord
                {
                    Key = new FKey
                    {
                        KeyName = EKeys2Name[InKey]
                    },
                    bShift = false,
                    bCtrl = false,
                    bAlt = false,
                    bCmd = false
                },
                InInputEvent,
                InDelegate);
        }

        void BindTouch(EInputEvent InInputEvent, Delegate InDelegate)
        {
            var InputTouchDelegateBinding = Input.GetDynamicBindingObject<UInputTouchDelegateBinding>(GetClass());

            if (InputTouchDelegateBinding != null)
            {
                foreach (var InputTouchDelegate in InputTouchDelegateBinding.InputTouchDelegateBindings)
                {
                    if (InputTouchDelegate.FunctionNameToBind.ToString() == InDelegate.Method.Name)
                    {
                        return;
                    }
                }

                var Binding = new FBlueprintInputTouchDelegateBinding
                {
                    InputKeyEvent = InInputEvent,
                    FunctionNameToBind = InDelegate.Method.Name
                };

                InputTouchDelegateBinding.InputTouchDelegateBindings.Add(Binding);

                Input.BindTouch(InputTouchDelegateBinding, InputComponent, this);
            }
        }

        void BindVectorAxis(EKeys InKey, Delegate InDelegate)
        {
            var InputVectorAxisDelegateBinding =
                Input.GetDynamicBindingObject<UInputVectorAxisDelegateBinding>(GetClass());

            if (InputVectorAxisDelegateBinding != null)
            {
                foreach (var InputAxisKeyDelegate in InputVectorAxisDelegateBinding.InputAxisKeyDelegateBindings)
                {
                    if (InputAxisKeyDelegate.FunctionNameToBind.ToString() == InDelegate.Method.Name)
                    {
                        return;
                    }
                }

                var Binding = new FBlueprintInputAxisKeyDelegateBinding
                {
                    AxisKey = new FKey
                    {
                        KeyName = EKeys2Name[InKey]
                    },
                    FunctionNameToBind = InDelegate.Method.Name
                };

                InputVectorAxisDelegateBinding.InputAxisKeyDelegateBindings.Add(Binding);

                Input.BindVectorAxis(InputVectorAxisDelegateBinding, InputComponent, this);
            }
        }

        void BindAction(UInputAction InInputAction, ETriggerEvent InTriggerEvent, Delegate InDelegate)
        {
            var EnhancedInputActionDelegateBinding =
                Library.EnhancedInput.GetDynamicBindingObject<UEnhancedInputActionDelegateBinding>(GetClass());

            if (EnhancedInputActionDelegateBinding != null)
            {
                foreach (var InputActionDelegate in EnhancedInputActionDelegateBinding.InputActionDelegateBindings)
                {
                    if (InputActionDelegate.FunctionNameToBind.ToString() == InDelegate.Method.Name)
                    {
                        return;
                    }
                }

                var Binding = new FBlueprintEnhancedInputActionBinding
                {
                    InputAction = InInputAction,
                    TriggerEvent = InTriggerEvent,
                    FunctionNameToBind = InDelegate.Method.Name
                };

                EnhancedInputActionDelegateBinding.InputActionDelegateBindings.Add(Binding);

                Library.EnhancedInput.BindAction(Binding, Unreal.Cast<UEnhancedInputComponent>(InputComponent), this);
            }
        }

        // @TODO
        public static readonly Dictionary<EKeys, string> EKeys2Name = new Dictionary<EKeys, string>
        {
            {EKeys.AnyKey, "AnyKey"},
            {EKeys.F9, "F9"}
        };
    }
}