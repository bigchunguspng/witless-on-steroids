namespace Witlesss.Commands
{
    public class GetChatID : Command
    {
        public override void Run()
        {
            Bot.SendMessage(Chat, Chat.ToString());
        }
    }
}