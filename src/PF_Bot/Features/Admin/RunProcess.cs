using System.Text;
using PF_Bot.Backrooms.Helpers;
using PF_Bot.Routing.Commands;
using PF_Bot.Tools_Legacy.Technical;
using PF_Tools.Backrooms.Helpers.ProcessRunning;
using Telegram.Bot.Extensions;

namespace PF_Bot.Features.Admin;

public class RunProcess : AsyncCommand
{
    protected override async Task Run()
    {
        if (!Message.SenderIsBotAdmin())
        {
            Bot.SendMessage(Origin, FORBIDDEN.PickAny());
            return;
        }

        if (Args is null)
        {
            Bot.SendMessage(Origin, "<code>/run [exe] [args]</code>");
            return;
        }

        var split = Args.SplitN(2);
        var exe = split[0];
        var args = split.Length > 1 ? split[1] : "";

        var (stdout, stderr) = await ProcessRunner.Run_GetOutput(exe, args);

        Log($"{Title} >> RUN {exe} {args}", color: LogColor.Yellow);
        Bot.SendMessage(Origin, FormatProcessOutputs(stdout, stderr));
    }

    private static string FormatProcessOutputs(string stdout, string stderr)
    {
        var sb = new StringBuilder();
        if (string.IsNullOrWhiteSpace(stdout) == false)
        {
            sb.Append("<u>OUT</u>:\n<pre>").Append(HtmlText.Escape(stdout)).Append("</pre>");
        }
        if (string.IsNullOrWhiteSpace(stderr) == false)
        {
            sb.Append("<u>ERR</u>:\n<pre>").Append(HtmlText.Escape(stderr)).Append("</pre>");
        }

        return sb.ToString();
    }
}