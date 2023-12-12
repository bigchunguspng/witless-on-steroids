namespace Witlesss.Commands
{
    public class ToggleStickers : SettingsCommand
    {
        protected override void ExecuteAuthorized()
        {
            Baka.Meme.Stickers = !Baka.Meme.Stickers;
            Bot.SaveChatList();
            Bot.SendMessage(Chat, XDDD(string.Format(STICKERS_RESPONSE, Baka.Meme.Stickers ? "" : "<b>НЕ</b> ")));
        }
    }
    
    public class ToggleColors : SettingsCommand
    {
        protected override void ExecuteAuthorized()
        {
            var c = Baka.Meme.Dye == ColorMode.Color;
            Baka.Meme.Dye = c ? ColorMode.White : ColorMode.Color;
            Bot.SaveChatList();
            Bot.SendMessage(Chat, XDDD(string.Format(COLORS_RESPONSE, c ? "белым" : "цветным")));
        }
    }
}