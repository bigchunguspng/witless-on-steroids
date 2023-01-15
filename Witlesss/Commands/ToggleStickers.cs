namespace Witlesss.Commands
{
    public class ToggleStickers : ToggleAdmins
    {
        public override void Run()
        {
            if (SenderIsSus()) return;
            
            Baka.MemeStickers = !Baka.MemeStickers;
            Bot.SaveChatList();
            Bot.SendMessage(Chat, XD(string.Format(STICKERS_RESPONSE, Baka.MemeStickers ? "" : "<b>НЕ</b> ")));
        }
    }
}