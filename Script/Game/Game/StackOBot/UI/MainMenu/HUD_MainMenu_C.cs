using Script.CoreUObject;
using Script.Engine;
using Script.Game.StackOBot.Blueprints.Framework;
using Script.Game.StackOBot.UI.LoadingScreen;
using Script.UMG;

namespace Script.Game.StackOBot.UI.MainMenu
{
    [Override]
    public partial class HUD_MainMenu_C
    {
        [Override]
        public override void ReceiveBeginPlay()
        {
            AddLoadingScreenWidget();

            AddMainMenuWidget();

            PlayMusic();
        }

        [Override]
        public override void ReceiveEndPlay(EEndPlayReason EndPlayReason)
        {
            MyLoadingWidget?.DoneFadingEvent.Clear();
        }

        private void AddLoadingScreenWidget()
        {
            MyLoadingWidget = Unreal.NewObject<BPW_LoadingScreen_C>(this);

            MyLoadingWidget.SetFade();

            MyLoadingWidget.AddToViewport(90000);
        }

        private void AddMainMenuWidget()
        {
            MyMainMenu = Unreal.NewObject<BPW_MainMenu_C>(this);

            MyMainMenu.AddToViewport();

            var PlayerController = GetOwningPlayerController();

            UWidgetBlueprintLibrary.SetInputMode_GameAndUIEx(GetOwningPlayerController(), MyMainMenu,
                EMouseLockMode.DoNotLock, false);

            PlayerController.bShowMouseCursor = true;

            var PlayerPawn = UGameplayStatics.GetPlayerPawn(this, 0);

            PlayerPawn.DisableInput(PlayerController);
        }

        /*
         * Start the music that is handled in the game instance to have a persitent music between level loads.
         */
        private void PlayMusic()
        {
            (UGameplayStatics.GetGameInstance(this) as IBPI_GameInstance_C)?.PlayMusic(0.6);
        }

        /*
         * Quit (Interface Event, called by Main Menu Widget)
         */
        [Override]
        public void QuitGame()
        {
            var PlayerController = GetOwningPlayerController();

            UKismetSystemLibrary.QuitGame(this, PlayerController, EQuitPreference.Quit, false);
        }

        /*
         * Load Game (Interface Event, called by Main Menu Widget)
         */
        [Override]
        public void LoadGame()
        {
            if (!StartLevelName.Equals("None"))
            {
                if (MyLoadingWidget != null)
                {
                    MyLoadingWidget.SetVisibility(ESlateVisibility.Visible);

                    MyLoadingWidget.SetFade(1.0);

                    MyLoadingWidget.DoneFadingEvent.Add(this, LoadingDoneFading);
                }
            }
            else
            {
                UKismetSystemLibrary.PrintString(this,
                    "ERROR: No StartLevelName defined in the HUD class. Please set this to your game's first Level name.");
            }
        }

        private void LoadingDoneFading()
        {
            UGameplayStatics.OpenLevel(this, StartLevelName);
        }
    }
}