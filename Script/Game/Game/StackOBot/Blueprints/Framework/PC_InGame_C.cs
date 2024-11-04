using Script.CoreUObject;
using Script.Engine;
using Script.EnhancedInput;
using Script.Game.StackOBot.Blueprints.Character;
using Script.Game.StackOBot.Blueprints.UI;
using Script.Game.StackOBot.Input;
using Script.Game.StackOBot.UI;
using Script.InputCore;
using Script.StackOBot;
using Script.UMG;

namespace Script.Game.StackOBot.Blueprints.Framework
{
    /*
     * Here the player controler handles the player camera manager, we add the hud and input that's not relavted to the character.
     */
    [Override]
    public partial class PC_InGame_C
    {
        [Override]
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

            InputComponent.BindKey(EKeys.AnyKey, EInputEvent.IE_Pressed, this, AnyKey_Pressed);

            InputComponent.BindKey(EKeys.F9, EInputEvent.IE_Pressed, this, F9_Pressed);

            var EnhancedInputComponent = InputComponent as UEnhancedInputComponent;

            Pause_Triggered =
                EnhancedInputComponent.BindAction<IA_Pause>(ETriggerEvent.Triggered, this, IA_Pause_Triggered);

            MoveForward_Triggered =
                EnhancedInputComponent.BindAction<IA_MoveForward>(ETriggerEvent.Triggered, this,
                    IA_MoveForward_Triggered);

            MoveRight_Triggered =
                EnhancedInputComponent.BindAction<IA_MoveRight>(ETriggerEvent.Triggered, this, IA_MoveRight_Triggered);

            Turn_Triggered =
                EnhancedInputComponent.BindAction<IA_Turn>(ETriggerEvent.Triggered, this, IA_Turn_Triggered);

            LookUp_Triggered =
                EnhancedInputComponent.BindAction<IA_LookUp>(ETriggerEvent.Triggered, this, IA_LookUp_Triggered);

            Jump_Started = EnhancedInputComponent.BindAction<IA_Jump>(ETriggerEvent.Started, this, IA_Jump_Started);

            Jump_Completed =
                EnhancedInputComponent.BindAction<IA_Jump>(ETriggerEvent.Completed, this, IA_Jump_Completed);

            Interact_Completed =
                EnhancedInputComponent.BindAction<IA_Interact>(ETriggerEvent.Completed, this, IA_Interact_Completed);

            UnpossesAndSpawnNew_Completed =
                EnhancedInputComponent.BindAction<IA_UnpossesAndSpawnNew>(ETriggerEvent.Completed, this,
                    IA_UnpossesAndSpawnNew_Completed);
        }

        [Override]
        public override void ReceiveEndPlay(EEndPlayReason EndPlayReason)
        {
            InputComponent.RemoveActionBinding(this, EKeys.AnyKey.KeyName, EInputEvent.IE_Pressed, AnyKey_Pressed);

            InputComponent.RemoveActionBinding(this, EKeys.F9.KeyName, EInputEvent.IE_Pressed, F9_Pressed);

            var EnhancedInputComponent = InputComponent as UEnhancedInputComponent;

            EnhancedInputComponent.RemoveAction(this, Pause_Triggered, IA_Pause_Triggered);

            EnhancedInputComponent.RemoveAction(this, MoveForward_Triggered, IA_MoveForward_Triggered);

            EnhancedInputComponent.RemoveAction(this, MoveRight_Triggered, IA_MoveRight_Triggered);

            EnhancedInputComponent.RemoveAction(this, Turn_Triggered, IA_Turn_Triggered);

            EnhancedInputComponent.RemoveAction(this, LookUp_Triggered, IA_LookUp_Triggered);

            EnhancedInputComponent.RemoveAction(this, Jump_Started, IA_Jump_Started);

            EnhancedInputComponent.RemoveAction(this, Jump_Completed, IA_Jump_Completed);

            EnhancedInputComponent.RemoveAction(this, Interact_Completed, IA_Interact_Completed);

            EnhancedInputComponent.RemoveAction(this, UnpossesAndSpawnNew_Completed, IA_UnpossesAndSpawnNew_Completed);
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
        [Override]
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
        [Override]
        private void IA_Pause_Triggered(FInputActionValue ActionValue = null, float ElapsedTime = 0.0f,
            float TriggeredTime = 0.0f, UInputAction SourceAction = null)
        {
            (GetHUD() as IBPI_HUD_Interface_C)?.SetPaused(!UGameplayStatics.IsGamePaused(this));
        }

        /*
         * F5 only in editor not packaged build as Escape closed play in editor.
         */
        [Override]
        private void F9_Pressed(FKey Key = null)
        {
            if (!UKismetSystemLibrary.IsPackagedForDistribution())
            {
                (GetHUD() as IBPI_HUD_Interface_C)?.SetPaused(!UGameplayStatics.IsGamePaused(this));
            }
        }

        [Override]
        private void IA_MoveForward_Triggered(FInputActionValue ActionValue = null, float ElapsedTime = 0,
            float TriggeredTime = 0, UInputAction SourceAction = null)
        {
            (Pawn as BP_Bot_C)?.IA_MoveForward_Triggered(ActionValue, ElapsedTime, TriggeredTime, SourceAction);
        }

        [Override]
        private void IA_MoveRight_Triggered(FInputActionValue ActionValue = null, float ElapsedTime = 0,
            float TriggeredTime = 0, UInputAction SourceAction = null)
        {
            (Pawn as BP_Bot_C)?.IA_MoveRight_Triggered(ActionValue, ElapsedTime, TriggeredTime, SourceAction);
        }

        [Override]
        private void IA_Turn_Triggered(FInputActionValue ActionValue = null, float ElapsedTime = 0,
            float TriggeredTime = 0, UInputAction SourceAction = null)
        {
            (Pawn as BP_Bot_C)?.IA_Turn_Triggered(ActionValue, ElapsedTime, TriggeredTime, SourceAction);
        }

        [Override]
        private void IA_LookUp_Triggered(FInputActionValue ActionValue = null, float ElapsedTime = 0,
            float TriggeredTime = 0, UInputAction SourceAction = null)
        {
            (Pawn as BP_Bot_C)?.IA_LookUp_Triggered(ActionValue, ElapsedTime, TriggeredTime, SourceAction);
        }

        [Override]
        private void IA_Jump_Started(FInputActionValue ActionValue = null, float ElapsedTime = 0,
            float TriggeredTime = 0, UInputAction SourceAction = null)
        {
            (Pawn as BP_Bot_C)?.IA_Jump_Started(ActionValue, ElapsedTime, TriggeredTime, SourceAction);
        }

        [Override]
        private void IA_Jump_Completed(FInputActionValue ActionValue = null, float ElapsedTime = 0,
            float TriggeredTime = 0, UInputAction SourceAction = null)
        {
            (Pawn as BP_Bot_C)?.IA_Jump_Completed(ActionValue, ElapsedTime, TriggeredTime, SourceAction);
        }

        [Override]
        private void IA_Interact_Completed(FInputActionValue ActionValue = null, float ElapsedTime = 0,
            float TriggeredTime = 0, UInputAction SourceAction = null)
        {
            (Pawn as BP_Bot_C)?.IA_Interact_Completed(ActionValue, ElapsedTime, TriggeredTime, SourceAction);
        }

        [Override]
        private void IA_UnpossesAndSpawnNew_Completed(FInputActionValue ActionValue = null, float ElapsedTime = 0,
            float TriggeredTime = 0, UInputAction SourceAction = null)
        {
            (Pawn as BP_Bot_C)?.IA_UnpossesAndSpawnNew_Completed(ActionValue, ElapsedTime, TriggeredTime, SourceAction);
        }

        public bool IsUsingGamepad;

        private FEnhancedInputActionEventBinding Pause_Triggered;

        private FEnhancedInputActionEventBinding MoveForward_Triggered;

        private FEnhancedInputActionEventBinding MoveRight_Triggered;

        private FEnhancedInputActionEventBinding Turn_Triggered;

        private FEnhancedInputActionEventBinding LookUp_Triggered;

        private FEnhancedInputActionEventBinding Jump_Started;

        private FEnhancedInputActionEventBinding Jump_Completed;

        private FEnhancedInputActionEventBinding Interact_Completed;

        private FEnhancedInputActionEventBinding UnpossesAndSpawnNew_Completed;
    }
}