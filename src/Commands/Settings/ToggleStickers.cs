namespace Witlesss.Commands.Settings
{
    public class ToggleStickers : SettingsCommand
    {
        protected override void RunAuthorized()
        {
            Baka.Stickers = !Baka.Stickers;
            ChatService.SaveChatsDB();
            Bot.SendMessage(Chat, string.Format(STICKERS_RESPONSE, Baka.Stickers ? "" : "<b>НЕ</b> ").XDDD());
        }
    }
}