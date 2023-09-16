using System;
using Script.Common;
using Script.Engine;
using Script.Game.StackOBot.Blueprints.Abilities;
using Script.Library;

namespace Script.Game.StackOBot.Blueprints.GameElements
{
    /*
     * The elevator is another example of how to use the triggers.
     * It has two locations and two triggers we mapped so the elevator goes between them.
     */
    [IsOverride]
    public partial class BP_Elevator_C
    {
        [IsOverride]
        public override void ReceiveBeginPlay()
        {
            /*
             * Start Location Trigger can be edited via the exposed, instance editable reference variable.
             * When that is triggered we call Move to Start Location
             */
            if (StartLocationTrigger != null && StartLocationTrigger.IsValid())
            {
                var Component =
                    Unreal.Cast<BP_InteractionComponent_C>(
                        StartLocationTrigger.GetComponentByClass(BP_InteractionComponent_C.StaticClass()));

                Component.OnInteract.Add(this, OnInteractStartLocation);
            }

            /*
             * We do the same with End Location Trigger and Move to End Location.
             * Note this is done with the button in mind.
             */
            if (EndLocationTrigger != null && EndLocationTrigger.IsValid())
            {
                var Component =
                    Unreal.Cast<BP_InteractionComponent_C>(
                        EndLocationTrigger.GetComponentByClass(BP_InteractionComponent_C.StaticClass()));

                Component.OnInteract.Add(this, OnInteractEndLocation);
            }

            Box.OnComponentBeginOverlap.Add(this, OnComponentBeginOverlap);

            ElevatorMovement.TheTimeline.TimelinePostUpdateFunc.Unbind();

            ElevatorMovement.TheTimeline.TimelinePostUpdateFunc.Bind(this, OnTimelinePostUpdateFunc);

            ElevatorMovement.TheTimeline.TimelineFinishedFunc.Unbind();

            ElevatorMovement.TheTimeline.TimelineFinishedFunc.Bind(this, TimelineFinishedFunc);
        }

        [IsOverride]
        public override void ReceiveEndPlay(EEndPlayReason EndPlayReason)
        {
            if (StartLocationTrigger != null && StartLocationTrigger.IsValid())
            {
                var Component =
                    Unreal.Cast<BP_InteractionComponent_C>(
                        StartLocationTrigger.GetComponentByClass(BP_InteractionComponent_C.StaticClass()));

                Component.OnInteract.RemoveAll(this);
            }

            if (EndLocationTrigger != null && EndLocationTrigger.IsValid())
            {
                var Component =
                    Unreal.Cast<BP_InteractionComponent_C>(
                        EndLocationTrigger.GetComponentByClass(BP_InteractionComponent_C.StaticClass()));

                Component.OnInteract.RemoveAll(this);
            }

            Box.OnComponentBeginOverlap.RemoveAll(this);

            ElevatorMovement.TheTimeline.TimelinePostUpdateFunc.Unbind();

            ElevatorMovement.TheTimeline.TimelineFinishedFunc.Unbind();
        }

        private void OnInteractStartLocation(Boolean On)
        {
            MoveToStartLocation();
        }

        private void OnInteractEndLocation(Boolean On)
        {
            MoveToEndLocation();
        }

        /*
         * When the pawn overlaps, we move either to the start or end location
         */
        private void OnComponentBeginOverlap(UPrimitiveComponent OverlappedComponent, AActor OtherActor,
            UPrimitiveComponent OtherComp, Int32 OtherBodyIndex, Boolean bFromSweep, FHitResult SweepResult)
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
        private void OnTimelinePostUpdateFunc()
        {
            var Height = ElevatorMovement.TheTimeline.InterpFloats[0].FloatCurve
                .GetFloatValue(ElevatorMovement.TheTimeline.Position);

            var NewLocation = UKismetMathLibrary.VLerp(StartLocation, EndLocation, Height);

            ElevatorMesh.K2_SetRelativeLocation(
                NewLocation,
                false,
                out var SweepHitResult,
                false);
        }

        /*
         * When finished we update the IsAtEndLocation boolean.
         */
        private void TimelineFinishedFunc()
        {
            IsAtEndLocation = !IsAtEndLocation;
        }
    }
}