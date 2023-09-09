using System;
using Script.Common;
using Script.CoreUObject;
using Script.Engine;
using Script.SlateCore;

namespace Script.Game.StackOBot.UI.LoadingScreen
{
    [IsOverride]
    public partial class BPW_LoadingScreen_C
    {
        [IsOverride]
        public override void Tick(FGeometry MyGeometry, float InDeltaTime)
        {
            if (UKismetMathLibrary.NearlyEqual_FloatFloat(ColorAndOpacity.A, TargetAlpha, 0.0001))
            {
                SetColorAndOpacity(new FLinearColor
                {
                    R = 1.0f,
                    G = 1.0f,
                    B = 1.0f,
                    A = (float)TargetAlpha
                });

                if (bEnabled)
                {
                    DoneFadingEvent.Broadcast();

                    bEnabled = false;
                }
            }
            else
            {
                SetColorAndOpacity(new FLinearColor
                {
                    R = 1.0f,
                    G = 1.0f,
                    B = 1.0f,
                    A = (float)UKismetMathLibrary.FInterpTo_Constant(ColorAndOpacity.A, TargetAlpha, InDeltaTime, 1.0)
                });

                bEnabled = true;
            }
        }

        [IsOverride]
        public void SetFade(Double TargetAlpha = 0)
        {
            this.TargetAlpha = TargetAlpha;
        }

        private Boolean bEnabled = true;
    }
}