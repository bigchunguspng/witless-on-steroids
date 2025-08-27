using System.Text;
using PF_Bot.Backrooms.Helpers;
using PF_Bot.Routing.Commands;
using PF_Bot.Tools_Legacy.Technical;
using PF_Tools.Backrooms.Helpers;
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

        var process = ProcessRunner.StartReadableProcess(exe, args);
        await process.WaitForExitAsync();

        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();

        var sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(stdout))
        {
            sb.Append("<u>OUT</u>:\n");
            sb.Append("<pre>").Append(HtmlText.Escape(stdout)).Append("</pre>");
        }
        if (!string.IsNullOrWhiteSpace(stderr))
        {
            sb.Append("<u>ERR</u>:\n");
            sb.Append("<pre>").Append(HtmlText.Escape(stderr)).Append("</pre>");
        }

        Log($"{Title} >> RUN {exe.ToUpper()} {args}", color: LogColor.Yellow);
        Bot.SendMessage(Origin, sb.ToString());
    }
}