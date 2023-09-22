using System;
using Script.Common;
using Script.Engine;
using Script.Game.StackOBot.Blueprints.Abilities;
using Script.Library;

namespace Script.Game.StackOBot.Blueprints.GameElements
{
    /*
     * A lamp can have several triggers and are used as feedback for the player.
     * For example for a door, when more than one trigger is needed to indicate that a needed trigger like a pressure plate is active.
     * If no tigger is set, the lamp is always switched on.
     */
    [IsOverride]
    public partial class BP_Lamp_C
    {
        [IsOverride]
        public override void UserConstructionScript()
        {
            LampGlowMaterial = LampMesh.CreateDynamicMaterialInstance(1);
        }

        [IsOverride]
        public override void ReceiveBeginPlay()
        {
            // @TODO
            UserConstructionScript();

            /*
             * If the level designer did not add any triggers to the array, the lamp will be switched on by default.
             * If there are triggers it is switched off instead.
             */
            if (Triggers.Num() > 0)
            {
                SwitchOff();

                foreach (var Trigger in Triggers)
                {
                    var Component =
                        Unreal.Cast<BP_InteractionComponent_C>(
                            Trigger.GetComponentByClass(BP_InteractionComponent_C.StaticClass()));

                    Component?.OnInteract.Add(this, OnInteract);
                }
            }
            else
            {
                SwitchOn();
            }
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
        }

        /*
         * This setup is the same as the door.
         * Check there for more details.
         * If you use identical behavior across different actors, consider moving it to its own component.
         */
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
                    SwitchOn();
                }
            }
            else
            {
                /*
                 * On Interact "off" of one of the components in the array, we decrease the amount of active triggers and close the door
                 */
                --TriggersActive;

                TriggersActive = Math.Max(TriggersActive, 0);

                SwitchOff();
            }
        }

        /*
         * The Dynamic Material reference for the lamp was saved in the construction script.
         * The emmissive parameter is changed when it is switched on.
         */
        private void SwitchOn()
        {
            LampGlowMaterial.SetScalarParameterValue("Emissive", 5.0f);
        }

        /*
         * The Dynamic Material reference for the lamp was saved in the construction script.
         * The emmissive parameter is changed when it is switched off.
         */
        private void SwitchOff()
        {
            LampGlowMaterial.SetScalarParameterValue("Emissive", 0.0f);
        }

        private Int32 TriggersActive;
    }
}