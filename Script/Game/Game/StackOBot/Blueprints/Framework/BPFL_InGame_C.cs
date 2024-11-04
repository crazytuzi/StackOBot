using Script.CoreUObject;

namespace Script.Game.StackOBot.Blueprints.Framework
{
    [Override]
    public partial class BPFL_InGame_C
    {
        public static int GetMaxBots()
        {
            return 8;
        }
    }
}