using System;
using Script.Common;
using Script.CoreUObject;

namespace Script.Game.StackOBot.Blueprints.Framework
{
    [IsOverride]
    public partial class BPFL_InGame_C
    {
        [IsOverride]
        public static void GetMaxBots(UObject __WorldContext, out Int32 Max)
        {
            Max = 8;
        }
    }
}