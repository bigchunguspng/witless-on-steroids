using PF_Bot.Commands;
using PF_Bot.Features_Aux.Settings.Core;

namespace PF_Bot.Features_Aux.Settings.Commands
{
    public class ToggleStickers : SettingsCommand
    {
        protected override void RunAuthorized()
        {
            Data.Stickers = Data.Stickers.Janai();
            ChatManager.SaveChats();
            Bot.SendMessage(Origin, string.Format(STICKERS_RESPONSE, Data.Stickers ? "" : "<b>НЕ</b> ").XDDD());
            Log($"{Title} >> STICKERS >> {(Data.Stickers ? "ON" : "OFF")}");
        }
    }
}