using System;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using static System.Environment;
using static Witlesss.Extension;
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

        public void SendMessage(long chat, string text)
        {
            var task = Client.SendTextMessageAsync(chat, text, ParseMode.Html, disableNotification: true);
            TrySend(task, chat, "MESSAGE");
        }

        public void SendPhoto(long chat, InputOnlineFile photo)
        {
            var task = Client.SendPhotoAsync(chat, photo);
            TrySend(task, chat, "PHOTO");
        }

        public void SendAnimation(long chat, InputOnlineFile animation)
        {
            var task = Client.SendAnimationAsync(chat, animation);
            TrySend(task, chat, "ANIMATION");
        }

        public void SendVideo(long chat, InputOnlineFile video)
        {
            var task = Client.SendVideoAsync(chat, video);
            TrySend(task, chat, "VIDEO");
        }

        public void SendAudio(long chat, InputOnlineFile audio)
        {
            var task = Client.SendAudioAsync(chat, audio);
            TrySend(task, chat, "AUDIO");
        }

        public void SendDocument(long chat, InputOnlineFile document)
        {
            var task = Client.SendDocumentAsync(chat, document);
            TrySend(task, chat, "DOCUMENT");
        }

        private void TrySend(Task task, long chat, string what)
        {
            try
            {
                task.Wait();
                if (task.IsFaulted) throw new Exception(task.Exception?.Message);
            }
            catch (Exception e)
            {
                LogError($"{chat} >> CAN'T SEND {what}: " + e.Message);
            }
        }
        
        protected int PingChat(long chat)
        {
            try
            {
                var task = Client.SendTextMessageAsync(chat, "мммм...", disableNotification: true);

                task.Wait();
                if (task.IsFaulted) throw new Exception(task.Exception?.Message);

                return task.Result.MessageId;
            }
            catch (Exception e)
            {
                LogError($"{chat} >> CAN'T PING: " + e.Message);
                if (e.Message.Contains("Forbidden") || e.Message.Contains("chat not found") || e.Message.Contains("have no rights to send a message"))
                    return -1;
                return -2;
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