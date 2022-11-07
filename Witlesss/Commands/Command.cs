﻿using Telegram.Bot.Types;

namespace Witlesss.Commands
{
    public abstract class Command
    {
        protected Message Message;
        protected string Text, Title;
        protected long Chat;
        protected bool ChatIsPrivate => Chat > 0;
        
        public static Bot Bot;

        public void Pass(Message message)
        {
            Message = message;
            Text = message.Caption ?? message.Text;
            Chat = message.Chat.Id;
            Title = TitleOrUsername(message);
        }

        public abstract void Run();
    }

    public abstract class WitlessCommand : Command
    {
        protected Witless Baka;

        public void Pass(Witless witless) => Baka = witless;
    }
}