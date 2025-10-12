using PF_Bot.Features_Aux.Help.Helpers;
using PF_Bot.Routing.Callbacks;
using PF_Bot.Routing.Commands;

namespace PF_Bot.Features_Aux.Help.Commands;

// /help == /man
// /help        -> main menu
// /help ffmpeg -> page exist ? navigated to page : ^
// /help 44     -> ^
// /help_44     -> ^
// man - 44     -> ^ (ffmpeg)

public class Help_Callback : CallbackHandler
{
    protected override Task Run()
    {
        RTFM.SendManualPage(Origin, Content, Message.Id);
        return Task.CompletedTask;
    }
}

public class Help : CommandHandlerBlocking
{
    protected override void Run()
    {
        var args = Args ?? (Options.IndexOf('_') is var i and >= 0 ? Options.Substring(i + 1) : "");
        RTFM.SendManualPage(Origin, args);
        Log($"{Title} >> MAN {args}");
    }
}