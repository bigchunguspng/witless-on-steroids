using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Witlesss.Commands;

namespace Witlesss
{
    public class TelegramUpdateHandler : IUpdateHandler
    {
        private readonly CallBackHandlingCommand _command;

        public TelegramUpdateHandler(CallBackHandlingCommand command) => _command = command;

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
                _command.Pass(message);
                _command.Run();
            }
            catch (Exception e)
            {
                LogError($"{Command.LastChat.Title} >> BRUH -> {FixedErrorMessage(e.Message)}");

                if (FFmpeg.IsMatch(e.Message)) Bot.Instance.SendErrorDetails(message.Chat.Id, e);
            }

            return Task.CompletedTask;
        }

        private Task OnCallback(CallbackQuery query)
        {
            try
            {
                _command.OnCallback(query);
            }
            catch (Exception e)
            {
                LogError($"Callback >> BRUH -> {FixedErrorMessage(e.Message)}");
            }
            
            return Task.CompletedTask;
        }
    }
}