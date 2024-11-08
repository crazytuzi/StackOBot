﻿using Script.CoreUObject;
using Script.Engine;
using Script.Game.StackOBot.Blueprints.Abilities;

namespace Script.Game.StackOBot.Blueprints.GameElements
{
    /*
     * The elevator is another example of how to use the triggers.
     * It has two locations and two triggers we mapped so the elevator goes between them.
     */
    [Override]
    public partial class BP_Elevator_C
    {
        [Override]
        public override void ReceiveBeginPlay()
        {
            /*
             * Start Location Trigger can be edited via the exposed, instance editable reference variable.
             * When that is triggered we call Move to Start Location
             */
            if (StartLocationTrigger != null && StartLocationTrigger.IsValid())
            {
                var Component =
                    StartLocationTrigger.GetComponentByClass(BP_InteractionComponent_C.StaticClass()) as
                        BP_InteractionComponent_C;

                Component.OnInteract.Add(this, OnInteractStartLocation);
            }

            /*
             * We do the same with End Location Trigger and Move to End Location.
             * Note this is done with the button in mind.
             */
            if (EndLocationTrigger != null && EndLocationTrigger.IsValid())
            {
                var Component =
                    EndLocationTrigger.GetComponentByClass(BP_InteractionComponent_C.StaticClass()) as
                        BP_InteractionComponent_C;

                Component.OnInteract.Add(this, OnInteractEndLocation);
            }

            Box.OnComponentBeginOverlap.Add(this, OnComponentBeginOverlap);
        }

        [Override]
        public override void ReceiveEndPlay(EEndPlayReason EndPlayReason)
        {
            if (StartLocationTrigger != null && StartLocationTrigger.IsValid())
            {
                var Component =
                    StartLocationTrigger.GetComponentByClass(BP_InteractionComponent_C.StaticClass()) as
                        BP_InteractionComponent_C;

                Component.OnInteract.RemoveAll(this);
            }

            if (EndLocationTrigger != null && EndLocationTrigger.IsValid())
            {
                var Component =
                    EndLocationTrigger.GetComponentByClass(BP_InteractionComponent_C.StaticClass()) as
                        BP_InteractionComponent_C;

                Component.OnInteract.RemoveAll(this);
            }

            Box.OnComponentBeginOverlap.RemoveAll(this);
        }

        private void OnInteractStartLocation(bool On)
        {
            MoveToStartLocation();
        }

        private void OnInteractEndLocation(bool On)
        {
            MoveToEndLocation();
        }

        /*
         * When the pawn overlaps, we move either to the start or end location
         */
        private void OnComponentBeginOverlap(UPrimitiveComponent OverlappedComponent, AActor OtherActor,
            UPrimitiveComponent OtherComp, int OtherBodyIndex, bool bFromSweep, FHitResult SweepResult)
        {
            if (IsAtEndLocation)
            {
                MoveToStartLocation();
            }
            else
            {
                MoveToEndLocation();
            }
        }

        private void MoveToStartLocation()
        {
            if (IsAtEndLocation)
            {
                ElevatorMovement.Reverse();
            }
        }

        private void MoveToEndLocation()
        {
            if (!IsAtEndLocation)
            {
                ElevatorMovement.Play();
            }
        }

        /*
         * A timeline is used to drive the elevator either to the start or end location.
         */
        [Override]
        private void ElevatorMovement__UpdateFunc()
        {
            var Height = ElevatorMovement.TheTimeline.InterpFloats[0].FloatCurve
                .GetFloatValue(ElevatorMovement.TheTimeline.Position);

            var NewLocation = UKismetMathLibrary.VLerp(StartLocation, EndLocation, Height);

            var SweepHitResult = new FHitResult();

            ElevatorMesh.K2_SetRelativeLocation(
                NewLocation,
                false,
                ref SweepHitResult,
                false);
        }

        /*
         * When finished we update the IsAtEndLocation boolean.
         */
        [Override]
        private void ElevatorMovement__FinishedFunc()
        {
            IsAtEndLocation = !IsAtEndLocation;
        }

        private bool IsAtEndLocation;
    }
}