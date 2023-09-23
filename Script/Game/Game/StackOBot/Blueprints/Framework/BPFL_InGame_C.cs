using System;
using Script.Common;

namespace Script.Game.StackOBot.Blueprints.Framework
{
    [IsOverride]
    public partial class BPFL_InGame_C
    {
        public static Int32 GetMaxBots()
        {
            return 8;
        }
    }
}