﻿using System;
using Script.CoreUObject;
using Script.Engine;
using Script.Game.StackOBot.Blueprints.Abilities;

namespace Script.Game.StackOBot.Blueprints.GameElements
{
    /*
     * The ramp can be triggered and is rotated so it can open new paths in the level.
     * This is very limited behaviour but you could extend or even build a whole bridge with several ones in a similar way.
     */
    [Override]
    public partial class BP_Ramp_C
    {
        /*
         * Like the door and other game objects, the ramp can reqyire more than one trigger.
         * You can specify an array of triggers and how many are required to be active, which makes it quite flexible.
         * For more details check out the BP_Door.
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
            }
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
        }

        private void OnInteract(bool On = false)
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
                    ToUpperLocation();
                }
            }
            else
            {
                /*
                 * On Interact "off" of one of the components in the array, we decrease the amount of active triggers and close the door
                 */
                --TriggersActive;

                TriggersActive = Math.Max(TriggersActive, 0);

                ToLowerLocation();
            }
        }

        private void ToUpperLocation()
        {
            RiseRamp.Play();
        }

        private void ToLowerLocation()
        {
            RiseRamp.Reverse();
        }

        /*
         * Using a timeline and an angle specified by the level designer to rotate the ramp up or down.
         */
        [Override]
        private void RiseRamp__UpdateFunc()
        {
            var Rise = RiseRamp.TheTimeline.InterpFloats[0].FloatCurve
                .GetFloatValue(RiseRamp.TheTimeline.Position);

            var SweepHitResult = new FHitResult();

            RampMesh.K2_SetRelativeRotation(
                new FRotator
                {
                    Pitch = Rise * UpperAngle,
                },
                false,
                ref SweepHitResult,
                false);
        }

        private int TriggersActive;
    }
}