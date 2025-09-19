using System.Text.Json;
using PF_Bot.Core;
using PF_Bot.Handlers.Help;
using PF_Bot.Routing_New.Routers;
using PF_Bot.Routing.Commands;
using PF_Bot.Routing.Inline;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PF_Bot.Telegram;

public partial class Bot
{
    public static SyncCommand     Router_Command  { get; private set; } = null!;
    public static ICallbackRouter Router_Callback { get; private set; } = null!;
    public static InlineRequestHandler    Inliner { get; private set; } = new();

    public void StartListening()
    {
        var options = new ReceiverOptions
        {
            AllowedUpdates =
            [
                UpdateType.Message,
                UpdateType.EditedMessage,
                UpdateType.CallbackQuery,
                UpdateType.InlineQuery,
            ],
        };

        Client.StartReceiving(HandleUpdate, HandlePollingError, options);
        Telemetry.Log_START(Me);
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
            _ => OnUnknown(),
        };
    }

    // todo: moving average and dynamic delay: 1s -> 5s -> 15s
    private async Task HandlePollingError(ITelegramBotClient bot, Exception exception, CancellationToken token)
    {
        LogError($"Telegram API | {exception.GetErrorMessage()}");

        await Task.Delay(1000, token);
    }

    private Task OnMessage(Message message)
    {
        try
        {
            Router_Command.Execute(CommandContext.FromMessage(message));
        }
        catch (Exception e)
        {
            HandleCommandException(e, Router_Command.Context);
        }

        return Task.CompletedTask;
    }

    private Task OnCallback(CallbackQuery query)
    {
        try
        {
            Router_Callback.Route(query);
        }
        catch (Exception e)
        {
            LogError_ToFile(e, query, "Callback");
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
            LogError_ToFile(e, inline, "Inline");
        }
    }

    private Task OnUnknown()
    {
        Print("How Did We Get Here?", ConsoleColor.Magenta);
        return Task.CompletedTask;
    }

    public void HandleCommandException(Exception e, CommandContext? context)
    {
        LogError_ToFile(e, context, context?.Title ?? "[unknown]");
        if (context != null)
        {
            SendMessage(context.Origin, GetSillyErrorMessage());
        }
    }

    private static readonly FileLogger_Simple _errorLogger = new (File_Errors);

    // todo outta here
    public static void LogError_ToFile(Exception e, object? context, string title)
    {
        LogError($"{title} >> BRUH | {e.GetErrorMessage()}");
        try
        {
            var json = JsonSerializer.Serialize(context, DebugMessage.JsonOptions);
            var entry =
                $"""
                 # {DateTime.Now:MMM' 'dd', 'HH:mm:ss.fff}

                 ## Error
                 ```c#
                 {e}
                 ```
                 ## Context
                 ```json
                 {json}
                 ```


                 """;
            _errorLogger.Log(entry);
        }
        catch
        {
            //
        }
    }
}