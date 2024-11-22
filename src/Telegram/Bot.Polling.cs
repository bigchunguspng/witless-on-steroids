using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Witlesss.Telegram;

public partial class Bot
{
    public static CommandAndCallbackRouter Router { get; private set; } = default!;

    private void StartListening()
    {
        var options = new ReceiverOptions
        {
            AllowedUpdates =
            [
                UpdateType.Message,
                UpdateType.EditedMessage,
                UpdateType.CallbackQuery
            ]
        };

        Client.StartReceiving(HandleUpdate, HandlePollingError, options);
        Print(string.Format(BUENOS_DIAS, Username, Me.FirstName), ConsoleColor.Yellow);
    }

    private Task HandleUpdate(ITelegramBotClient bot, Update update, CancellationToken token)
    {
        return update switch
        {
            { Message:       { } message } => OnMessage(message),
            { EditedMessage: { } message } => OnMessage(message),
            { CallbackQuery: { } query   } => OnCallback(query),
            _ => OnUnknown()
        };
    }

    private Task HandlePollingError(ITelegramBotClient bot, Exception exception, CancellationToken token)
    {
        LogError($"Telegram API Error x_x --> {exception.Message}\n{exception.StackTrace}");
        return Task.CompletedTask;
    }

    private Task OnMessage(Message message)
    {
        try
        {
            Router.Execute(CommandContext.FromMessage(message));
        }
        catch (Exception e)
        {
            HandleCommandException(e, Router.Context);
        }

        return Task.CompletedTask;
    }

    private Task OnCallback(CallbackQuery query)
    {
        try
        {
            Router.OnCallback(query);
        }
        catch (Exception e)
        {
            LogError($"Callback >> BRUH -> {e.GetFixedMessage()}");
        }

        return Task.CompletedTask;
    }

    private Task OnUnknown()
    {
        Print("How Did We Get Here?", ConsoleColor.Magenta);
        return Task.CompletedTask;
    }

    public static void HandleCommandException(Exception e, CommandContext? context)
    {
        var title = context?.Title ?? "[unknown]";
        LogError($"{title} >> BRUH -> {e.GetFixedMessage()}");
    }
}