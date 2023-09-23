using System;
using Script.Common;
using Script.Engine;
using Script.EnhancedInput;
using Script.Game.StackOBot.Blueprints.UI;
using Script.Game.StackOBot.UI;
using Script.InputCore;
using Script.Library;
using Script.UMG;

namespace Script.Game.StackOBot.Blueprints.Framework
{
    /*
     * Here the player controler handles the player camera manager, we add the hud and input that's not relavted to the character.
     */
    [IsOverride]
    public partial class PC_InGame_C
    {
        [IsOverride]
        public override void ReceiveBeginPlay()
        {
            (UGameplayStatics.GetGameInstance(this) as IBPI_GameInstance_C)?.InitSaveGame();

            /*
             * The player controller also has a reference to the camera manager.
             * We set the min and max to clamp the pitch
             */
            PlayerCameraManager.ViewPitchMin = -65.0f;

            PlayerCameraManager.ViewPitchMax = 0.0f;

            /*
             * Change input mode and hide mouse cursor
             */
            UWidgetBlueprintLibrary.SetInputMode_GameOnly(this);

            bShowMouseCursor = false;

            /*
             * While the pause menu is spawned and handled by the HUD_InGame this player controler owns the head up display where we show the amount of orbs, when the game is saved and controller hints.
             */
            HeadupDisplay = Unreal.NewObject<BPW_HeadupDisplay_C>(this);

            HeadupDisplay.AddToViewport();

            BindKey(EKeys.AnyKey, EInputEvent.IE_Pressed, AnyKey_Pressed);

            BindAction(Unreal.LoadObject<UInputAction>(this, "/Game/StackOBot/Input/IA_Pause.IA_Pause"),
                ETriggerEvent.Triggered, IA_Pause_Triggered);

            BindKey(EKeys.F9, EInputEvent.IE_Pressed, F9_Pressed);
        }

        /*
         * Proper gamepad and keyboard/mouse support you usually need some C++.
         * This is more a workaround as this is a BP only project.
         * When any key is pressed we check if it is coming from a  gamepad.
         * In this way we can toggle between keyboard/ mouse and gamepad input hints.
         * In this case in the headup widget we show the key or the gamepad button to press.
         * In a bigger project this will not hold up.
         * Especially on startup or reloading the map, you can not easily know what the last input device was.
         * Important to know that this event doesnt consume the input.
         */
        [IsOverride]
        private void AnyKey_Pressed(FKey Key = null)
        {
            var bIsGamepadKey = UKismetInputLibrary.Key_IsGamepadKey(Key);

            HeadupDisplay.BPW_Controls.ToggleControlDisplay(bIsGamepadKey);

            IsUsingGamepad = bIsGamepadKey;
        }

        /*
         * Key Binding (Pause Menu)
         * You will notice we will have input proccessed here in the player controller and in the BP_Bot character.
         * In this projects case that is fine. The player controller handles everything expect the pawn controls.
         * In a project where you might have several different characters you control, you might want to have all input in the controller and and the pawn/character executing them.
         * Also for multiplayer this setup is not sufficient.
         * Think of the controller being the brain and the pawn being the body executing the brains commands.
         * That said we made the conscious decision to keep the input for the character in BP_Bot
         */
        [IsOverride]
        private void IA_Pause_Triggered(FInputActionValue ActionValue = null, Single ElapsedTime = 0.0f,
            Single TriggeredTime = 0.0f, UInputAction SourceAction = null)
        {
            (GetHUD() as IBPI_HUD_Interface_C)?.SetPaused(!UGameplayStatics.IsGamePaused(this));
        }

        /*
         * F5 only in editor not packaged build as Escape closed play in editor.
         */
        [IsOverride]
        private void F9_Pressed(FKey Key = null)
        {
            if (!UKismetSystemLibrary.IsPackagedForDistribution())
            {
                (GetHUD() as IBPI_HUD_Interface_C)?.SetPaused(!UGameplayStatics.IsGamePaused(this));
            }
        }

        public Boolean IsUsingGamepad;
    }
}