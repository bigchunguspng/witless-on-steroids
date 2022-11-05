using System.IO;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands
{
    public class DebugMessage : Command
    {
        public override void Run()
        {
            if (Message.ReplyToMessage == null) return;

            var mess = Message.ReplyToMessage;
            var name = $"Message-{mess.MessageId}-{mess.Chat.Id}.json";
            var path = $@"{TEMP_FOLDER}\{name}";
            Directory.CreateDirectory(TEMP_FOLDER);
            new FileIO<Message>(path).SaveData(mess);
            using var stream = File.OpenRead(path);
            Bot.SendDocument(Chat, new InputOnlineFile(stream, name.Replace("--", "-")));
        }
    }
}