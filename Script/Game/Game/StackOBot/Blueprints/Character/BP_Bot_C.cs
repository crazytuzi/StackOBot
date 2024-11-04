using System.Threading;
using System.Threading.Tasks;
using Script.CoreUObject;
using Script.Engine;
using Script.EnhancedInput;
using Script.Game.StackOBot.Blueprints.Abilities;
using Script.Game.StackOBot.Blueprints.Framework;
using Script.Niagara;

namespace Script.Game.StackOBot.Blueprints.Character
{
    /*
     * The character has the input implemented directly in the pawn.
     * Normally, this would be done in the player controller, as the controller is the brain and the pawn/character is the body executing commands from the brain.
     * As this is a simple singleplayer game with only one playable character we decided to keep it simple.
     * The values in the movement component have been tweaked and iterated to get the feel we wanted.
     */
    [Override]
    public partial class BP_Bot_C
    {
        /*
         * Create dynamic material instances and store the references into an array for changing it later when printing and when the bot goes into inactive state. Also set the main color by index.
         */
        [Override]
        public override void UserConstructionScript()
        {
        }

        /*
         * At begin play we assure the first thrust time is as long as the max time in air
         */
        [Override]
        public override void ReceiveBeginPlay()
        {
            // @TODO
            UserConstructionScript();

            ThrusterTime = ThrusterMaxTime;
        }

        /*
         * At tick, as long as the robot is active the camera and the jetpack are updated.
         * His location is pushed to a Parameter collection which is used to have the grass and bushes react to him.
         * Be aware that doing complex things in tick can cause performance issues.
         */
        [Override]
        public override void ReceiveTick(float DeltaSeconds)
        {
            if (!IsInactive)
            {
                UpdateCamera();

                UpdateJetpack();

                UKismetMaterialLibrary.SetVectorParameterValue(
                    this,
                    Unreal.LoadObject<UMaterialParameterCollection>(this,
                        "/Game/StackOBot/Environment/Materials/MPC_DataSet.MPC_DataSet"),
                    "PlayerPosition",
                    UKismetMathLibrary.Conv_VectorToLinearColor(K2_GetActorLocation())
                );
            }
        }

        [Override]
        public override void ReceiveEndPlay(EEndPlayReason EndPlayReason)
        {
            TokenSource?.Cancel();
        }

        /*
         * As we use the enhanced input system, we have to add the mapping context to the controller
         */
        [Override]
        public override void ReceivePossessed(AController NewController)
        {
            if (NewController != null)
            {
                var PlayerController = NewController as APlayerController;

                if (PlayerController != null)
                {
                    var EnhancedInputLocalPlayerSubsystem =
                        USubsystemBlueprintLibrary.GetLocalPlayerSubSystemFromPlayerController(PlayerController,
                            UEnhancedInputLocalPlayerSubsystem.StaticClass()) as UEnhancedInputLocalPlayerSubsystem;

                    EnhancedInputLocalPlayerSubsystem?.AddMappingContext(
                        Unreal.LoadObject<UInputMappingContext>(this,
                            "/Game/StackOBot/Input/IM_ThirdPersonControls.IM_ThirdPersonControls"), 0);
                }
            }
        }

        /*
         * When character landed, reset the Jetpack
         */
        [Override]
        public override void OnLanded(FHitResult Hit)
        {
            /*
             * The jetpack is also a skeletal mesh, so you could extend it with a rig and some procedural animations driven by its status and thrust.
             * The jetpack could also become its own component or actor, allowing you to place one in the level and have the character pick it up as an upgrade.
             */
            ToggleJetpack(true);
        }

        /*
         * INPUT: Movement
         */
        public void IA_MoveForward_Triggered(FInputActionValue ActionValue = null, float ElapsedTime = 0,
            float TriggeredTime = 0, UInputAction SourceAction = null)
        {
            /*
             * Zero out pitch and roll, only move on plane
             */
            AddMovementInput(UKismetMathLibrary.GetForwardVector(new FRotator
                {
                    Yaw = GetControlRotation().Yaw
                }),
#if UE_5_1_OR_LATER
                (float) UEnhancedInputLibrary.Conv_InputActionValueToAxis1D(ActionValue)
#else
                UEnhancedInputLibrary.Conv_InputActionValueToAxis1D(ActionValue)
#endif
            );
        }

        /*
         * INPUT: Movement
         */
        public void IA_MoveRight_Triggered(FInputActionValue ActionValue = null, float ElapsedTime = 0,
            float TriggeredTime = 0, UInputAction SourceAction = null)
        {
            /*
             * Zero out pitch and roll, only move on plane
             */
            AddMovementInput(UKismetMathLibrary.GetRightVector(new FRotator
                {
                    Yaw = GetControlRotation().Yaw
                }),
#if UE_5_1_OR_LATER
                (float) UEnhancedInputLibrary.Conv_InputActionValueToAxis1D(ActionValue)
#else
                UEnhancedInputLibrary.Conv_InputActionValueToAxis1D(ActionValue)
#endif
            );
        }

        /*
         * INPUT: Turning
         */
        public void IA_Turn_Triggered(FInputActionValue ActionValue = null, float ElapsedTime = 0,
            float TriggeredTime = 0, UInputAction SourceAction = null)
        {
#if UE_5_1_OR_LATER
            AddControllerYawInput((float) UEnhancedInputLibrary.Conv_InputActionValueToAxis1D(ActionValue));
#else
            AddControllerYawInput(UEnhancedInputLibrary.Conv_InputActionValueToAxis1D(ActionValue));
#endif
        }

        public void IA_LookUp_Triggered(FInputActionValue ActionValue = null, float ElapsedTime = 0,
            float TriggeredTime = 0, UInputAction SourceAction = null)
        {
#if UE_5_1_OR_LATER
            AddControllerPitchInput((float) UEnhancedInputLibrary.Conv_InputActionValueToAxis1D(ActionValue));
#else
            AddControllerPitchInput(UEnhancedInputLibrary.Conv_InputActionValueToAxis1D(ActionValue));
#endif
        }

        /*
         * INPUT Jump & Jetpack: When walking do a regular jump, when in air, start the jetpack.
         * Jetpack can only be used once until character lands again.
         */
        public void IA_Jump_Started(FInputActionValue ActionValue = null, float ElapsedTime = 0,
            float TriggeredTime = 0, UInputAction SourceAction = null)
        {
            if (CharacterMovement.MovementMode is EMovementMode.MOVE_Walking or EMovementMode.MOVE_NavWalking)
            {
                Jump();
            }
            else if (CharacterMovement.MovementMode is EMovementMode.MOVE_Falling or EMovementMode.MOVE_Flying)
            {
                ToggleJetpack(false, true);
            }
        }

        /*
         * INPUT Jump & Jetpack: When walking do a regular jump, when in air, start the jetpack.
         * Jetpack can only be used once until character lands again.
         */
        public void IA_Jump_Completed(FInputActionValue ActionValue = null, float ElapsedTime = 0,
            float TriggeredTime = 0, UInputAction SourceAction = null)
        {
            StopJumping();

            ToggleJetpack();
        }

        /*
         * INPUT INTERACT: Starts the interaction with an overlapping object that has the BP_Interaction component implemented.
         * Using an Interface call
         */
        public void IA_Interact_Completed(FInputActionValue ActionValue = null, float ElapsedTime = 0,
            float TriggeredTime = 0, UInputAction SourceAction = null)
        {
            var OverlappingActors = new TArray<AActor>();

            GetOverlappingActors(ref OverlappingActors);

            if (OverlappingActors.Num() > 0)
            {
                /*
                 * Takes the first actor found.
                 * This could be extended to handle  multiple overlaps, for example by sorting by distance.
                 */
                var Component = OverlappingActors[0].GetComponentByClass(BP_InteractionComponent_C.StaticClass());

                if (Component != null)
                {
                    var InteractionComponent = Component as BP_InteractionComponent_C;

                    /*
                     * Get the button location from the interaction component, allowing the animation blueprint to get the value and feed it into control rig for fullbody IK adjustment of the press button animation
                     */
                    InteractionWorldLocation = InteractionComponent.InteractionWorldLocation;

                    InteractionComponent.StartInteraction();
                }
            }
        }

        /*
         * INPUT UNPOSSES AND SPAWN NEW: Player can leave the current bot behind and spawn a new one.
         */
        public void IA_UnpossesAndSpawnNew_Completed(FInputActionValue ActionValue = null, float ElapsedTime = 0,
            float TriggeredTime = 0, UInputAction SourceAction = null)
        {
            /*
             * Disable collision of the capsule and enable (query and physics) on the mesh
             */
            CapsuleComponent.SetCollisionEnabled(ECollisionEnabled.NoCollision);

            Mesh.SetCollisionEnabled(ECollisionEnabled.QueryAndPhysics);

            /*
             * Simulate the head.
             * Check out the bot's physics asset.
             */
            Mesh.SetAllBodiesBelowSimulatePhysics(UKismetSystemLibrary.MakeLiteralName("head"), true);

            /*
             * Detach the Camera so it stays where it is at the moment of unpossession
             */
            CameraBoom.K2_DetachFromComponent(EDetachmentRule.KeepWorld, EDetachmentRule.KeepWorld,
                EDetachmentRule.KeepWorld, false);

            IsInactive = true;

            /*
             * Start a timer to check if to go to full ragdoll
             */
            TokenSource = new CancellationTokenSource();

            CheckRagdoll();

            /*
             * Change the face and disable emissive to indicate this bot is inactive.
             */
            foreach (var Material in Materials)
            {
                Material.SetScalarParameterValue("EnablePrinting", 0.0f);

                Material.SetScalarParameterValue("Mood", 15.0f);
            }

            Delay();
        }

        /*
         * looping check as an own timer (not on tick) - if the surface below is removed or the bot moves a bit switch to full ragdoll and clear this timer
         */
        private async void CheckRagdoll()
        {
            while (!TokenSource.IsCancellationRequested)
            {
                if (CheckForSurfaceOrVelocity())
                {
                    Mesh.SetSimulatePhysics(true);

                    TokenSource.Cancel();
                }

                await Task.Delay(100);
            }
        }

        /*
         * Check if we still standing on something. If not go to ragdoll.
         */
        private bool CheckForSurfaceOrVelocity()
        {
            var OutHits = new TArray<FHitResult>();

            var bHit = UKismetSystemLibrary.LineTraceMultiForObjects(
                this,
                K2_GetActorLocation(),
                K2_GetActorLocation() - new FVector { Z = 100.0f },
                new TArray<EObjectTypeQuery>
                {
                    (EObjectTypeQuery)ECollisionChannel.WorldStatic,
                    (EObjectTypeQuery)ECollisionChannel.PhysicsBody,
                    (EObjectTypeQuery)ECollisionChannel.WorldDynamic
                },
                true,
                new TArray<AActor>(),
                EDrawDebugTrace.None,
                ref OutHits,
                true,
                new FLinearColor
                {
                    R = 1.0f,
                    A = 1.0f
                },
                new FLinearColor
                {
                    G = 1.0f,
                    A = 1.0f
                },
                5.0f
            );

            /*
             * check if the head is moving quickly, so if something like the stomper hits us, we go into ragdoll
             */
            var bHeadIsMovingQuickly = Mesh.GetPhysicsLinearVelocity("head").Size() > 200.0f;

            /*
             * Check if the actor is moving quickly (like in mid-air or on a moving platform) if yes go to ragdoll
             */
            var bActorIsMovingQuickly = GetVelocity().Size() >= 10.0f;

            return bHeadIsMovingQuickly || !bHit || bActorIsMovingQuickly;
        }

        /*
         * Delay a bit and start spawning a new character via the game mode.
         */
        private async void Delay()
        {
            await Task.Delay(500);

            var GM_InGame = UGameplayStatics.GetGameMode(this) as GM_InGame_C;

            GM_InGame?.SpawnPlayerAtActiveSpawnPad();
        }

        private CancellationTokenSource TokenSource;

        /*
         * Update the parameters in the robot and jetpack material during the spawn effect.
         * Updated by the Spawnpad
         */
        public void EnableSpawnEffect(double EnablePrinting = 0, double SliceZ = 0)
        {
            foreach (var Material in Materials)
            {
                Material.SetScalarParameterValue("EnablePrinting", (float)EnablePrinting);

                Material.SetScalarParameterValue("SliceZ", (float)SliceZ);
            }
        }

        /*
         * DISSOLVE called by game mode when player reached the maximum amount of bots allowed.
         */
        public void Dissolve()
        {
            /*
             * First disable simulate physics of the mesh and then diable collision
             */
            Mesh.SetSimulatePhysics(false);

            Mesh.SetCollisionEnabled(ECollisionEnabled.NoCollision);

            /*
             * Spawn a dissolve effect that samples the skeletal mesh and feed the color in so the effect is the same color as the bot
             */
            var NiagaraComponent = UNiagaraFunctionLibrary.SpawnSystemAttached(
                Unreal.LoadObject<UNiagaraSystem>(this,
                    "/Game/StackOBot/FX/Desolve/FX_CharacterDesolve.FX_CharacterDesolve"),
                Mesh,
                new FName("None"),
                new FVector(),
                new FRotator(),
                EAttachLocation.KeepRelativeOffset,
                true
            );

            var Hue = GetHueByIndex();

            NiagaraComponent.SetColorParameter("AntBotColor", new FLinearColor
            {
                R = (float)Hue,
                A = 1.0f
            });

            /*
             * Jetpack and Mesh set to invisible
             */
            Jetpack.SetVisibility(false);

            Mesh.SetVisibility(false);

            /*
             * Set the life span so this actor gets destroyed after 10 seconds
             */
            SetLifeSpan(10.0f);
        }

        /*
         * The jetpack can be started, stopped and reset here.
         * This is called by the jump input when in air or reset when landed.
         * The jetpack and its functionality could become an own component if you would have several characters able to use it.
         */
        private void ToggleJetpack(bool Reset = false, bool Activate = false)
        {
            /*
             * If reset, thruster time is set back to max thruster time
             */
            if (Reset)
            {
                ThrusterTime = ThrusterMaxTime;
            }

            JetpackActive = Activate;

            if (JetpackActive)
            {
                /*
                 * More air control while jetpack is active
                 */
                CharacterMovement.AirControl = 5.0f;

                /*
                 * The jetpack particle effect is activated
                 */
                FX_JetpackThruster.SetActive(true);

                /*
                 * The meta sound gets started.
                 * It has an ignite and a loop that is played until the JetpackOff event is triggered
                 */
                JetpackSFX.Play();
            }
            else
            {
                /*
                 * More air control while jetpack is active
                 */
                CharacterMovement.AirControl = 1.0f;

                /*
                 * The jetpack particle effect is deactivated
                 */
                FX_JetpackThruster.SetActive(false);

                /*
                 * The parameter interface of the metasound only exists while playing.
                 * So check before trying to pass data in.
                 * When the jetpack goes off a trigger switches of the sound.
                 * But instead just stop playing the looping part of the sound stops and a winding down sound is played.
                 */
                if (JetpackSFX.IsPlaying())
                {
                    JetpackSFX.SetTriggerParameter("JetpackOff");
                }
            }
        }

        /*
         * Update Jetpack, called from tick function to decrease the thruster time.
         * Using a curve asset to get the Z force applied on the character.
         */
        private void UpdateJetpack()
        {
            /*
             * When Jetpack is active decrease the thruster time
             */
            if (JetpackActive)
            {
                ThrusterTime -= UGameplayStatics.GetWorldDeltaSeconds(this);

                /*
                 * Using ThrusterMaxTime to get a value between 0 and 1 and use that in a curve  to get the launch velocity on the character.
                 * Now you can modify the curve to get the wanted behaviour.
                 * The bot is only launched a bit at start but then the jetpack depletes .
                 */
                var CurveValue = JetpackBoostCurve.GetFloatValue(
                    (float)UKismetMathLibrary.NormalizeToRange(ThrusterMaxTime - ThrusterTime, 0.0f,
                        ThrusterMaxTime));

                LaunchCharacter(new FVector
                {
                    Z = CurveValue * 120.0f
                }, false, true);

                /*
                 * The particle effect samples the sockets and has a variable that can be updated.
                 * So the thrust amount is reflected in the visible effect.
                 * You could also feed this data into the sound if you want.
                 */
                FX_JetpackThruster.SetNiagaraVariableFloat("ThrusterStrength", CurveValue);

                /*
                 * When thruster time is 0 the jetpack is toggled off
                 */
                if (ThrusterTime <= 0.0f)
                {
                    ToggleJetpack();
                }
            }
        }

        /*
         * The camera in 3rd person games can be very challenging.
         * Here is only a tiny setup tp deal with rooms.
         * Feel free to experiement more with traces, collision types, etc
         */
        private void UpdateCamera()
        {
            /*
             * Trace upwards to check if we are "inside"
             */
            var OutHit = new FHitResult();

            var bHit = UKismetSystemLibrary.LineTraceSingle(
                this,
                K2_GetActorLocation(),
                K2_GetActorLocation() + new FVector { Z = 250.0f },
                (ETraceTypeQuery)ECollisionChannel.Camera,
                false,
                new TArray<AActor>(),
                EDrawDebugTrace.None,
                ref OutHit,
                true,
                new FLinearColor
                {
                    R = 1.0f,
                    A = 1.0f
                },
                new FLinearColor
                {
                    G = 1.0f,
                    A = 1.0f
                },
                5.0f
            );

            /*
             * When the bot is in air the camera moves out a bit
             */
            var Bit = CharacterMovement.IsFalling() ? 600.0f : 500.0f;

            /*
             * FInterpTo is used to change the Target Arm Length over time.
             */
            var Target = bHit ? 300.0f : Bit;

            CameraBoom.TargetArmLength = (float)UKismetMathLibrary.FInterpTo(CameraBoom.TargetArmLength, Target,
                UGameplayStatics.GetWorldDeltaSeconds(this), 2.0f);
        }

        /*
         * When the game mode spawns a bot it gets an index.
         * That is used to calculate a hue that is passed into the material to change its color.
         * The dissolve particle effect uses it as well.
         * The more robots spawned, the slighter the difference in color will be.
         */
        private double GetHueByIndex()
        {
            var Max = BPFL_InGame_C.GetMaxBots();

            return 1.0f * BotIdx / Max;
        }

        private void SetMaterials()
        {
            var Material = Mesh.CreateDynamicMaterialInstance(0);

            Materials.Add(Material);

            var Hue = GetHueByIndex();

            Material.SetScalarParameterValue("HueShift", (float)Hue);

            Material = Mesh.CreateDynamicMaterialInstance(1);

            Materials.Add(Material);

            Material = Jetpack.CreateDynamicMaterialInstance(0);

            Materials.Add(Material);
        }

        public void SetBotIdx(int InBotIdx)
        {
            BotIdx = InBotIdx;

            SetMaterials();
        }

        private int BotIdx;
    }
}