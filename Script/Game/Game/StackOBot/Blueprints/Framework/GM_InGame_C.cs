using System;
using Script.Common;
using Script.Engine;
using Script.Game.StackOBot.Blueprints.GameElements;
using Script.Library;

namespace Script.Game.StackOBot.Blueprints.Framework
{
    /*
     * In the game mode we mainly handle the player spawing including the spawn count, the max spawn count and giving the new robots an index.
     * The game mode also stores an array of spawn characters.
     * This is a singleplayer game.
     * Be aware in a multiplayer game, the game mode would only exist on the server, not the clients
     */
    [IsOverride]
    public partial class GM_InGame_C
    {
        /*
         * Initilize the save game handling in the game instance via an interface.
         * The reason for calling it her is the fact that the init event in the game instance is too early.
         * The world doesnt exist.
         * But we need the world context to do what we need to do in the save game.
         * Check the game instance for more details.
         */
        [IsOverride]
        public override void ReceiveBeginPlay()
        {
            // (UGameplayStatics.GetGameInstance(this) as IBPI_GameInstance_C)?.InitSaveGame();
        }

        /*
         * The same functionality as at game start (see below) can be triggered when the player "prints" a new robot.
         */
        [IsOverride]
        public void SpawnPlayerAtActiveSpawnPad()
        {
            StartingNewPlayer();
        }

        /*
         * Try to find a spawn pad to spawn our robots.
         */
        private BP_SpawnPad_C GetActiveSpawnPad()
        {
            /*
             * Do we have a valid reference. Then take that!
             */
            if (ActiveSpawnPad != null && ActiveSpawnPad.IsValid())
            {
                return ActiveSpawnPad;
            }
            else
            {
                /*
                 * If not, we check all actors in the scene.
                 * If that has the bool IsStartSpawnPad set to true, we save that as active.
                 * (if more than one has it set, the last in that list will be taken here)
                 */
                UGameplayStatics.GetAllActorsOfClass(this, BP_SpawnPad_C.StaticClass(), out var OutActors);

                foreach (var OutActor in OutActors)
                {
                    var BP_SpawnPad = Unreal.Cast<BP_SpawnPad_C>(OutActor);

                    if (BP_SpawnPad.IsStartSpawnPad)
                    {
                        ActiveSpawnPad = BP_SpawnPad;
                    }
                }

                /*
                 * If we found one, we take that, if not...
                 */
                if (ActiveSpawnPad != null && ActiveSpawnPad.IsValid())
                {
                    return ActiveSpawnPad;
                }

                /*
                 * If not we take the first one in the list, if there is none...
                 */
                if (OutActors.Num() > 0)
                {
                    ActiveSpawnPad = Unreal.Cast<BP_SpawnPad_C>(OutActors[0]);

                    return ActiveSpawnPad;
                }
                else
                {
                    /*
                     * We complain. The game will not be playable.
                     */
                    Console.WriteLine("No Spawnpad found!");

                    return null;
                }
            }
        }

        /*
         * This is an overwriteable function in game mode.
         * We do not want the exsiting framework to handle it but do something own here
         */
        [IsOverride]
        public override void HandleStartingNewPlayer(APlayerController NewPlayer)
        {
            StartingNewPlayer();
        }

        private void StartingNewPlayer()
        {
            /*
             * Spawn a new player at the active spawn pad.
             * Give him an index between 0 and max bots.
             * GetMaxBots is a pure library function we can call from anywhere.
             */
            var Max = BPFL_InGame_C.GetMaxBots();

            var SpawnPad = GetActiveSpawnPad();

            var Character = SpawnPad.SpawnPlayer(SpawnCounter % Max);

            /*
             * if the amount of spawn characters is >= the max amount robots we allow....
             */
            if (SpawndCharacters.Num() >= Max)
            {
                /*
                 * ...tell the first robot in the list to dissolve itself and remove him from our array of spawned characters
                 */
                var SpawndCharacter = SpawndCharacters[0];

                SpawndCharacter.Dissolve();

                SpawndCharacters.Remove(SpawndCharacter);
            }

            /*
             * ...always add the new spawned robot to the array and increase the spawn counter.
             */
            SpawndCharacters.AddUnique(Character);

            ++SpawnCounter;
        }

        /*
         * When a bot overlaps with a spawnpad, set that as the new active spawnpad and disable the previous one.
         */
        public void SetCurrentSpawnPad(BP_SpawnPad_C NewSpawnPad, out Boolean Success)
        {
            if (NewSpawnPad != null && NewSpawnPad.IsValid())
            {
                /*
                 * Disable old spawnpad
                 */
                ActiveSpawnPad.ToggleActivation();

                /*
                 * Set new spawnpad
                 */
                ActiveSpawnPad = NewSpawnPad;

                /*
                 * Tell the game instance to save the game data
                 */
                (UGameplayStatics.GetGameInstance(this) as IBPI_GameInstance_C)?.SaveGame();

                /*
                 * Tell the HUD that hot spawned in the player controller to display "saving" on the UI
                 */
                var PC_InGame = Unreal.Cast<PC_InGame_C>(UGameplayStatics.GetPlayerController(this, 0));

                PC_InGame.HeadupDisplay.ShowSaveText();

                /*
                 * Return that it was a success
                 */
                Success = true;
            }
            else
            {
                Success = false;
            }
        }

        private BP_SpawnPad_C ActiveSpawnPad;

        private Int32 SpawnCounter;
    }
}