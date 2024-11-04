using System.Threading;
using System.Threading.Tasks;
using Script.CoreUObject;
using Script.Engine;

namespace Script.Game.StackOBot.Blueprints.Abilities
{
    /*
     * This component can be added to triggers like BP_Button and provides some general functionality for player character interaction
     */
    [Override]
    public partial class BP_InteractionComponent_C
    {
        [Override]
        public override void ReceiveEndPlay(EEndPlayReason EndPlayReason)
        {
            TokenSource?.Cancel();
        }

        /*
         * Allows interaction and calls an event so you can react on enable
         */
        private void EnableInteractability()
        {
            Interactable = true;

            OnToggleInteractability.Broadcast(true);
        }

        /*
         * Disable interaction and calls an event so you can react on disable
         */
        private void DisableInteractability()
        {
            Interactable = true;

            OnToggleInteractability.Broadcast(false);
        }

        /*
         * Event to start the Interaction, check if interaction is possible, save the state and call OnInteract so others can react to it
         */
        public void StartInteraction()
        {
            if (Interactable)
            {
                Activated = !Activated;

                OnInteract.Broadcast(Activated);
            }
        }

        /*
         * Event to be able to stop an interaction, call OnInteract to react to that and save the state
         */
        public void StopInteraction()
        {
            OnInteract.Broadcast(false);

            Activated = false;
        }

        /*
         * Event to start an ongoing check if the character looks in the direction of the owning actor.
         * Mainly for the BP_Button. Enables and Disables Interactability and saves a Timer for the event-
         */
        public void StartCharacterOrientationCheck()
        {
            TokenSource = new CancellationTokenSource();

            CharacterOrientationCheck();
        }

        /*
         * Event to stop the ongoing check if the player is facing in the direction of the owning actor
         */
        public void EndCharacterOrientationCheck()
        {
            TokenSource?.Cancel();

            DisableInteractability();
        }

        private async void CharacterOrientationCheck()
        {
            while (!TokenSource.IsCancellationRequested)
            {
                /*
                 * Using a dot product to check if the character is roughly facing the actor
                 */

                if ((UGameplayStatics.GetPlayerPawn(this, 0).GetActorForwardVector() |
                     GetOwner().GetActorForwardVector()) <= -0.5f)
                {
                    /*
                     * Assure it is only called once
                     */
                    if (bDoEnable)
                    {
                        bDoEnable = false;

                        EnableInteractability();

                        bDoDisable = true;
                    }
                }
                else
                {
                    /*
                     * Assure it is only called once
                     */
                    if (bDoDisable)
                    {
                        bDoDisable = false;

                        DisableInteractability();

                        bDoEnable = true;
                    }
                }

                await Task.Delay(10);
            }
        }

        private CancellationTokenSource TokenSource;

        private bool bDoEnable = true;

        private bool bDoDisable = true;
    }
}