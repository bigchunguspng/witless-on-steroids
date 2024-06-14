namespace Witlesss.Commands;

public class Tell : SyncCommand
{
    protected override void Run()
    {
        if (Message.From?.Id != Config.AdminID)
        {
            Bot.SendMessage(Chat, "LOL XD)0)");
            return;
        }

        var args = Args?.Split(' ', 2);
        if (args is null || args.Length < 2)
        {
            Bot.SendMessage(Chat, "<code>/tell [chat] [message]</code>");
            return;
        }
        
        var chat = long.TryParse(args[0], out var x) ? x : 0;
        if (chat != 0)
        {
            var text = args[1];
            Bot.SendMessage(chat, text, preview: false);
            if (Bot.WitlessExist(chat)) Bot.SussyBakas[chat].Eat(text);
        }
    }
}