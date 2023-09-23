using System;
using Script.Common;
using Script.Engine;
using Script.Game.StackOBot.Blueprints.GameElements;
using Script.Game.StackOBot.Blueprints.SaveGame;
using Script.Library;
using Script.MetasoundEngine;

namespace Script.Game.StackOBot.Blueprints.Framework
{
    /*
     * The game instance is persistent for the lifetime of the session, so it is a good place to store everything that need to persists even when traveling to new levels.
     * In this case we use it to play persitent music and to handle the save game functionality for the project.
     * Under class settings you will find the BPI_GameInstance interface.
     */
    [IsOverride]
    public partial class GI_StackOBot_C
    {
        /*
         * The GI knows the collected Orbs the player has.
         * This project might only have one level right now but in this way we can have a persitent inventory.
         * There is also a Event Dispatcher so the UI can react on a change instead of constantly pulling that information.
         */
        [IsOverride]
        public void UpdateOrbs(Int32 Change = 0)
        {
            CollectedOrbs += Change;

            OnCoinsUpdated.Broadcast(CollectedOrbs);
        }

        /*
         * On begin play, the UI checks how many orbs the player has.
         */
        [IsOverride]
        public void GetOrbAmount(out Int32 Amount)
        {
            Amount = CollectedOrbs;
        }

        /*
         * When player overlaps with a spawnpad this is called to dave the data to slots
         */
        [IsOverride]
        public void SaveGame()
        {
            SaveGameToSlots();
        }

        /*
         * Collectable Data is a struct we made for the save game data.
         * It contains the actor class and the transform of the actor in the world.
         * Here an orb gets removed from the save when collected.
         * Check InitLevelSaveData how it is made.
         */
        [IsOverride]
        public void RemoveCollectableFromSaveGame(CollectableObjectData CollectableData = null)
        {
            LevelSaveObject.RemoveCollectable(CollectableData);
        }

        /*
         * The GI at the "init" event doesnt have a world context yet.
         * But we need that to collect the level data.
         * For that reason the game mode calls this to initialize and load the player and map data.
         * In a multiplayer game you would do this different as the game mode would only run on the server.
         */
        [IsOverride]
        public void InitSaveGame()
        {
            InitPlayerSaveData();

            LoadPlayerSaveData();

            InitLevelSaveData();
        }

        /*
         * Called by a button press in the pause menu.
         * Check if a savegame exists, detele it and report back if the task was successful.
         */
        [IsOverride]
        public void ResetSaveGame(FString LevelName, out Boolean Success)
        {
            if (UGameplayStatics.DoesSaveGameExist(LevelName, 0))
            {
                var bDeleteLevelNameSuccess = UGameplayStatics.DeleteGameInSlot(LevelName, 0);

                var bDeleteSaveGameSlotNameSuccess = UGameplayStatics.DeleteGameInSlot(SaveGameSlotName, 0);

                Success = bDeleteLevelNameSuccess && bDeleteSaveGameSlotNameSuccess;
            }
            else
            {
                Success = false;
            }
        }

        /*
         * We wanted the music to play also during level transtion.
         * In our case from Main menu to game world and back.
         * As this is 2D UI music we decided to call it from the HUD via the interface
         */
        [IsOverride]
        public void PlayMusic(Double Volume = 1.000000)
        {
            /*
             * Create a 2D Sound with our Metasound asset when none is existing yet.
             */
            if (Music == null || !Music.IsValid())
            {
                var Sound = Unreal.LoadObject<UMetaSoundSource>(this, "/Game/StackOBot/Audio/SFX_Music.SFX_Music");

                Music = UGameplayStatics.CreateSound2D(this, Sound, (float) Volume, 1.0f, 0.0f, null, true);
            }

            /*
             * Start the Music if it is not playing already.
             */
            if (!Music.IsPlaying())
            {
                Music.Play();
            }
        }

        private void InitPlayerSaveData()
        {
            if (UGameplayStatics.DoesSaveGameExist(SaveGameSlotName, 0))
            {
                /*
                 * If a save game with SaveGameSlotName exists, load it, cast to the one we made and store that.
                 */
                var PlayerSaveObject =
                    Unreal.Cast<PlayerSaveObject_C>(UGameplayStatics.LoadGameFromSlot(SaveGameSlotName, 0));

                if (PlayerSaveObject != null)
                {
                    PlayerInventorySaveGame = PlayerSaveObject;
                }
            }
            else
            {
                /*
                 * If no save game with the SaveGameSlotName exists, create one.
                 */
                PlayerInventorySaveGame =
                    UGameplayStatics.CreateSaveGameObject(PlayerSaveObject_C.StaticClass()) as PlayerSaveObject_C;
            }
        }

        /*
         * The only data in our player inventory is the amount of orbs the bot collected.
         * We get that data and save it to a variable.
         */
        private void LoadPlayerSaveData()
        {
            CollectedOrbs = PlayerInventorySaveGame.Orbs;
        }

        private void SaveGameToSlots()
        {
            /*
             * First we update the "Orbs" variable in the save game object and then save that to disc.
             */
            PlayerInventorySaveGame.Orbs = CollectedOrbs;

            UGameplayStatics.SaveGameToSlot(PlayerInventorySaveGame, SaveGameSlotName, 0);

            /*
             * Then we save the level save object to disc.
             * It stores the Collectibles that are left on the map and we update it with "Remove Collectable From Save Game" that is called when an Orb is collected.
             */
            UGameplayStatics.SaveGameToSlot(LevelSaveObject, UGameplayStatics.GetCurrentLevelName(this), 0);
        }

        private void InitLevelSaveData()
        {
            /*
             * The order of calls matters.
             * As the game mode calls the InitSaveGame function, we can be sure the current level exists.
             * On Init of this game instance it would not.
             */
            var CurrentLevelName = UGameplayStatics.GetCurrentLevelName(this);

            if (UGameplayStatics.DoesSaveGameExist(CurrentLevelName, 0))
            {
                /*
                 * When a save game for this level exists, we store the object reference.
                 */
                var LoadLevelSaveObject =
                    Unreal.Cast<LevelSaveObject_C>(UGameplayStatics.LoadGameFromSlot(CurrentLevelName, 0));

                if (LoadLevelSaveObject != null)
                {
                    LevelSaveObject = LoadLevelSaveObject;
                }

                /*
                 * Next we iterate over all BP Energy Orbs in the level and remove them.
                 * You could have a parent class for all collectables and the Orb being a child of that if you want different collectables.
                 */
                UGameplayStatics.GetAllActorsOfClass(this, BP_EnergyOrb_C.StaticClass(), out var OutActors);

                foreach (var OutActor in OutActors)
                {
                    OutActor.K2_DestroyActor();
                }

                /*
                 * Next we take the level save object, get the array of collectables that are left on the map and spawn them.
                 * In this way we do not store any actor references that might change.
                 * This is an array of the struct we made containing the actor class and the transform in the world it was originally placed.
                 */
                LevelSaveObject.GetCollectablesOnMap(out var CollectablePickups);

                foreach (var CollectablePickup in CollectablePickups)
                {
                    GetWorld().SpawnActor<AActor>(CollectablePickup.ActorClass.Get(), CollectablePickup.Transform);
                }
            }
            else
            {
                /*
                 * If there is no savegame, we create one and store the object reference.
                 */
                LevelSaveObject =
                    UGameplayStatics.CreateSaveGameObject(LevelSaveObject_C.StaticClass()) as LevelSaveObject_C;

                /*
                 * Now we collect all BPEnergyOrb Actors and add them to our save game object.
                 * We use the struct that contains only the class and actor transform and no reference.
                 * Check above how the savegame is restored.
                 */
                UGameplayStatics.GetAllActorsOfClass(this, BP_EnergyOrb_C.StaticClass(), out var OutActors);

                foreach (var OutActor in OutActors)
                {
                    LevelSaveObject.AddCollectable(new CollectableObjectData
                    {
                        ActorClass = OutActor.GetClass(),
                        Transform = OutActor.GetTransform()
                    });
                }
            }
        }
    }
}