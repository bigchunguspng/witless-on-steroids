using System.Text;
using PF_Bot.Core;
using PF_Bot.Features_Aux.Listing;
using PF_Bot.Routing.Callbacks;
using PF_Bot.Routing.Messages.Commands;
using PF_Tools.ProcessRunning;
using Telegram.Bot.Extensions;

namespace PF_Bot.Commands.Admin.System;

public class RunProcess_Callback : CallbackHandler
{
    protected override Task Run()
    {
        ProcessOutputSender.SendOutputPage(GetPagination(Content));
        return Task.CompletedTask;
    }
}

public class RunProcess : CommandHandlerAsync_Admin
{
    protected override async Task Run()
    {
        if (Args is null)
        {
            SendManual(MANUAL);
            return;
        }

        string exe, args;
        if      (Options.Contains('b'))
        {
            exe  = "bash";
            args = $"-c \"{Args}\"";
        }
        else if (Options.Contains('c'))
        {
            exe  = "cmd";
            args = $"/c \"{Args}\"";
        }
        else
        {
            var    bits = Args.SplitN(2);
            exe  = bits[0];
            args = bits.Length > 1 ? bits[1] : "";
        }

        if (Options.Contains('-')) // don't wait
        {
            var process = ProcessStarter.InitProcess(exe, args, listen: false);
            process.StartInfo.UseShellExecute = true;
            process.Start();

            await Task.Delay(100); // (to mark command processed)

            if (Options.Contains('x')) // exit
                Environment.Exit(8);
        }
        else
        {
            var sw = Stopwatch.StartNew();
            var (stdout, stderr, code) = await ProcessRunner.Run_GetOutput(exe, args);
            var time = sw.Elapsed;

            ProcessOutputSender.SendProcessOutput(Origin, stdout, stderr, code, time);
        }

        Log($"{Title} >> RUN {exe} {args}", color: LogColor.Yellow);
    }

    private const string MANUAL =
        """
        <code>/run [exe] [args]</code>

        <code>/runb  [bash command]</code>
        <code>/runc  [cmd  command]</code>
        <code>/run-  [exe args]</code> - launch & forget
        <code>/run-x [exe args]</code> - launch & stop bot
        """;
}

public static class ProcessOutputSender
{
    public static void SendProcessOutput
        (MessageOrigin origin, string? stdout, string? stderr, int code, TimeSpan time = default)
    {
        var output = FormatProcessOutputs(stdout, stderr, code, time);
        var max_page_digits = (1 + output.Length / 4000).Digits(); // <-- rough page count is used
        var offset = 4 + 2 * max_page_digits; // "📃1918/1919\n"
        var output_pages = output.SplitIntoPages(offset_paginated: offset, headBodySeparator: "");
        if (output_pages.Length > 1)
        {
            int id;
            lock (App.ProcessOutputs)
            {
                id = App.ProcessOutputs.Count;
                App.ProcessOutputs.Add(output_pages);
            }
            SendOutputPage(new ListPagination(origin, perPage: 1, extra: id));
        }
        else
            App.Bot.SendMessage(origin, output);
    }

    private static string FormatProcessOutputs(string? stdout, string? stderr, int exitCode, TimeSpan time)
    {
        var sb = new StringBuilder();
        if (time != default)                  sb.Append($"<u>TIME</u>: <code>{time.ReadableTime()}</code>\n");
        _ =                                   sb.Append($"<u>EXIT CODE</u>: <code>{exitCode}</code>\n");
        if (stdout.IsNotNull_NorWhiteSpace()) sb.Append($"<u>OUT</u>:\n<pre>{HtmlText.Escape(stdout)}</pre>");
        if (stderr.IsNotNull_NorWhiteSpace()) sb.Append($"<u>ERR</u>:\n<pre>{HtmlText.Escape(stderr)}</pre>");
        return sb.ToString();
    }

    public static void SendOutputPage(ListPagination pagination)
    {
        var id = pagination.Extra;
        new PaginatedList<string>(App.ProcessOutputs[id], Registry.CallbackKey_Runs, pagination)
        {
            ItemText = (sb, page) => sb.Append(page),
            ShowArrowsTip = false,
            HeadSeparator = "\n",
        }.Send();
    }
}