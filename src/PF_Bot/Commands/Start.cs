using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Routing.Commands;

namespace PF_Bot.Commands;

public class Start : CommandHandlerBlocking
{
    protected override void Run()
    {
        var success = ChatManager.TryAdd(Chat);
        if (success)
        {
            ChatManager.SaveChats();
            Log($"{Title} >> DIC CREATED >> {Chat}", LogLevel.Info, LogColor.Fuchsia);
            Bot.SendMessage(Origin, START_RESPONSE);
        }
        else
            SetBadStatus();
    }
}