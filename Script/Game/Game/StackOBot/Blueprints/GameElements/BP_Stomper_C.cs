﻿using Script.CoreUObject;
using Script.Engine;
using Script.Game.StackOBot.Blueprints.Abilities;

namespace Script.Game.StackOBot.Blueprints.GameElements
{
    /*
     * The stomper moves up and down driven by a curve in the timeline.
     * When no trigger is referenced it move constantly.
     * An assigned trigger, such as the pressure plate, activates and deactivates the stomper.
     */
    [Override]
    public partial class BP_Stomper_C
    {
        [Override]
        public override void ReceiveBeginPlay()
        {
            /*
             * Set the initial position of the stomper
             */
            InitStomper();

            /*
             * If there is an assigned trigger, assign an event to the OnInteraction of the interaction component of that trigger.
             * If not, just start the stomper
             */
            if (Trigger != null && Trigger.IsValid())
            {
                var Component =
                    Trigger.GetComponentByClass(BP_InteractionComponent_C.StaticClass()) as BP_InteractionComponent_C;

                Component.OnInteract.Add(this, OnInteract);
            }
            else
            {
                StartStomper();
            }
        }

        [Override]
        public override void ReceiveEndPlay(EEndPlayReason EndPlayReason)
        {
            if (Trigger != null && Trigger.IsValid())
            {
                var Component =
                    Trigger.GetComponentByClass(BP_InteractionComponent_C.StaticClass()) as BP_InteractionComponent_C;

                Component.OnInteract.RemoveAll(this);
            }
        }

        /*
         * Setup this way, we won't see the initial position in the editor, only when play.
         * If you would like to show it in editor you would need to leverage the construction script - but the timeline won't work there.
         */
        private void InitStomper()
        {
            StomperMovement.SetNewTime((float)InitialPos);
        }

        private void StartStomper()
        {
            StomperMovement.Play();
        }

        private void StopStomper()
        {
            StomperMovement.Stop();
        }

        /*
         * Using a curve in a timeline to move the stomper up and down.
         */
        [Override]
        private void StomperMovement__UpdateFunc()
        {
            var Movement = StomperMovement.TheTimeline.InterpFloats[0].FloatCurve
                .GetFloatValue(StomperMovement.TheTimeline.Position);

            var SweepHitResult = new FHitResult();

            Stomper.K2_SetRelativeLocation(
                new FVector
                {
                    X = Movement * -400.0f + 500.0f
                },
                false,
                ref SweepHitResult,
                false);
        }

        private void OnInteract(bool On)
        {
            if (On)
            {
                StartStomper();
            }
            else
            {
                StopStomper();
            }
        }
    }
}