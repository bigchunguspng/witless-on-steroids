using System;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Witlesss.Also;
using File = System.IO.File;

namespace Witlesss.Commands
{
    public class DebugMessage : Command
    {
        public override void Run()
        {
            if (Message.ReplyToMessage == null)
                return;
                    
            var mess = Message.ReplyToMessage;
            var name = $"Message-{mess.MessageId}-{mess.Chat.Id}.json";
            var path = $@"{Environment.CurrentDirectory}\{Strings.DEBUG_FOLDER}\{name}";
            Extension.CreatePath(path);
            new FileIO<Message>(path).SaveData(mess);
            using var stream = File.OpenRead(path);
            Bot.SendDocument(Chat, new InputOnlineFile(stream, name.Replace("--", "-")));
        }
    }
}