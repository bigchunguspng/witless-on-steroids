using Witlesss.Also;

namespace Witlesss.Commands
{
    public class ChatInfo : WitlessCommand
    {
        public override void Run()
        {
            string info = $"<b>{Title}</b>\n\nВес словаря: {Extension.FileSize(Baka.Path)}" +
                          $"\nИнтервал генерации: {Baka.Interval}\nВероятность демотивации: {Baka.DgProbability}%" +
                          $"\nДемотивация стикеров: {(Baka.DemotivateStickers? "ON" : "OFF")}";
            Bot.SendMessage(Chat, info);
        }
    }
}