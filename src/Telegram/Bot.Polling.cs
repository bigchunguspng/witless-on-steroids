using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Witlesss.Services.Sounds;

namespace Witlesss.Telegram;

public partial class Bot
{
    public static CommandAndCallbackRouter Router { get; private set; } = default!;
    public static InlineRequestHandler    Inliner { get; private set; } = new();

    private void StartListening()
    {
        var options = new ReceiverOptions
        {
            AllowedUpdates =
            [
                UpdateType.Message,
                UpdateType.EditedMessage,
                UpdateType.CallbackQuery,
                UpdateType.InlineQuery
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
            { InlineQuery:   { } inline  } => OnInline(inline),
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
            HandleError(e, "Callback");
        }

        return Task.CompletedTask;
    }

    private async Task OnInline(InlineQuery inline)
    {
        try
        {
            await Inliner.HandleRequest(inline);
        }
        catch (Exception e)
        {
            HandleError(e, "Inline");
        }
    }

    private Task OnUnknown()
    {
        Print("How Did We Get Here?", ConsoleColor.Magenta);
        return Task.CompletedTask;
    }

    public static void HandleCommandException(Exception e, CommandContext? context)
    {
        HandleError(e, context?.Title ?? "[unknown]");
    }

    private static void HandleError(Exception e, string context)
    {
        LogError($"{context} >> BRUH -> {e.GetFixedMessage()}");
        try
        {
            File.AppendAllText(File_Errors, $"[{DateTime.Now:MM'/'dd' 'HH:mm:ss.fff}]\n{e}\n\n");
        }
        catch
        {
            //
        }
    }
}