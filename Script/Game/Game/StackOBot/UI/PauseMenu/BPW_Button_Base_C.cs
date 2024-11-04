using Script.CoreUObject;

namespace Script.Game.StackOBot.UI.PauseMenu
{
    [Override]
    public partial class BPW_Button_Base_C
    {
        /*
         * This button is used on all menus so we can implement a consistend look and behaviour.
         * The label string is exposed and the menus can register to the OnClick event to implement the behaviour.
         */
        [Override]
        public override void PreConstruct(bool IsDesignTime)
        {
            Label.SetText(Label_h20_String);
        }
    }
}