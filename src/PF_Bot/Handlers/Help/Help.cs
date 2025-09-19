using PF_Bot.Backrooms.Helpers;
using PF_Bot.Handlers.Help.Helpers;
using PF_Bot.Routing_New.Routers;
using PF_Bot.Routing.Commands;

namespace PF_Bot.Handlers.Help;

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
        RTFM.SendManualPage(Query.GetOrigin(), Content, Query.GetMessageId());
        return Task.CompletedTask;
    }
}

public class Help : SyncCommand
{
    protected override void Run()
    {
        var args = Args ?? (Command!.Contains('_') ? Command.Substring(Command.IndexOf('_') + 1) : "");
        RTFM.SendManualPage(Origin, args);
    }
}