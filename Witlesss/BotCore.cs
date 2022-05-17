using System;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using static System.Environment;
using static Witlesss.Also.Extension;
using static Witlesss.Logger;
using static Witlesss.Also.Strings;
using File = System.IO.File;

namespace Witlesss
{
    public abstract class BotCore
    {
        protected readonly TelegramBotClient Client;

        protected BotCore()
        {
            string token = File.ReadAllText($@"{CurrentDirectory}\.token");
            Client = new TelegramBotClient(token);
        }

        public async void SendMessage(long chat, string text)
        {
            Task task = Client.SendTextMessageAsync(chat, text, ParseMode.Html, disableNotification: true);
            await TrySend(task, chat, "MESSAGE");
        }

        public void SendPhoto(long chat, InputOnlineFile photo)
        {
            Task task = Client.SendPhotoAsync(chat, photo);
            TrySend(task, chat, "PHOTO").Wait();
        }

        public void SendAnimation(long chat, InputOnlineFile animation)
        {
            Task task = Client.SendAnimationAsync(chat, animation);
            TrySend(task, chat, "ANIMATION").Wait();
        }

        public void SendVideo(long chat, InputOnlineFile video)
        {
            Task task = Client.SendVideoAsync(chat, video);
            TrySend(task, chat, "VIDEO").Wait();
        }

        public void SendAudio(long chat, InputOnlineFile audio)
        {
            Task task = Client.SendAudioAsync(chat, audio);
            TrySend(task, chat, "AUDIO").Wait();
        }

        public void SendDocument(long chat, InputOnlineFile document)
        {
            Task task = Client.SendDocumentAsync(chat, document);
            TrySend(task, chat, "DOCUMENT").Wait();
        }

        private async Task TrySend(Task task, long chat, string what)
        {
            try
            {
                await task.ContinueWith(action =>
                {
                    LogError($"{chat} >> CAN'T SEND {what}: " + action.Exception?.Message);
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
            catch
            {
                // 21
            }
        }

        public async Task DownloadFile(string fileId, string path, long chat = default)
        {
            Directory.CreateDirectory($@"{CurrentDirectory}\{PICTURES_FOLDER}");
            try
            {
                var file = await Client.GetFileAsync(fileId);
                var stream = new FileStream(path, FileMode.Create);
                Client.DownloadFileAsync(file.FilePath!, stream).Wait();
                await stream.DisposeAsync();
            }
            catch (Exception e)
            {
                LogError(e.Message);
                SendMessage(chat, e.Message == "Bad Request: file is too big" ? FILE_TOO_BIG_RESPONSE() : e.Message);
            }
        }
    }
}