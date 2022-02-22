using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static Witlesss.Logger;

namespace Witlesss
{
    public class Handler : IUpdateHandler
    {
        private readonly Bot _bot;
            
        public Handler(Bot bot) => _bot = bot;

        public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message)
                _bot.TryHandleMessage(update.Message);
            else if (update.Type == UpdateType.EditedMessage)
                _bot.TryHandleMessage(update.EditedMessage);
            return Task.CompletedTask;
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            LogError("Telegram API Error...");
            return Task.CompletedTask;
        }
    }
}