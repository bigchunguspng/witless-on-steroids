using PF_Bot.State.Chats;

namespace PF_Bot.Features.Manage.Settings
{
    public class ToggleStickers : SettingsCommand
    {
        protected override void RunAuthorized()
        {
            Data.Stickers = !Data.Stickers;
            ChatManager.SaveChatsDB();
            Bot.SendMessage(Origin, string.Format(STICKERS_RESPONSE, Data.Stickers ? "" : "<b>НЕ</b> ").XDDD());
            Log($"{Title} >> STICKERS >> {(Data.Stickers ? "ON" : "OFF")}");
        }
    }
}