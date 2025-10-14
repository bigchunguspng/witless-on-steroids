using PF_Bot.Core;
using PF_Bot.Routing.Callbacks;
using PF_Bot.Routing.Inline;
using PF_Bot.Routing.Messages;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PF_Bot.Telegram;

public partial class Bot
{
    private static IMessageRouter  Router_Message  { get; set; } = null!;
    private static ICallbackRouter Router_Callback { get; set; } = null!;
    private static InlineQueryHandler      Inliner { get; set; } = new();

    public void StartListening()
    {
        UpdateType[] updates =
        [
            UpdateType.Message,
            UpdateType.EditedMessage,
            UpdateType.CallbackQuery,
            UpdateType.InlineQuery,
        ];

        var options = new ReceiverOptions { AllowedUpdates = updates };

        Client.StartReceiving(HandleUpdate, HandlePollingError, options);

        Telemetry.Log_START(Me);
        Print(BUENOS_DIAS.Format(Username, Me.FirstName), ConsoleColor.Yellow);
    }

    private Task HandleUpdate
    (
        ITelegramBotClient bot,
        Update update,
        CancellationToken token
    ) => update switch
    {
        { Message:       { } message } => OnMessage  (message),
        { EditedMessage: { } message } => OnMessage  (message),
        { CallbackQuery: { } query   } => OnCallback (query),
        { InlineQuery:   { } inline  } => OnInline   (inline),
        _ => OnUnknown(),
    };

    // todo: moving average and dynamic delay: 1s -> 5s -> 15s
    private async Task HandlePollingError
    (
        ITelegramBotClient bot,
        Exception exception,
        CancellationToken token
    )
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
        catch (Exception exception)
        {
            Unluckies.Handle(exception, message, title: "MESSAGE R.");

            SendMessage(message.GetOrigin(), GetSillyErrorMessage());
        }

        return Task.CompletedTask;
    }

    private Task OnCallback(CallbackQuery query)
    {
        try
        {
            Router_Callback.Route(query);
        }
        catch (Exception exception)
        {
            Unluckies.Handle(exception, query, title: "CALLBACK R.");
        }

        return Task.CompletedTask;
    }

    private async Task OnInline(InlineQuery inline)
    {
        try
        {
            await Inliner.Handle(inline);
        }
        catch (Exception exception)
        {
            Unluckies.Handle(exception, inline, title: "INLINE H.");
        }
    }

    private Task OnUnknown()
    {
        Print("How Did We Get Here?", ConsoleColor.Magenta);
        return Task.CompletedTask;
    }
}