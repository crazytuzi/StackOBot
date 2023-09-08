using Script.Common;

namespace Script.Game.StackOBot.UI.PauseMenu
{
    [IsOverride]
    public partial class BPW_Button_Base_C
    {
        /*
         * This button is used on all menus so we can implement a consistend look and behaviour.
         * The label string is exposed and the menus can register to the OnClick event to implement the behaviour.
         */
        [IsOverride]
        public override void PreConstruct(bool IsDesignTime)
        {
            Label.SetText(Label_String);
        }
    }
}