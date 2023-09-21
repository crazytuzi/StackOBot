using System;
using System.Threading.Tasks;
using Script.Common;
using Script.CoreUObject;
using Script.Engine;
using Script.Game.StackOBot.Blueprints.Character;
using Script.Library;

namespace Script.Game.StackOBot.Blueprints.GameElements
{
    /*
     * As with the pressure plate, this button has the BP_Interaction Component implemented.
     * The player needs to press the interaction input button (E).
     * You could use it to also make a lever or another device the player can activate.
     */
    [IsOverride]
    public partial class BP_Button_C
    {
        [IsOverride]
        public override void UserConstructionScript()
        {
            var SourceMaterial =
                Unreal.LoadObject<UMaterialInstance>(this, "/Game/StackOBot/Environment/Materials/MI_Glow.MI_Glow");

            Mat = Button.CreateDynamicMaterialInstance(0, SourceMaterial);

            Mat.SetScalarParameterValue("Emissive", 0.0f);
        }

        [IsOverride]
        public override void ReceiveBeginPlay()
        {
            // @TODO
            UserConstructionScript();

            /*
             * Assign an event to the event dispatcher so when the player presses INTERACT the button gets pressed.
             */
            BP_InteractionComponent.OnInteract.Add(this, OnInteract);

            /*
             * Assign an event to the event dispatcher so when the interaction is possible show the widget component with the hint text
             */
            BP_InteractionComponent.OnToggleInteractability.Add(this, OnToggleInteraction);

            /*
             * Save the interaction location to the interaction component.
             * The animation BP will take that data and feed it into the control rig so the character blends into IK when they press the button.
             */
            BP_InteractionComponent.InteractionWorldLocation = IKLocation.K2_GetComponentLocation();

            InteractionArea.OnComponentBeginOverlap.Add(this, OnComponentBeginOverlap);

            InteractionArea.OnComponentEndOverlap.Add(this, OnComponentEndOverlap);
        }

        [IsOverride]
        public override void ReceiveEndPlay(EEndPlayReason EndPlayReason)
        {
            BP_InteractionComponent.OnInteract.RemoveAll(this);

            BP_InteractionComponent.OnToggleInteractability.RemoveAll(this);

            InteractionArea.OnComponentBeginOverlap.RemoveAll(this);

            InteractionArea.OnComponentEndOverlap.RemoveAll(this);
        }

        private void OnInteract(Boolean On)
        {
            PressButton();
        }

        private void OnToggleInteraction(Boolean On)
        {
            HintText.SetVisibility(On);
        }

        /*
         * The interaction area is a sphere with a custom collision.
         * It only overlaps with a pawn.
         * When it does, we start the check if the character is roughly facing this actor.
         * We cast here to the BP_Bot to play the anim montage on him when the button get's pressed.
         * This is a hard reference so when this blueprint is loaded it will also load the BP_Bot which will load more references.
         * You can use soft references or try to avoid casting like this.
         * This button is only ever loaded when BP_Bot is already in memory, but if you were to move it to the main menu level you would increase loading times and memory usages.
         */
        private void OnComponentBeginOverlap(UPrimitiveComponent OverlappedComponent, AActor OtherActor,
            UPrimitiveComponent OtherComp, Int32 OtherBodyIndex, Boolean bFromSweep, FHitResult SweepResult)
        {
            BP_InteractionComponent.StartCharacterOrientationCheck();

            var BP_Bot = Unreal.Cast<BP_Bot_C>(OtherActor);

            if (BP_Bot != null)
            {
                InteractingCharacter = BP_Bot;
            }
        }

        /*
         * When the character ends overlapping, stop the orientation check and clear the Interacting Character reference.
         */
        private void OnComponentEndOverlap(UPrimitiveComponent OverlappedComponent, AActor OtherActor,
            UPrimitiveComponent OtherComp, Int32 OtherBodyIndex)
        {
            BP_InteractionComponent.EndCharacterOrientationCheck();

            InteractingCharacter = null;
        }

        /*
         * Using a timeline to move the button and change it's emissive.
         * In construction script we created the dynamic material and saved the reference.
         */
        [IsOverride]
        private void ButtonAnimation__UpdateFunc()
        {
            var ButtonMovement = ButtonAnimation.TheTimeline.InterpFloats[0].FloatCurve
                .GetFloatValue(ButtonAnimation.TheTimeline.Position);

            Button.K2_SetRelativeLocation(
                new FVector
                {
                    X = 0.0f,
                    Y = 0.0f,
                    Z = ButtonMovement * -10.0f
                },
                false,
                out var SweepHitResult,
                false);

            Mat.SetScalarParameterValue("Emissive", ButtonMovement * 5.0f);
        }

        private async void Delay()
        {
            /*
             * We wait a bit before the button begins to move to sync the button movement and the animation.
             */
            await Task.Delay(200);

            ButtonAnimation.PlayFromStart();
        }

        private void PressButton()
        {
            /*
             * Tell the character to play the montage pressing the button.
             * That is in an upper body slot and has a curve that drives how much and when to blend into IK so he really reached for the button
             */
            var AnimMontage = Unreal.LoadObject<UAnimMontage>(this,
                "/Game/StackOBot/Characters/Bot/Animations/AM_Bot_Interact_PressButton.AM_Bot_Interact_PressButton");

            InteractingCharacter.PlayAnimMontage(AnimMontage);

            Delay();
        }
    }
}