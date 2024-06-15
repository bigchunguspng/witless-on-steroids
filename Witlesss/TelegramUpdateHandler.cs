using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace Witlesss
{
    public class TelegramUpdateHandler : IUpdateHandler
    {
        public TelegramUpdateHandler(CommandAndCallbackRouter router) => Router = router;

        public static CommandAndCallbackRouter Router { get; private set; } = default!;

        public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            return update switch
            {
                { Message:       { } message } => OnMessage(message),
                { EditedMessage: { } message } => OnMessage(message),
                { CallbackQuery: { } query   } => OnCallback(query)
            };
        }

        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            LogError($"Telegram API Error x_x --> {exception.Message}");
            return Task.CompletedTask;
        }
        
        private Task OnMessage(Message message)
        {
            try
            {
                Router.Pass(message);
                Router.Run();
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
                LogError($"Callback >> BRUH -> {FixedErrorMessage(e.Message)}");
            }
            
            return Task.CompletedTask;
        }

        public static void HandleCommandException(Exception e, CommandContext context)
        {
            LogError($"{context.Title} >> BRUH -> {FixedErrorMessage(e.Message)}");

            if (FFmpeg.IsMatch(e.Message)) Bot.Instance.SendErrorDetails(context.Chat, e);
        }
    }
}