using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Witlesss.Commands;

namespace Witlesss
{
    public class Handler : IUpdateHandler
    {
        private readonly EntryPoint _fork = new();

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
                _fork.Pass(message);
                _fork.Run();
            }
            catch (Exception e)
            {
                LogError($"{TitleOrUsername(message)} >> Can't process message --> {e.Message}");

                if (e.Message.Contains("ffmpeg"))
                {
                    Directory.CreateDirectory(TEMP_FOLDER);
                    var path = UniquePath($@"{TEMP_FOLDER}\error.txt", ".txt");
                    File.WriteAllText(path, e.Message);
                    using var stream = File.OpenRead(path);
                    Command.Bot.SendDocument(message.Chat.Id, new InputOnlineFile(stream, "произошла ашыпка(9.txt"));
                }
            }

            return Task.CompletedTask;
        }
    }
}