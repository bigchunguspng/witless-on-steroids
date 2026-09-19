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

        var (stdout, stderr, code) = await ProcessRunner.Run_GetOutput(exe, args);

        ProcessOutputSender.SendProcessOutput(Origin, stdout, stderr, code);

        Log($"{Title} >> RUN {exe} {args}", color: LogColor.Yellow);
    }

    private const string MANUAL =
        """
        <code>/run [exe] [args]</code>

        <code>/runb [bash command]</code>
        <code>/runc [cmd  command]</code>
        """;
}

public static class ProcessOutputSender
{
    public static void SendProcessOutput
        (MessageOrigin origin, string? stdout, string? stderr, int code)
    {
        var output = FormatProcessOutputs(stdout, stderr, code);
        var output_pages = output.SplitIntoPages();
        if (output_pages.Length > 1)
        {
            int id;
            lock (App.ProcessOutputs)
            {
                id = App.ProcessOutputs.Count;
                App.ProcessOutputs.Add(output_pages);
            }
            SendOutputPage(new ListPagination(origin, PerPage: id));
        }
        else
            App.Bot.SendMessage(origin, output);
    }

    private static string FormatProcessOutputs(string? stdout, string? stderr, int exitCode)
    {
        var sb = new StringBuilder                     ($"<u>EXIT CODE</u>: <code>{exitCode}</code>\n");
        if (stdout.IsNotNull_NorWhiteSpace()) sb.Append($"<u>OUT</u>:\n<pre>{HtmlText.Escape(stdout)}</pre>");
        if (stderr.IsNotNull_NorWhiteSpace()) sb.Append($"<u>ERR</u>:\n<pre>{HtmlText.Escape(stderr)}</pre>");
        return sb.ToString();
    }

    public static void SendOutputPage(ListPagination pagination)
    {
        var (origin, messageId, page, _) = pagination;
        var id = pagination.PerPage;
        var pages = App.ProcessOutputs[id];
        var last_page = pages.Length - 1;
        var buttons = pagination.GetPaginationKeyboard(last_page, Registry.CallbackKey_Runs);
        App.Bot.SendOrEditMessage(origin, pages[page], messageId, buttons);
    }
}