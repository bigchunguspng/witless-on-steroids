using static Witlesss.Strings;

namespace Witlesss.Commands
{
    public class ToggleStickers : WitlessCommand
    {
        public override void Run()
        {
            Baka.DemotivateStickers = !Baka.DemotivateStickers;
            Bot.SaveChatList();
            Bot.SendMessage(Chat, string.Format(STICKERS_RESPONSE, Baka.DemotivateStickers ? "" : "<b>НЕ</b> "));
        }
    }
}