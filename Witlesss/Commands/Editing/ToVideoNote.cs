﻿using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands.Editing
{
    public class ToVideoNote : VideoCommand
    {
        public override void Run()
        {
            if (NoVideo()) return;
            
            Bot.Download(FileID, Chat, out string path);
            
            using var stream = File.OpenRead(Memes.ToVideoNote(path));
            Bot.SendVideoNote(Chat, new InputOnlineFile(stream));
            Log($"{Title} >> NOTE (*)");
        }
    }
}