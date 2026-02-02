using PF_Bot.Core;
using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Routing.Messages.Auto;
using PF_Bot.Routing.Messages.Commands;

namespace PF_Bot.Features_Main.Edit.Commands.Manual;

// todo have /auto as a command first, then adapt to an autohandler ?

public class Auto : CommandHandlerAsync
{
    protected override async Task Run()
    {
        var expression = Args ?? Settings.Options?[MemeType.Auto];
        if (expression == null)
        {
            Bot.SendMessage(Origin, "☝️ Установите автообработчик или передайте его аргументом. Справка - /man_341");
            return;
        }

        var repeats = _r_repeat.ExtractGroup(0, Options, int.Parse, 1);
        for (var i = 0; i < repeats; i++)
        {
            var input = AutoHandler.TryGetHandlerInput(Context, expression, Context.Message.ReplyToMessage, cache: false);
            if (input == null)
            {
                Bot.SendMessage(Origin, $"Не удалось распарсить ввод {FAIL_EMOJI.PickAny()}");
                return;
            }

            var func = Registry.CommandHandlers.Resolve(input, out var command);
            if (func == null)
            {
                Bot.SendMessage(Origin, PIPE_FAIL_RESOLVE.Format(input));
                return;
            }

            var context = CommandContext.CreateForAuto(Message, command!, input.TrimEnd(), CommandMode.AUTO);
            var handler = func.Invoke();

            await handler.Handle(context);
        }
    }

    private static readonly Regex
        _r_repeat = new("[2-9]", RegexOptions.Compiled);
}