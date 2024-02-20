namespace Witlesss.Commands;

public class Tell : Command
{
    public override void Run()
    {
        if (Message.From?.Id != Config.AdminID)
        {
            Bot.SendMessage(Chat, "LOL XD)0)");
            return;
        }

        var split = Text.Split(' ', 3);
        if (split.Length < 3)
        {
            Bot.SendMessage(Chat, "<code>/tell [chat] [message]</code>");
            return;
        }
        
        var chat = long.TryParse(split[1], out var x) ? x : 0;
        if (chat != 0)
        {
            var text = split[2];
            Bot.SendMessage(chat, text, preview: false);
            if (Bot.SussyBakas.TryGetValue(chat, out var baka)) baka.Eat(text);
        }
    }
}