namespace Witlesss.Commands
{
    public class ToggleStickers : WitlessCommand
    {
        public override void Run()
        {
            Baka.DemotivateStickers = !Baka.DemotivateStickers;
            Bot.SaveChatList();
            Bot.SendMessage(Chat, $"Стикеры {(Baka.DemotivateStickers ? "" : "<b>НЕ</b> ")}будут демотивироваться в случайном порядке");
        }
    }
}