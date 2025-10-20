using PF_Bot.Core;
using PF_Bot.Routing.Messages.Commands;
using PF_Tools.ProcessRunning;

namespace PF_Bot.Routing.Messages.Auto;

public class AutoMeme : MessageHandler
{
    private HandlingStatus Status = HandlingStatus.OK;

    public async Task Run(Task makeMeme, CommandContext context)
    {
        Context = context;
        try
        {
            await makeMeme;
        }
        catch (Exception exception)
        {
            Status = HandlingStatus.FAIL;

            HandleError(exception, context);
        }
        finally
        {
            Log(context);
        }
    }

    private void HandleError(Exception exception, CommandContext context)
    {
        if (exception is ProcessException e)
        {
            Unluckies.HandleProcessException(e, context);
        }
        else if (exception is not FileTooBigException)
        {
            App.Bot.SendMessage(Origin, GetSillyErrorMessage());
            Unluckies.Handle(exception, context, $"AUTOMEMES | {Title}");
        }
    }

    private void Log(CommandContext context)
    {
        var settings = context.Settings;
        var options = settings.Options?[settings.Type];
        var args = context.Automemes_UseMessageText ? Text : null;
        var input = $"/{context.Command}{options} {args}";

        BigBrother.LogAuto(Chat, Status, Message, AutoType.MEME, settings.Pics, input);
    }
}