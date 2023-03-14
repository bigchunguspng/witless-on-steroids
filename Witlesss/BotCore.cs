using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace Witlesss
{
    public abstract class BotCore
    {
        public readonly TelegramBotClient Client = new(File.ReadAllText(".token"));

        public void SendMessage(long chat, string text)
        {
            var task = Client.SendTextMessageAsync(chat, text, ParseMode.Html, disableNotification: true);
            TrySend(task, chat, "message");
        }

        public void SendPhoto(long chat, InputOnlineFile photo)
        {
            var task = Client.SendPhotoAsync(chat, photo);
            TrySend(task, chat, "photo");
        }

        public void SendAnimation(long chat, InputOnlineFile animation)
        {
            var task = Client.SendAnimationAsync(chat, animation);
            TrySend(task, chat, "animation");
        }

        public void SendVideo(long chat, InputOnlineFile video)
        {
            var task = Client.SendVideoAsync(chat, video);
            TrySend(task, chat, "video");
        }

        public void SendAudio(long chat, InputOnlineFile audio)
        {
            var task = Client.SendAudioAsync(chat, audio);
            TrySend(task, chat, "audio");
        }

        public void SendDocument(long chat, InputOnlineFile document)
        {
            var task = Client.SendDocumentAsync(chat, document);
            TrySend(task, chat, "document");
        }
        
        public void SendSticker(long chat, InputOnlineFile sticker)
        {
            var task = Client.SendStickerAsync(chat, sticker);
            TrySend(task, chat, "sticker");
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
                LogError($"{chat} >> Can't send the {what} --> " + FixErrorMessage(e.Message));
            }
        }
        
        public void SendErrorDetails(long chat, Exception e)
        {
            var path = UniquePath($@"{TEMP_FOLDER}\error.txt");
            File.WriteAllText(path, FixErrorMessage(e.Message));
            using var stream = File.OpenRead(path);
            SendDocument(chat, new InputOnlineFile(stream, "произошла ашыпка.txt"));
        }

        public int PingChat(long chat)
        {
            try
            {
                var task = Client.SendTextMessageAsync(chat, "😏", disableNotification: true);

                task.Wait();
                if (task.IsFaulted) throw new Exception(task.Exception?.Message);

                return task.Result.MessageId;
            }
            catch (Exception e)
            {
                LogError($"{chat} >> Can't ping --> " + e.Message);
                if (e.Message.Contains("Forbidden") || e.Message.Contains("chat not found") || e.Message.Contains("rights to send"))
                    return -1;
                return -2;
            }
        }

        public async Task DownloadFile(string fileId, string path, long chat = default)
        {
            Directory.CreateDirectory(PICTURES_FOLDER);
            try
            {
                var file = await Client.GetFileAsync(fileId);
                var stream = new FileStream(path, FileMode.Create);
                Client.DownloadFileAsync(file.FilePath!, stream).Wait();
                await stream.DisposeAsync();
            }
            catch (Exception e)
            {
                SendMessage(chat, e.Message == "Bad Request: file is too big" ? Pick(FILE_TOO_BIG_RESPONSE) : XD(e.Message));
                throw;
            }
        }
        
        public bool UserIsAdmin(User user, long chat)
        {
            var admins = Client.GetChatAdministratorsAsync(chat);
            admins.Wait();
            return admins.Result.Any(x => x.User.Id == user.Id);
        }
    }
}