using System;
using Script.Common;
using Script.CoreUObject;
using Script.Engine;
using Script.Game.StackOBot.Blueprints.Abilities;
using Script.Library;

namespace Script.Game.StackOBot.Blueprints.GameElements
{
    /*
     * The door has an array of triggers, meaning designers can require multiple pressure plate interactions before the door opens.
     * You can implement different door logic via the events triggered by the interaction component.
     */
    [IsOverride]
    public partial class BP_Door_C
    {
        /*
         * Triggers are in an exposed array that can be edited per instance.
         * If the level designer added references to triggers (buttons or pressure plates) we assign OnInteract event for all of them.
         */
        [IsOverride]
        public override void ReceiveBeginPlay()
        {
            if (Triggers.Num() > 0)
            {
                foreach (var Trigger in Triggers)
                {
                    var Component =
                        Unreal.Cast<BP_InteractionComponent_C>(
                            Trigger.GetComponentByClass(BP_InteractionComponent_C.StaticClass()));

                    Component?.OnInteract.Add(this, OnInteract);
                }
            }

            Opengate.TheTimeline.TimelinePostUpdateFunc.Unbind();

            Opengate.TheTimeline.TimelinePostUpdateFunc.Bind(this, OnTimelinePostUpdateFunc);
        }

        [IsOverride]
        public override void ReceiveEndPlay(EEndPlayReason EndPlayReason)
        {
            if (Triggers.Num() > 0)
            {
                foreach (var Trigger in Triggers)
                {
                    var Component =
                        Unreal.Cast<BP_InteractionComponent_C>(
                            Trigger.GetComponentByClass(BP_InteractionComponent_C.StaticClass()));

                    Component?.OnInteract.RemoveAll(this);
                }
            }

            Opengate.TheTimeline.TimelinePostUpdateFunc.Unbind();
        }

        private void OnInteract(Boolean On = false)
        {
            if (On)
            {
                /*
                 * On Interact "on" of one of the components in the array, we increase the count of active triggers.
                 * If they are more than the triggers needed (which can also be set by the level designer) we call open door
                 */
                ++TriggersActive;

                if (TriggersActive >= TriggersNeeded)
                {
                    OpenDoor();
                }
            }
            else
            {
                /*
                 * On Interact "off" of one of the components in the array, we decrease the amount of active triggers and close the door
                 */
                --TriggersActive;

                TriggersActive = UKismetMathLibrary.Max(TriggersActive, 0);

                CloseDoor();
            }
        }

        private void OpenDoor()
        {
            Opengate.Play();
        }

        private void CloseDoor()
        {
            Opengate.Reverse();
        }

        /*
         * Open and close door playing a timeline forward or reverse.
         * The numbers for the Y location of the doors were determined through testing and iteration.
         */
        private void OnTimelinePostUpdateFunc()
        {
            var DoorOpen = Opengate.TheTimeline.InterpFloats[0].FloatCurve
                .GetFloatValue(Opengate.TheTimeline.Position);

            DoorRight.K2_SetRelativeLocation(
                new FVector
                {
                    X = 0.0f,
                    Y = DoorOpen * -70.0f + 172,
                    Z = 0.0f
                },
                false,
                out var SweepHitResultRight,
                false);

            DoorLeft.K2_SetRelativeLocation(
                new FVector
                {
                    X = 0.0f,
                    Y = DoorOpen * 70.0f + 328,
                    Z = 0.0f
                },
                false,
                out var SweepHitResultLeft,
                false);
        }
    }
}