namespace Witlesss.Commands
{
    public class ToggleStickers : ToggleAdmins
    {
        public override void Run()
        {
            if (SenderIsSus()) return;
            
            Baka.Meme.Stickers = !Baka.Meme.Stickers;
            Bot.SaveChatList();
            Bot.SendMessage(Chat, XD(string.Format(STICKERS_RESPONSE, Baka.Meme.Stickers ? "" : "<b>НЕ</b> ")));
        }
    }
    
    public class ToggleColors : ToggleAdmins
    {
        public override void Run()
        {
            if (SenderIsSus()) return;

            var c = Baka.Meme.Dye == ColorMode.Color;
            Baka.Meme.Dye = c ? ColorMode.White : ColorMode.Color;
            Bot.SaveChatList();
            Bot.SendMessage(Chat, XD(string.Format(COLORS_RESPONSE, c ? "белым" : "цветным")));
        }
    }
}