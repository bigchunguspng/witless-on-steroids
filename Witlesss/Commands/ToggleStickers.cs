namespace Witlesss.Commands
{
    public class ToggleStickers : SettingsCommand
    {
        protected override void RunAuthorized()
        {
            Baka.Meme.Stickers = !Baka.Meme.Stickers;
            Bot.SaveChatList();
            Bot.SendMessage(Chat, XDDD(string.Format(STICKERS_RESPONSE, Baka.Meme.Stickers ? "" : "<b>НЕ</b> ")));
        }
    }
}