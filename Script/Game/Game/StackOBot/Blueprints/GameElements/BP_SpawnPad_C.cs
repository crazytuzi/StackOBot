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

            Spawn.TheTimeline.TimelinePostUpdateFunc.Unbind();

            Spawn.TheTimeline.TimelinePostUpdateFunc.Bind(this, OnTimelinePostUpdateFunc);

            Spawn.TheTimeline.TimelineFinishedFunc.Unbind();

            Spawn.TheTimeline.TimelineFinishedFunc.Bind(this, TimelineFinishedFunc);
        }

        [IsOverride]
        public override void ReceiveEndPlay(EEndPlayReason EndPlayReason)
        {
            ActivationTrigger.OnComponentBeginOverlap.RemoveAll(this);

            Spawn.TheTimeline.TimelinePostUpdateFunc.Unbind();

            Spawn.TheTimeline.TimelineFinishedFunc.Unbind();
        }

        /*
         * (De)activate the particle effect and switch the material to show a glow or a metal plate.
         */
        [IsOverride]
        private void ToggleActivation(Boolean On = false)
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
         * This function handles the visual "printing" of the robot and the camera.
         * It also disables the input while the printing is in progress.
         * Called by SpawnPlayer and not part of that function as we use a timeline that only works in the event graph.
         */
        [IsOverride]
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
        private void OnTimelinePostUpdateFunc()
        {
            var RingMovement = Spawn.TheTimeline.InterpFloats[0].FloatCurve
                .GetFloatValue(Spawn.TheTimeline.Position);

            SpawnPadRingMesh.K2_SetRelativeLocation(new FVector
                {
                    X = 0.0f,
                    Y = 0.0f,
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
        private void TimelineFinishedFunc()
        {
            var PlayerController = UGameplayStatics.GetPlayerController(this, 0);

            Bot.EnableInput(PlayerController);

            PlayerController.SetViewTargetWithBlend(Bot, 0.3f);

            HoloGridPlane.SetVisibility(false);
        }

        private BP_Bot_C Bot;
    }
}