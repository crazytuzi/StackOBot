using System;
using Script.Common;
using Script.CoreUObject;
using Script.Engine;
using Script.Game.StackOBot.Blueprints.Framework;
using Script.Game.StackOBot.Blueprints.SaveGame;
using Script.Library;

namespace Script.Game.StackOBot.Blueprints.GameElements
{
    /*
     * The orb is the only collectable in this project.
     * If you want more, you could create a parent base class with the standard functionality and create children of them to do handle visuals and data.
     */
    [IsOverride]
    public partial class BP_EnergyOrb_C
    {
        /*
         * Set a bit of random starting rotation per instance.
         */
        [IsOverride]
        public override void ReceiveBeginPlay()
        {
            OrbMesh.K2_SetWorldRotation(
                new FRotator
                {
                    Roll = 0.0f,
                    Pitch = 0.0f,
                    Yaw = UKismetMathLibrary.RandomIntegerInRange(0, 15) * 22.5f
                },
                false,
                out var SweepHitResult,
                false);

            OnActorBeginOverlap.Add(this, OnActorBeginOverlapSignature);
        }

        [IsOverride]
        public override void ReceiveEndPlay(EEndPlayReason EndPlayReason)
        {
            OnActorBeginOverlap.RemoveAll(this);
        }

        /*
         * Add a bit of rotation per tick
         */
        [IsOverride]
        public override void ReceiveTick(float DeltaSeconds)
        {
            OrbMesh.K2_AddLocalRotation(
                new FRotator
                {
                    Roll = 0.0,
                    Pitch = 0.0,
                    Yaw = DeltaSeconds * 100.0f
                },
                false,
                out var SweepHitResult,
                false);
        }

        private void OnActorBeginOverlapSignature(AActor OverlappedActor, AActor OtherActor)
        {
            /*
             * The root of this component is box collision that overlaps only the pawn.
             * As we have only one pawn no additional checks are needed.
             */
            PlayerCharacter = OtherActor;

            /*
             * The game instance stores the amount of collected orbs which we increase by one.
             */
            var GameInstance = UGameplayStatics.GetGameInstance(this);

            (GameInstance as IBPI_GameInstance_C)?.UpdateOrbs(1);

            /*
             * We get this actor class and transform to remove this orb from the saved game so next time it doesnt get recreated on level load.
             * The saved game needs to be saved to disc, which occurs when the character next overlaps with a spawnpad.
             */
            (GameInstance as IBPI_GameInstance_C)?.RemoveCollectableFromSaveGame(new CollectableObjectData
            {
                ActorClass = GetClass(),
                Transform = GetTransform()
            });

            Box.SetCollisionEnabled(ECollisionEnabled.NoCollision);

            /*
             * Play a procedural metasound at this actors location
             */
            var Sound = Unreal.LoadObject<USoundBase>(this, "/Game/StackOBot/Audio/SFX_CollectCoin.SFX_CollectCoin");

            UGameplayStatics.PlaySoundAtLocation(this, Sound, K2_GetActorLocation(), new FRotator());

            /*
             * Store the actors transform for the collect animation
             */
            ActorTransform = GetTransform();

            CollectCoinTransition.SetPlayRate(2.5f);

            CollectCoinTransition.PlayFromStart();
        }

        /*
         * Lerp location from actor location to player location with an offset, scaling it down over time.
         */
        [IsOverride]
        private void CollectCoinTransition__UpdateFunc()
        {
            var Height = CollectCoinTransition.TheTimeline.InterpFloats[0].FloatCurve
                .GetFloatValue(CollectCoinTransition.TheTimeline.Position);

            var Distance = CollectCoinTransition.TheTimeline.InterpFloats[1].FloatCurve
                .GetFloatValue(CollectCoinTransition.TheTimeline.Position);

            var Scale = CollectCoinTransition.TheTimeline.InterpFloats[2].FloatCurve
                .GetFloatValue(CollectCoinTransition.TheTimeline.Position);

            var Location = UKismetMathLibrary.VLerp(ActorTransform.GetLocation(),
                               PlayerCharacter.K2_GetActorLocation(),
                               Distance)
                           + new FVector {Z = Height * 200.0};

            K2_SetActorTransform(new FTransform
                {
                    Translation = Location,
                    Rotation = K2_GetActorRotation().Quaternion(),
                    Scale3D = new FVector {X = Scale, Y = Scale, Z = Scale}
                },
                false,
                out var SweepHitResult,
                false);
        }

        /*
         * when animation is finished remove this orb
         */
        [IsOverride]
        private void CollectCoinTransition__FinishedFunc()
        {
            K2_DestroyActor();
        }
    }
}