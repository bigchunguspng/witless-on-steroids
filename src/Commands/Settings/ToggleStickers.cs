namespace Witlesss.Commands.Settings
{
    public class ToggleStickers : SettingsCommand
    {
        protected override void RunAuthorized()
        {
            Data.Stickers = !Data.Stickers;
            ChatService.SaveChatsDB();
            Bot.SendMessage(Chat, string.Format(STICKERS_RESPONSE, Data.Stickers ? "" : "<b>НЕ</b> ").XDDD());
            Log($"{Title} >> STICKERS >> {(Data.Stickers ? "ON" : "OFF")}");
        }
    }
}