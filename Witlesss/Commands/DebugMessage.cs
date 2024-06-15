using System.IO;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands
{
    public class DebugMessage : SyncCommand
    {
        protected override void Run()
        {
            if (Message.ReplyToMessage == null) return;

            var mess = Message.ReplyToMessage;
            var name = $"Message-{mess.MessageId}-{mess.Chat.Id}.json";
            var path = $@"{Paths.Dir_Temp}\{name}";
            Directory.CreateDirectory(Paths.Dir_Temp);
            new FileIO<Message>(path).SaveData(mess);
            using var stream = File.OpenRead(path);
            Bot.SendDocument(Chat, new InputOnlineFile(stream, name.Replace("--", "-")));
        }
    }
}