using PF_Bot.Core;
using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Routing.Messages.Auto;
using PF_Bot.Routing.Messages.Commands;
using Telegram.Bot.Types.Enums;

namespace PF_Bot.Features_Main.Edit.Commands.Manual;

// todo have /auto as a command first, then adapt to an autohandler ?

public class Auto : CommandHandlerAsync
{
    protected override async Task Run()
    {
        var expression = Args ?? Settings.Options?[MemeType.Auto];
        if (expression == null || Message is { Type: MessageType.Text, ReplyToMessage: null })
        {
            SendManual(AUTO_MANUAL);
            return;
        }

        var repeats = _r_repeat.ExtractGroup(0, Options, int.Parse, 1);
        for (var i = 0; i < repeats; i++)
        {
            var input = AutoHandler.TryGetHandlerInput(Context, expression, Context.Message.ReplyToMessage, cache: false);
            if (input == null)
            {
                SendBadNews(AUTO_FAIL_TYPE.Format(FAIL_EMOJI.PickAny()));
                return;
            }

            var func = Registry.CommandHandlers.Resolve(input, out var command);
            if (func == null)
            {
                SendBadNews(PIPE_FAIL_RESOLVE.Format(input));
                return;
            }

            var context = CommandContext.CreateForAuto(Message, command!, input.TrimEnd(), CommandMode.AUTO);
            var handler = func.Invoke();

            await handler.Handle(context);
        }

        var suffix = repeats > 1 ? $"-{repeats}" : null;
        Log($"{Title} >> AUTO{suffix}", color: LogColor.Yellow);
    }

    private static readonly Regex
        _r_repeat = new("[2-9]", RegexOptions.Compiled);
}