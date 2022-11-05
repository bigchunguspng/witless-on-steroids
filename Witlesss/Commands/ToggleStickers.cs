namespace Witlesss.Commands
{
    public class ToggleStickers : ToggleAdmins
    {
        public override void Run()
        {
            if (SenderIsSus()) return;
            
            Baka.DemotivateStickers = !Baka.DemotivateStickers;
            Bot.SaveChatList();
            Bot.SendMessage(Chat, XD(string.Format(STICKERS_RESPONSE, Baka.DemotivateStickers ? "" : "<b>НЕ</b> ")));
        }
    }
}