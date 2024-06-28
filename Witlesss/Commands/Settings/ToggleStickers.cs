namespace Witlesss.Commands.Settings
{
    public class ToggleStickers : SettingsCommand
    {
        protected override void RunAuthorized()
        {
            Baka.Meme.Stickers = !Baka.Meme.Stickers;
            ChatsDealer.SaveChatList();
            Bot.SendMessage(Chat, string.Format(STICKERS_RESPONSE, Baka.Meme.Stickers ? "" : "<b>НЕ</b> ").XDDD());
        }
    }
}