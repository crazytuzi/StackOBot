using Script.CoreUObject;
using Script.Engine;
using Script.Game.StackOBot.Blueprints.Framework;

namespace Script.Game.StackOBot.Blueprints.UI
{
    /*
     * This widget is used in BP_Button in a widget component and shown when the player is facing the button.
     * Check there for the display logic.
     */
    [Override]
    public partial class BPW_HintText_C
    {
        /*
         * We use a widget switcher to toggle between keyboard input hint and an image indicating the button to press on a gamepad.
         * Detecting what input device is currently used isnt easy be doable in blueprint.
         * This way is a bit hacky.
         * See in PC_InGame for more info.
         * In proper projects you might need a bit C++ for a full gamepad support.
         * Also if you would switch from keyboard to gamepad or vis versa when the character stands in front of a button where this hint is shows, it would not update, as we update it only on construct.
         */
        [Override]
        public override void Construct()
        {
            var PlayerController = UGameplayStatics.GetPlayerController(this, 0);

            var PC_InGame = PlayerController as PC_InGame_C;

            if (PC_InGame != null)
            {
                InputTypeSwitcherUse.SetActiveWidgetIndex(PC_InGame.IsUsingGamepad ? 1 : 0);
            }
        }
    }
}