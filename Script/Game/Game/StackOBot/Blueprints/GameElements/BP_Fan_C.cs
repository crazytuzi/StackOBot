using System;
using System.Threading;
using System.Threading.Tasks;
using Script.CoreUObject;
using Script.Engine;
using Script.Game.StackOBot.Blueprints.Abilities;

namespace Script.Game.StackOBot.Blueprints.GameElements
{
    /*
     * The fan can be activated and deactivated.
     * When active, it adds force to the player when they are inside the FanArea.
     * As we use Launch Character it cannot push away crates or other objects.
     * Another caveat is that you need to stop applying force in the event of the player deciding to unposses this character and spawn a new one.
     * If we don't check, the force would keep being applied to the new spawned character.
     * In a more complex project, you would likely use a different setup than this.
     */
    [Override]
    public partial class BP_Fan_C
    {
        /*
         * This is the same setup as the BP_Door.
         * Check there for more details.
         * If you use identical behavior across different actors, consider moving it to its own component.
         */
        [Override]
        public override void ReceiveBeginPlay()
        {
            if (Triggers.Num() > 0)
            {
                foreach (var Trigger in Triggers)
                {
                    var Component =
                        Trigger.GetComponentByClass(BP_InteractionComponent_C.StaticClass()) as
                            BP_InteractionComponent_C;

                    Component?.OnInteract.Add(this, OnInteract);
                }

                StopFan();
            }
            else
            {
                StartFan();
            }

            FanArea.OnComponentBeginOverlap.Add(this, OnComponentBeginOverlap);

            FanArea.OnComponentEndOverlap.Add(this, OnComponentEndOverlap);
        }

        [Override]
        public override void ReceiveEndPlay(EEndPlayReason EndPlayReason)
        {
            if (Triggers.Num() > 0)
            {
                foreach (var Trigger in Triggers)
                {
                    var Component =
                        Trigger.GetComponentByClass(BP_InteractionComponent_C.StaticClass()) as
                            BP_InteractionComponent_C;

                    Component?.OnInteract.RemoveAll(this);
                }
            }

            FanArea.OnComponentBeginOverlap.RemoveAll(this);

            FanArea.OnComponentEndOverlap.RemoveAll(this);

            TokenSource?.Cancel();
        }

        private void OnInteract(bool On)
        {
            if (On)
            {
                ++TriggersActive;

                if (TriggersActive >= TriggersNeeded)
                {
                    StartFan();
                }
            }
            else
            {
                --TriggersActive;

                TriggersActive = Math.Max(TriggersActive, 0);

                StopFan();
            }
        }

        /*
         * When the fan is active we want to "blow" the player away from the fan so we start a looping timer
         */
        private void OnComponentBeginOverlap(UPrimitiveComponent OverlappedComponent, AActor OtherActor,
            UPrimitiveComponent OtherComp, int OtherBodyIndex, bool bFromSweep, FHitResult SweepResult)
        {
            if (Active)
            {
                TokenSource?.Cancel();

                TokenSource = new CancellationTokenSource();

                UpdateForce();
            }
        }

        /*
         * Stop the force when player doesnt overlap anymore.
         */
        private void OnComponentEndOverlap(UPrimitiveComponent OverlappedComponent, AActor OtherActor,
            UPrimitiveComponent OtherComp, int OtherBodyIndex)
        {
            TokenSource?.Cancel();
        }

        private void StartFan()
        {
            Active = true;

            FX_WindBands.Activate(true);

            FanRotation.Play();
        }

        private void StopFan()
        {
            Active = false;

            FX_WindBands.Deactivate();

            FanRotation.Stop();
        }

        /*
         * Start and stop fan (de)activates a particle effect with a ribbon to visualize wind and rotates the fan with a looped timeline.
         */
        [Override]
        private void FanRotation__UpdateFunc()
        {
            var RotationSpeed = FanRotation.TheTimeline.InterpFloats[0].FloatCurve
                .GetFloatValue(FanRotation.TheTimeline.Position);

            var SweepHitResult = new FHitResult();

            Fan.K2_AddLocalRotation(
                new FRotator
                {
                    Roll = RotationSpeed,
                    Pitch = 0.0,
                    Yaw = 0.0
                },
                false,
                ref SweepHitResult,
                false);
        }

        private async void UpdateForce()
        {
            while (!TokenSource.IsCancellationRequested)
            {
                /*
                 * When the player unpossesses the character while in mid air, we want to force to stop and not being applied to the newly spawned character.
                 */
                var PlayerCharacter = UGameplayStatics.GetPlayerCharacter(this, 0);

                if (PlayerCharacter.IsOverlappingActor(this))
                {
                    /*
                     * Using an arrow component to define the direction of the force
                     */
                    var A = ForceDirection.GetForwardVector();

                    /*
                     * We caluclate the strength of the force by the distance of the player and the actor origin and map it with the box extents of the fan trigger.
                     * More force the closer the player is to the fan's origin point.
                     */
                    var B = UKismetMathLibrary.MapRangeClamped(
                        (PlayerCharacter.K2_GetActorLocation() - K2_GetActorLocation()).Size(),
                        0.0,
                        FanArea.BoxExtent.X * 2.0,
                        800.0,
                        0.0
                    );

                    var LaunchVelocity = A * B + new FVector
                    {
                        Z = 20.0
                    };

                    /*
                     * Use Launch character to apply force
                     */
                    PlayerCharacter.LaunchCharacter(LaunchVelocity, false, true);
                }
                else
                {
                    /*
                     * Stop updating and thus launching the character
                     */
                    TokenSource.Cancel();
                }

                await Task.Delay(10);
            }
        }

        private bool Active;

        private int TriggersActive;

        private CancellationTokenSource TokenSource;
    }
}