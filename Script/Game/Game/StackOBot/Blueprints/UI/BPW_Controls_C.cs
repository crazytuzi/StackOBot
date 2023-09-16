using System;
using Script.Common;
using Script.Engine;

namespace Script.Game.StackOBot.Blueprints.UI
{
    [IsOverride]
    public partial class BPW_Controls_C
    {
        /*
         * We use a widget switcher to toggle between keyboard input hint and an image indicating the button to press on a gamepad.
         * Detecting what input device is currently used isnt easy be doable in blueprint.
         * This way is a bit hacky.
         * See in PC_InGame for more info. In proper projects you might need a bit C++ for a full gamepad support.
         */
        [IsOverride]
        public void ToggleControlDisplay(Boolean IsGamepad = false)
        {
            InputTypeSwitcher.SetActiveWidgetIndex(UKismetMathLibrary.SelectInt(1, 0, IsGamepad));
        }
    }
}