using System;
using Script.CoreUObject;
using Script.Engine;
using Script.Game.StackOBot.UI.LoadingScreen;
using Script.Game.StackOBot.UI.PauseMenu;
using Script.UMG;

namespace Script.Game.StackOBot.Blueprints.Framework
{
    /*
     * We use the HUD to handle the pause menu, loading screen and it starts the music in the game instance.
     */
    [Override]
    public partial class HUD_InGame_C
    {
        [Override]
        public override void ReceiveBeginPlay()
        {
            /*
             * Add Loading Screen Widget
             */
            LoadingWidget = Unreal.NewObject<BPW_LoadingScreen_C>(this);

            LoadingWidget.SetFade();

            LoadingWidget.AddToViewport(900);

            /*
             * Add the Pause Menu widget
             */
            PauseMenuWidget = Unreal.NewObject<BPW_PauseMenu_C>(this);

            PauseMenuWidget.SetVisibility(ESlateVisibility.Collapsed);

            PauseMenuWidget.AddToViewport(100);

            /*
             * Play music via game instance to have it persist between main level and menu level
             */
            (UGameplayStatics.GetGameInstance(this) as IBPI_GameInstance_C)?.PlayMusic(0.5);
        }

        [Override]
        public override void ReceiveEndPlay(EEndPlayReason EndPlayReason)
        {
            if (LoadingWidget != null && LoadingWidget.IsValid())
            {
                LoadingWidget.DoneFadingEvent.RemoveAll(this);
            }
        }

        /*
         * Reset The Level
         */
        [Override]
        public void LoadGame()
        {
            LoadALevel(UGameplayStatics.GetCurrentLevelName(this).ToString());
        }

        /*
         * Load Main Menu Map
         */
        [Override]
        public void LoadMenu()
        {
            LoadALevel(MainMenuLevelName);
        }

        /*
         * Quit (Interface Event, called by Pause Menu Widget)
         */
        [Override]
        public void QuitGame()
        {
            UKismetSystemLibrary.QuitGame(this, GetOwningPlayerController(), EQuitPreference.Quit, false);
        }

        /*
         * Pause and Unpause (Interface Event)
         */
        [Override]
        public void SetPaused(bool Paused = false)
        {
            if (Paused)
            {
                if (PauseMenuWidget != null && PauseMenuWidget.IsValid())
                {
                    PauseMenuWidget.SetVisibility(ESlateVisibility.Visible);

                    UWidgetBlueprintLibrary.SetInputMode_UIOnlyEx(GetOwningPlayerController(), PauseMenuWidget);
                }

                UGameplayStatics.SetGamePaused(this, true);

                GetOwningPlayerController().bShowMouseCursor = true;
            }
            else
            {
                UGameplayStatics.SetGamePaused(this, false);

                GetOwningPlayerController().bShowMouseCursor = false;

                UWidgetBlueprintLibrary.SetInputMode_GameOnly(GetOwningPlayerController());

                if (PauseMenuWidget != null && PauseMenuWidget.IsValid())
                {
                    PauseMenuWidget.SetVisibility(ESlateVisibility.Collapsed);
                }
            }
        }

        /*
         * Interface call from the reset button in pause menu in order to delete the save game data and reload the level.
         */
        [Override]
        public void ResetMap()
        {
            var CurrentLevelName = UGameplayStatics.GetCurrentLevelName(this);

            (UGameplayStatics.GetGameInstance(this) as IBPI_GameInstance_C)?.ResetSaveGame(CurrentLevelName,
                out var Success);

            LoadALevel(CurrentLevelName.ToString());
        }

        /*
         * Load Level Functionality.
         * Fades in the Loading Screen before attepting to load the level.
         */
        private void LoadALevel(FName Level_Name = null)
        {
            if (Level_Name != null)
            {
                if (LoadingWidget != null && LoadingWidget.IsValid())
                {
                    LoadingWidget.SetVisibility(ESlateVisibility.SelfHitTestInvisible);

                    UWidgetBlueprintLibrary.SetInputMode_UIOnlyEx(GetOwningPlayerController(), PauseMenuWidget);

                    LevelName = Level_Name;

                    LoadingWidget.SetFade(1.0f);

                    LoadingWidget.DoneFadingEvent.Add(this, DoneFadingEvent);
                }
            }
            else
            {
                Console.WriteLine("ERROR! No Level Name set!");
            }
        }

        /*
         * Wait till fading is done to load level
         */
        private void DoneFadingEvent()
        {
            UGameplayStatics.OpenLevel(this, LevelName);
        }

        private FName LevelName;
    }
}