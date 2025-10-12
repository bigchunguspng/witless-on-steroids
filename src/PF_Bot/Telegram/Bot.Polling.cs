using System.Text.Json;
using System.Text.Json.Serialization;
using PF_Bot.Core;
using PF_Bot.Routing.Callbacks;
using PF_Bot.Routing.Commands;
using PF_Bot.Routing.Inline;
using PF_Bot.Routing.Messages;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PF_Bot.Telegram;

public partial class Bot
{
    public static IMessageRouter  Router_Message  { get; private set; } = null!;
    public static ICallbackRouter Router_Callback { get; private set; } = null!;
    public static InlineQueryHandler      Inliner { get; private set; } = new();

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
            Router_Message.Route(message);
        }
        catch (Exception e)
        {
            HandleMessageRoutingException(e, message);
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
            await Inliner.Handle(inline);
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

    public void HandleMessageRoutingException(Exception e, Message message)
    {
        LogError_ToFile(e, message, $"Message router ({message.Format_ChatMessage()})");
        SendMessage(message.GetOrigin(), GetSillyErrorMessage());
    }

    private static readonly FileLogger_Simple _errorLogger = new (File_Errors);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = NewtonsoftJsonCompatibleEncoder.Encoder,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true,
        Converters =
        {
            new CommandHandler.JsonConverter(),
            new CallbackContextJsonConverter(),
        },
    };

    // todo outta here
    public static void LogError_ToFile(Exception e, object? context, string title)
    {
        LogError($"{title} >> BRUH | {e.GetErrorMessage()}");
        try
        {
            var json = JsonSerializer.Serialize(context, JsonOptions);
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