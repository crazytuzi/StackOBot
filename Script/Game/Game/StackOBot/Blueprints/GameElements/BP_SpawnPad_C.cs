using System;
using Script.Common;
using Script.CoreUObject;
using Script.Engine;
using Script.Game.StackOBot.Blueprints.Character;
using Script.Game.StackOBot.Blueprints.Framework;
using Script.Library;

namespace Script.Game.StackOBot.Blueprints.GameElements
{
    /*
     * The spawn pad handles the spawning of the character when triggerd by the game mode.
     * It starts in the function "SpawnPlayer".
     * As we wanted it look like the character get printed feed data into the robot that has a function to update it's material.
     * As a level can have more than one spawn pad the game mode keeps reference to the active one.
     * The level designer can set IsStartSpawnPad for the first level start.
     * When the player walks over a spawnpad it tells the game mode it is the new active one and it tells the game instance to save the game to disc.
     */
    [IsOverride]
    public partial class BP_SpawnPad_C
    {
        /*
         * When this is the start spawn pad, active it
         */
        [IsOverride]
        public override void ReceiveBeginPlay()
        {
            ToggleActivation(IsStartSpawnPad);

            ActivationTrigger.OnComponentBeginOverlap.Add(this, OnComponentBeginOverlap);
        }

        [IsOverride]
        public override void ReceiveEndPlay(EEndPlayReason EndPlayReason)
        {
            ActivationTrigger.OnComponentBeginOverlap.RemoveAll(this);
        }

        /*
         * (De)activate the particle effect and switch the material to show a glow or a metal plate.
         */
        public void ToggleActivation(Boolean On = false)
        {
            if (On)
            {
                FX_SpawnPadActive.Activate();

                var SourceMaterial =
                    Unreal.LoadObject<UMaterialInstance>(this, "/Game/StackOBot/Environment/Materials/MI_Glow.MI_Glow");

                SpawnPadBaseMesh.SetMaterial(1, SourceMaterial);
            }
            else
            {
                FX_SpawnPadActive.Deactivate();

                var SourceMaterial =
                    Unreal.LoadObject<UMaterialInstance>(this,
                        "/Game/StackOBot/Environment/Materials/MI_Metal.MI_Metal");

                SpawnPadBaseMesh.SetMaterial(1, SourceMaterial);
            }
        }

        /*
         * The sphere collision is set to only overlap the player.
         * Here we cast which means here is a hard reference to the game mode.
         * This is okay as the spawn pad and the game mode can not work without the other.
         * We set this the new spawn pad (and the game mode tells the old spawn pad to deactivate.
         * The game mode tells the game instance to save and the UI to display a save animation.
         */
        private void OnComponentBeginOverlap(UPrimitiveComponent OverlappedComponent, AActor OtherActor,
            UPrimitiveComponent OtherComp, Int32 OtherBodyIndex, Boolean bFromSweep, FHitResult SweepResult)
        {
            var GameMode = Unreal.Cast<GM_InGame_C>(UGameplayStatics.GetGameMode(this));

            if (GameMode != null)
            {
                GameMode.SetCurrentSpawnPad(this, out var Success);

                ToggleActivation(Success);
            }
        }

        /*
         * We spawn the character using an arrow component to set the exact location.
         * Even as you spawn more robots, you can leave your old one at this exact location.
         * Given that, we try to adjust our location to avoid colliding but always spawn.
         * This can lead to issues if you spawn a lot of objects in a single location, you would likely need a more complex ruleset in a full game.
         */
        public BP_Bot_C SpawnPlayer(Int32 BotIdx)
        {
            var Character = GetWorld().SpawnActor<BP_Bot_C>(PlayerSpawnLocation.K2_GetComponentToWorld(), null, null,
                ESpawnActorCollisionHandlingMethod.AdjustIfPossibleButAlwaysSpawn);

            Character.SetBotIdx(BotIdx);

            /*
             * Possess the new spawned bot
             */
            var PlayerController = UGameplayStatics.GetPlayerControllerFromID(this, 0);

            PlayerController.Possess(Character);

            /*
             * Start the "printing" animation
             */
            StartSpawnAnimation(Character);

            return Character;
        }

        /*
         * This function handles the visual "printing" of the robot and the camera.
         * It also disables the input while the printing is in progress.
         * Called by SpawnPlayer and not part of that function as we use a timeline that only works in the event graph.
         */
        private void StartSpawnAnimation(BP_Bot_C PlayerCharacter)
        {
            Bot = PlayerCharacter;

            var PlayerController = UGameplayStatics.GetPlayerController(this, 0);

            /*
             * Switch the camera to the one here in the spawn pad and disabple player input
             */
            PlayerController.SetViewTargetWithBlend(PlayerCharacter);

            PlayerCharacter.DisableInput(PlayerController);

            /*
             * Make the Holo Grid plane visible that we will move up with the ring of the pad
             */
            HoloGridPlane.SetVisibility(true);

            Spawn.PlayFromStart();
        }

        /*
         * The timeline has two curves.
         * One moving the ring and one toggeling the slice effect in the bots material.
         * Check the EnbaleSpawnEffect function in BP_Bot to see how it works.
         * The materials in the bot have a material function handling the visual effects.
         */
        [IsOverride]
        private void Spawn__UpdateFunc()
        {
            var RingMovement = Spawn.TheTimeline.InterpFloats[0].FloatCurve
                .GetFloatValue(Spawn.TheTimeline.Position);

            SpawnPadRingMesh.K2_SetRelativeLocation(new FVector
                {
                    Z = 200.0f
                } * RingMovement,
                false,
                out var SweepHitResult,
                false);

            var SliceEffect = Spawn.TheTimeline.InterpFloats[1].FloatCurve
                .GetFloatValue(Spawn.TheTimeline.Position);

            Bot.EnableSpawnEffect(SliceEffect, SpawnPadRingMesh.K2_GetComponentLocation().Z);
        }

        /*
         * When the animation is finished, we enable the input and switch back to the camera in BP_Bot.
         * Then we make the hologrid invisible again.
         */
        [IsOverride]
        private void Spawn__FinishedFunc()
        {
            var PlayerController = UGameplayStatics.GetPlayerController(this, 0);

            Bot.EnableInput(PlayerController);

            PlayerController.SetViewTargetWithBlend(Bot, 0.3f);

            HoloGridPlane.SetVisibility(false);
        }

        private BP_Bot_C Bot;
    }
}