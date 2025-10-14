using System.Text;
using PF_Bot.Routing;
using PF_Bot.Routing.Commands;
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
                Status = CommandResultStatus.BAD;
                Bot.SendMessage(Origin, string.Format(PIPE_FAIL_RESOLVE, pipe[i].SplitN(2)[0]));
                return;
            }

            var sw = Stopwatch.StartNew();

            await plumber.TraversePipe();

            Log($"{Title} >> PIPE [{Args}] >> {sw.ElapsedReadable()}", color: LogColor.Yellow);
        }
        else
            SendManual(string.Format(EDIT_MANUAL_SYN, "🎬, 📸, 🎧, 📎", "/man_pipe"));
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
            var section = pipe[i];
            if (registry.Resolve(section, out var command) is { } handler)
            {
                var context = new CommandContext(message, command!, section);
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