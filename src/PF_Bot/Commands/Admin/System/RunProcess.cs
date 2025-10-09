using System.Text;
using PF_Bot.Backrooms.Helpers;
using PF_Tools.ProcessRunning;
using Telegram.Bot.Extensions;

namespace PF_Bot.Commands.Admin.System;

public class RunProcess : CommandHandlerAsync_Admin
{
    protected override async Task RunAuthourized()
    {
        if (Args is null)
        {
            SendManual("<code>/run [exe] [args]</code>");
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
        if (stdout.IsNotNull_NorWhiteSpace())
        {
            sb.Append("<u>OUT</u>:\n<pre>").Append(HtmlText.Escape(stdout)).Append("</pre>");
        }
        if (stderr.IsNotNull_NorWhiteSpace())
        {
            sb.Append("<u>ERR</u>:\n<pre>").Append(HtmlText.Escape(stderr)).Append("</pre>");
        }

        return sb.ToString();
    }
}