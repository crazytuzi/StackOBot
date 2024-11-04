using System.Threading;
using System.Threading.Tasks;
using Script.CoreUObject;
using Script.Engine;
using Script.Library;

namespace Script.Game.StackOBot.UI.MainMenu
{
    /*
     * As this is purely cosmetic and only in this main menu level the level blueprint is a nice and easy place to change the robots face every few seconds.
     * For gameplay you might want to look for a more flexible way that the actors can communicate with each other.
     * Like using interfaces, components or building managing objects for certain features.
     */
    [PathName("/Game/StackOBot/UI/MainMenu/MainMenu.MainMenu_C")]
    [Override]
    public partial class MainMenu_C : ALevelScriptActor, IStaticClass
    {
        public new static UClass StaticClass()
        {
            return StaticClassSingleton ??=
                UObjectImplementation.UObject_StaticClassImplementation(
                    "/Game/StackOBot/UI/MainMenu/MainMenu.MainMenu_C");
        }

        [Override]
        public override void ReceiveBeginPlay()
        {
            var OutActors = new TArray<AActor>();

            UGameplayStatics.GetAllActorsOfClass(this, ASkeletalMeshActor.StaticClass(), ref OutActors);

            if (OutActors.Num() > 0)
            {
                var SKM_Bot = OutActors[0] as ASkeletalMeshActor;

                BotFaceMaterial = SKM_Bot?.SkeletalMeshComponent.CreateDynamicMaterialInstance(1);

                TokenSource = new CancellationTokenSource();

                ChangeMood();
            }
        }

        [Override]
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

        public UMaterialInstanceDynamic BotFaceMaterial
        {
            get
            {
                unsafe
                {
                    var __ReturnBuffer = stackalloc byte[8];

                    FPropertyImplementation.FProperty_GetObjectPropertyImplementation(GarbageCollectionHandle,
                        __BotFaceMaterial, __ReturnBuffer);

                    return *(UMaterialInstanceDynamic*)__ReturnBuffer;
                }
            }

            set
            {
                unsafe
                {
                    var __InBuffer = stackalloc byte[8];

                    *(nint*)__InBuffer = value?.GarbageCollectionHandle ?? nint.Zero;

                    FPropertyImplementation.FProperty_SetObjectPropertyImplementation(GarbageCollectionHandle,
                        __BotFaceMaterial, __InBuffer);
                }
            }
        }

        private CancellationTokenSource TokenSource;

        private static UClass StaticClassSingleton { get; set; }

        private static uint __BotFaceMaterial = 0;
    }
}