using Script.Common;
using Script.Engine;

namespace Script.Game.StackOBot.UI.MainMenu
{
    [IsOverride]
    public partial class BPW_MainMenu_C
    {
        /*
         * Assign an event to the OnClicked event of our custom button.
         * Then we use an interface implemented in our hud to avoid casting.
         * The HUD handles all the logic for the main menu but the same interface is also implemented into the ingame hud that handles the paus menu in the same way.
         */
        [IsOverride]
        public override void Construct()
        {
            UMG_Button_Start.ButtonWidget.OnClicked.Add(this, OnStartButtonClicked);

            UMG_Button_Quit.ButtonWidget.OnClicked.Add(this, OnQuitButtonClicked);
        }

        [IsOverride]
        public override void Destruct()
        {
            UMG_Button_Quit.ButtonWidget.OnClicked.Clear();

            UMG_Button_Start.ButtonWidget.OnClicked.Clear();
        }

        private void OnStartButtonClicked()
        {
            var PlayerController = UGameplayStatics.GetPlayerController(this, 0);

            (PlayerController.GetHUD() as IBPI_HUD_Interface_C)?.LoadGame();
        }

        private void OnQuitButtonClicked()
        {
            var PlayerController = UGameplayStatics.GetPlayerController(this, 0);

            (PlayerController.GetHUD() as IBPI_HUD_Interface_C)?.QuitGame();
        }
    }
}