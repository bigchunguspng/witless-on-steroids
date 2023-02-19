using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Witlesss.Commands;

namespace Witlesss
{
    public class Handler : IUpdateHandler
    {
        private readonly Bot _bot;
        
        public Handler(Bot bot) => _bot = bot;

        public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            return update switch
            {
                { Message:       { } message } => OnMessage(message),
                { EditedMessage: { } message } => OnMessage(message)
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
                _bot.Fork.Pass(message);
                _bot.Fork.Run();
            }
            catch (Exception e)
            {
                LogError($"{Command.TitleOrUsername} >> BRUH -> {FixErrorMessage(e.Message)}");

                if (e.Message.Contains("ffmpeg")) _bot.SendErrorDetails(message.Chat.Id, e);
            }

            return Task.CompletedTask;
        }
    }
}