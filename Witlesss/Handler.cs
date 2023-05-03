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
    public class Handler : IUpdateHandler
    {
        private readonly Command _command;
        private readonly Regex _ffmpeg = new(@"ffmpeg|ffprobe", RegexOptions.IgnoreCase);
        
        public Handler(Command command) => _command = command;

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
                _command.Pass(message);
                _command.Run();
            }
            catch (Exception e)
            {
                LogError($"{Command.TitleOrUsername} >> BRUH -> {FixedErrorMessage(e.Message)}");

                if (_ffmpeg.IsMatch(e.Message)) Command.Bot.SendErrorDetails(message.Chat.Id, e);
            }

            return Task.CompletedTask;
        }
    }
}