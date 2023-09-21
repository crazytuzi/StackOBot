using System;
using Script.Common;
using Script.Engine;
using Script.Game.StackOBot.Blueprints.Framework;
using Script.Library;

namespace Script.Game.StackOBot.Blueprints.UI
{
    /*
     * This hud shows the amount of orbs the player has gathered and has a small hint when the game saves.
     * As the saving takes nearly no time it only triggeres a small animation that stay for a short time.
     * The widget also contains the BPW_Controls widget that has a hint what to press to spawn a new robot.
     * The player controler triggers it to change when it detects a gamepad or keyboard input.
     * But thats a bit hacky.
     * Check the comments in PC_InGame.
     */
    [IsOverride]
    public partial class BPW_HeadupDisplay_C
    {
        /*
         * Try to make the UI event driven and avoid binding if you can.
         * Here we get the coin amount from the instance at start and assign an update function to an event dispacther the instance fires when coins get updated.
         */
        [IsOverride]
        public override void Construct()
        {
            var GameInstance = UGameplayStatics.GetGameInstance(this);

            (GameInstance as IBPI_GameInstance_C).GetOrbAmount(out var Amount);

            SetCoinTxt(Amount);

            var GI_StackOBot = Unreal.Cast<GI_StackOBot_C>(GameInstance);

            GI_StackOBot?.OnCoinsUpdated.Add(this, OnCoinsUpdated);
        }

        [IsOverride]
        public override void Destruct()
        {
            var GameInstance = UGameplayStatics.GetGameInstance(this);

            var GI_StackOBot = Unreal.Cast<GI_StackOBot_C>(GameInstance);

            GI_StackOBot?.OnCoinsUpdated.RemoveAll(this);
        }

        private void OnCoinsUpdated(Int32 NewAmount = 0)
        {
            SetCoinTxt(NewAmount);
        }

        /*
         * Its a function as we call this from the init and from the OnCoinUpdated event.
         */
        private void SetCoinTxt(Int32 Amount = 0)
        {
            CointsAmount.SetText(UKismetTextLibrary.Conv_IntToText(Amount));
        }

        /*
         * The game mode calls this, when the game gets saved.
         * It is only playing a widget animation to show it and then it fades out
         */
        public void ShowSaveText()
        {
            PlayAnimation(ShowSaveTextAnimation);
        }
    }
}