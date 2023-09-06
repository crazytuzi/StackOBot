using System.Threading;
using System.Threading.Tasks;
using Script.Common;
using Script.Engine;

namespace Script.Game.StackOBot.UI.MainMenu
{
    /*
     * As this is purely cosmetic and only in this main menu level the level blueprint is a nice and easy place to change the robots face every few seconds.
     * For gameplay you might want to look for a more flexible way that the actors can communicate with each other.
     * Like using interfaces, components or building managing objects for certain features.
     */
    [IsOverride]
    public partial class MainMenu_C
    {
        [IsOverride]
        public override void ReceiveBeginPlay()
        {
            UGameplayStatics.GetAllActorsOfClass(this, ASkeletalMeshActor.StaticClass(), out var OutActors);

            if (OutActors.Num() > 0)
            {
                var SKM_Bot = OutActors[0] as ASkeletalMeshActor;

                BotFaceMaterial = SKM_Bot?.SkeletalMeshComponent.CreateDynamicMaterialInstance(1);

                TokenSource = new CancellationTokenSource();

                ChangeMood();
            }
        }

        [IsOverride]
        public override void ReceiveEndPlay(EEndPlayReason EndPlayReason)
        {
            TokenSource.Cancel();
        }

        /*
         * Change the mood (check the face material) with a flipbook every few seconds
         */
        private async void ChangeMood()
        {
            while (!TokenSource.IsCancellationRequested)
            {
                BotFaceMaterial.SetScalarParameterValue("Mood", UKismetMathLibrary.RandomIntegerInRange(0, 14));

                await Task.Delay(UKismetMathLibrary.RandomIntegerInRange(3, 6) * 1000);
            }
        }

        private CancellationTokenSource TokenSource;
    }
}