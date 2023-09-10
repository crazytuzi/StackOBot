using Script.Common;
using Script.Engine;

namespace Script.Game.StackOBot.UI.PauseMenu
{
    [IsOverride]
    public partial class BPW_PauseMenu_C
    {
        /*
         * Assign events to our custom button OnClick event.
         * Use an interface implemented in the hud to have the logic in one place.
         * The same interface and setup is used in the main menu hud.
         */
        [IsOverride]
        public override void Construct()
        {
            UMG_Button_MainMenu.ButtonWidget.OnClicked.Add(this, OnMainMenuButtonClicked);

            UMG_Button_Quit.ButtonWidget.OnClicked.Add(this, OnQuitButtonClicked);

            UMG_Button_Resume.ButtonWidget.OnClicked.Add(this, OnResumeButtonClicked);

            UMG_Button_Reset.ButtonWidget.OnClicked.Add(this, OnResetMapClicked);
        }

        [IsOverride]
        public override void Destruct()
        {
            UMG_Button_MainMenu.ButtonWidget.OnClicked.Clear();

            UMG_Button_Quit.ButtonWidget.OnClicked.Clear();

            UMG_Button_Resume.ButtonWidget.OnClicked.Clear();

            UMG_Button_Reset.ButtonWidget.OnClicked.Clear();
        }

        private void OnMainMenuButtonClicked()
        {
            (UGameplayStatics.GetPlayerController(this, 0).GetHUD() as IBPI_HUD_Interface_C)?.LoadMenu();
        }

        private void OnQuitButtonClicked()
        {
            (UGameplayStatics.GetPlayerController(this, 0).GetHUD() as IBPI_HUD_Interface_C)?.QuitGame();
        }

        private void OnResumeButtonClicked()
        {
            (UGameplayStatics.GetPlayerController(this, 0).GetHUD() as IBPI_HUD_Interface_C)?.SetPaused();
        }

        private void OnResetMapClicked()
        {
            (UGameplayStatics.GetPlayerController(this, 0).GetHUD() as IBPI_HUD_Interface_C)?.ResetMap();
        }
    }
}