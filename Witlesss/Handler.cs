﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Witlesss.Commands;
using static Witlesss.Extension;
using static Witlesss.Logger;

namespace Witlesss
{
    public class Handler : IUpdateHandler
    {
        private readonly MainJunction _fork;
            
        public Handler() => _fork = new MainJunction();

        public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message)
                TryHandleMessage(update.Message);
            else if (update.Type == UpdateType.EditedMessage)
                TryHandleMessage(update.EditedMessage);
            return Task.CompletedTask;
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            LogError("Telegram API Error...");
            return Task.CompletedTask;
        }
        
        private void TryHandleMessage(Message message)
        {
            try
            {
                _fork.Pass(message);
                _fork.Run();
            }
            catch (Exception exception)
            {
                LogError(TitleOrUsername(message) + " >> CAN'T HANDLE MESSAGE: " + exception.Message);
            }
        }
    }
}