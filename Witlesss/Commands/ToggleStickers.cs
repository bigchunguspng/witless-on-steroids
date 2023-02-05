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
}