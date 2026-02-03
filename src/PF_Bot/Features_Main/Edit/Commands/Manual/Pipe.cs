using System.Text;
using PF_Bot.Core;
using PF_Bot.Routing.Messages.Commands;
using Telegram.Bot.Types;

namespace PF_Bot.Features_Main.Edit.Commands.Manual;

// /pipe meme3 > nuke > im o4:4! .

public class Pipe : CommandHandlerAsync
{
    protected override async Task Run()
    {
        if (Args != null)
        {
            var pipe = Args.Split(">").Select(x => x.Trim()).ToArray();

            var plumber = new PipeExecutionTask(pipe.Length);

            if (plumber.LayPipe(pipe, Message, out var i).Failed())
            {
                SendBadNews(PIPE_FAIL_RESOLVE.Format(pipe[i].SplitN(2)[0]));
                return;
            }

            var sw = Stopwatch.StartNew();

            await plumber.TraversePipe();

            var log_args = string.Join(" > ", pipe).Replace("\n", "[N]");
            Log($"{Title} >> PIPE [{log_args}] >> {sw.ElapsedReadable()}", color: LogColor.Yellow);
        }
        else
            SendManual(EDIT_MANUAL_SYN.Format("🎬, 📸, 🎧, 📎", "/man_pipe"));
    }
}

public class PipeExecutionTask(int length)
{
    private static readonly CommandRegistry<Func<CommandHandler>> registry = Registry.CommandHandlers;

    private readonly Func<CommandHandler>[] handlers = new Func<CommandHandler>[length];
    private readonly      CommandContext [] contexts = new      CommandContext [length];

    public bool LayPipe(string[] pipe, Message message, out int i)
    {
        for (i = 0; i < length; i++)
        {
            var input = pipe[i];
            if (registry.Resolve(input, out var command) is { } handler)
            {
                var context = CommandContext.CreateForAuto(message, command!, input, CommandMode.PIPE);
                handlers[i] = handler;
                contexts[i] = context;
            }
            else
                return false;
        }

        return true;
    }

    public async Task TraversePipe(int i = 0, FilePath? input = null)
    {
        var last = i == length - 1;

        var basket  = new List<FilePath>();
        var handler = handlers[i].Invoke();
        var context = contexts[i];

        if (input != null)
            context.Input = input;

        if (last.Janai())
            context.Output = basket;

        LogPipeSection(context, input);

        await handler.Handle(context);

        if (last) return;

        foreach (var file in basket)
        {
            await TraversePipe(i + 1, file);
        }
    }

    private void LogPipeSection(CommandContext ctx, FilePath? input)
    {
        var sb = new StringBuilder("PIPE >> /").Append(ctx.Command);
        if (ctx.Options != null) sb.Append($"[{ctx.Options}]");
        if (ctx.Args    != null) sb.Append($" [{ctx.Args}]");
        if (input       != null) sb.Append($" INPUT: {input}");

        LogDebug(sb.ToString());
    }
}