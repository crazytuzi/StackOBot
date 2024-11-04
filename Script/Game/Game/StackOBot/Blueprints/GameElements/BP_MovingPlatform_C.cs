using Script.CoreUObject;
using Script.ControlRig;
using Script.Engine;
using Script.Game.StackOBot.Blueprints.Abilities;

namespace Script.Game.StackOBot.Blueprints.GameElements
{
    /*
     * An InterpToMovment component is used to follow the spline that determines the platform path.
     * We wanted to give the moving platform a bit more juice, so we added a control rig component that has a verlet simulation to simulate the rotation of the platform depending on its movement.
     * And as with the other objects it can be (de)activated by a trigger that has the BP Interaction component implemented.
     */
    [Override]
    public partial class BP_MovingPlatform_C
    {
        [Override]
        public override void ReceiveBeginPlay()
        {
            /*
             * The InterpToMovement component needs control points
             */
            SetControlPoints();

            /*
             * Active can be set per instance
             */
            TogglePlatform(Active);

            if (Trigger != null && Trigger.IsValid())
            {
                var Component = Trigger.GetComponentByClass(BP_InteractionComponent_C.StaticClass()) as BP_InteractionComponent_C;

                Component.OnInteract.Add(this, OnInteract);
            }

            ControlRig.OnPreForwardsSolveDelegate.Add(this, OnPreForwardsSolve);

            ControlRig.OnPostForwardsSolveDelegate.Add(this, OnPostForwardsSolve);
        }

        [Override]
        public override void ReceiveEndPlay(EEndPlayReason EndPlayReason)
        {
            if (Trigger != null && Trigger.IsValid())
            {
                var Component = Trigger.GetComponentByClass(BP_InteractionComponent_C.StaticClass()) as BP_InteractionComponent_C;

                Component.OnInteract.RemoveAll(this);
            }

            ControlRig.OnPreForwardsSolveDelegate.RemoveAll(this);

            ControlRig.OnPostForwardsSolveDelegate.RemoveAll(this);
        }

        /*
         * OnInteract (here it assumes a bit it is a pressure plate used) the platform begins or stops to move.
         * For a full game you might want to move the platform back to it's initial positon.
         * We kept it simple for now.
         */
        private void OnInteract(bool On)
        {
            Active = On;

            TogglePlatform(Active);
        }

        /*
         * We start and stop the movement by toggling the component tick
         */
        private void TogglePlatform(bool bEnabled)
        {
            InterpToMovement.SetComponentTickEnabled(bEnabled);
        }

        /*
         * Here, we iterate over the spline points and add them as control points in the Interp component
         */
        private void SetControlPoints()
        {
            for (var i = 0; i < Spline.GetNumberOfSplinePoints() - 1; ++i)
            {
                InterpToMovement.AddControlPointPosition(
                    Spline.GetLocationAtSplinePoint(i, ESplineCoordinateSpace.Local));
            }

            /*
             * When completed, set the time the platform needs to move from start to end and call a finalise function.
             */
            InterpToMovement.Duration = (float)DurationBetweenEnds;

            InterpToMovement.FinaliseControlPoints();
        }

        /*
         * Event from the control rig component.
         * We feed our actor transform into the control rig on pre forward solve.
         * There we calculate the rotation with a verlet simulation.
         * Check the CR_Platform to see the caclulation.
         * This event is also updated in editor.
         */
        private void OnPreForwardsSolve(UControlRigComponent Component)
        {
            Component.SetBoneTransform("Anchor", GetTransform());
        }

        /*
         * In Post forward solve we get the calculated platform location and move our platform mesh that.
         * The result looks physics driven but is not.
         */
        private void OnPostForwardsSolve(UControlRigComponent Component)
        {
            var BoneTransform = Component.GetBoneTransform("Platform");

            var SweepHitResult = new FHitResult();

            PlatformMesh.K2_SetWorldLocationAndRotation(
                BoneTransform.GetLocation(),
                BoneTransform.GetRotation().Rotator(),
                true,
                ref SweepHitResult,
                false
            );
        }
    }
}