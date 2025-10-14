using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Routing.Commands;

namespace PF_Bot.Features_Aux.Settings.Commands
{
    public class ToggleStickers : CommandHandlerAsync_SettingsBlocking
    {
        protected override void RunAuthorized()
        {
            Data.Stickers = Data.Stickers.Janai();
            ChatManager.SaveChats();
            Bot.SendMessage(Origin, STICKERS_RESPONSE.Format(Data.Stickers ? "" : "<b>НЕ</b> ").XDDD());
            Log($"{Title} >> STICKERS >> {(Data.Stickers ? "ON" : "OFF")}");
        }
    }
}