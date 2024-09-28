using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Witlesss
{
    public partial class Bot
    {
        public void SendOrEditMessage(long chat, string text, int messageId, InlineKeyboardMarkup? buttons)
        {
            if (messageId < 0) SendMessage(chat,            text, buttons);
            else               EditMessage(chat, messageId, text, buttons);
        }

        // SEND

        public void SendMessage(long chat, string text, bool preview = false, int? replyTo = null)
        {
            var task = Client.SendTextMessageAsync
            (
                chat, text, ParseMode.Html,
                replyToMessageId: replyTo,
                disableWebPagePreview: !preview
            );
            TrySend(chat, task, "message");
        }

        public void SendMessage(long chat, string text, InlineKeyboardMarkup? inline, bool preview = false)
        {
            var task = Client.SendTextMessageAsync
            (
                chat, text, ParseMode.Html,
                replyMarkup: inline,
                disableWebPagePreview: !preview
            );
            TrySend(chat, task, "message [+][-]");
        }

        public void CopyMessage(long chat, long fromChat, int messageId, int? replyTo = null)
        {
            var task = Client.CopyMessageAsync(chat, fromChat, messageId, replyToMessageId: replyTo);
            TrySend(chat, task, "message", "copy");
        }

        // EDIT

        public void EditMessage(long chat, int id, string text)
        {
            var task = Client.EditMessageTextAsync(chat, id, text, ParseMode.Html);
            TrySend(chat, task, "message", "edit");
        }

        public void EditMessage(long chat, int id, string text, InlineKeyboardMarkup? inline, bool preview = false)
        {
            var task = Client.EditMessageTextAsync
            (
                chat, id, text, ParseMode.Html,
                replyMarkup: inline,
                disableWebPagePreview: !preview
            );
            TrySend(chat, task, "message [+][-]", "edit");
        }

        // DELETE

        public async void DeleteMessageAsync(long chat, int id)
            => await Task.Run(() => DeleteMessage(chat, id));

        private void DeleteMessage(long chat, int id)
        {
            if (id <= 0) return;
            TrySend(chat, Client.DeleteMessageAsync(chat, id), "message", "delete");
        }

        // SEND MEDIA

        public void SendPhoto(long chat, InputOnlineFile photo)
            => TrySend(chat, Client.SendPhotoAsync(chat, photo), "photo");

        public void SendVideo(long chat, InputOnlineFile video)
            => TrySend(chat, Client.SendVideoAsync(chat, video), "video");

        public void SendVoice(long chat, InputOnlineFile voice)
            => TrySend(chat, Client.SendVoiceAsync(chat, voice), "voice");

        public void SendVideoNote(long chat, InputOnlineFile note)
            => TrySend(chat, Client.SendVideoNoteAsync(chat, note), "videonote");

        public void SendAnimation(long chat, InputOnlineFile animation)
            => TrySend(chat, Client.SendAnimationAsync(chat, animation), "animation");

        public void SendDocument(long chat, InputOnlineFile document)
            => TrySend(chat, Client.SendDocumentAsync(chat, document), "document");

        public void SendSticker(long chat, InputOnlineFile sticker)
            => TrySend(chat, Client.SendStickerAsync(chat, sticker), "sticker");

        public void SendAlbum(long chat, IEnumerable<IAlbumInputMedia> album)
            => TrySend(chat, Client.SendMediaGroupAsync(chat, album), "album");

        public void SendAudio(long chat, InputOnlineFile audio, string? art = null)
        {
            using var cover = File.OpenRead(art ?? File_DefaultAlbumCover);
            var thumb = new InputMedia(cover, "xd");
            TrySend(chat, Client.SendAudioAsync(chat, audio, thumb: thumb), "audio");
        }


        private static readonly Regex _retryAfter = new(@"retry after (\d+)");

        private static void TrySend(long chat, Task task, string what, string action = "send", int patience = 5)
        {
            try
            {
                task.Wait();
                if (task.IsFaulted) throw new Exception(task.Exception?.Message);
            }
            catch (Exception e)
            {
                var reason = e.GetFixedMessage();
                LogError($"{chat} >> Can't {action} {what} --> " + reason);
                if (patience > 0)
                {
                    var serverError = reason.Contains("Server Error");
                    var retryDelay = serverError ? 0 : _retryAfter.ExtractGroup(1, reason, int.Parse, 0);
                    if (retryDelay > 0) Task.Delay(retryDelay * 250).Wait();
                    if (retryDelay > 0 || serverError) TrySend(chat, task, what, action, patience - 1);
                }
            }
        }

        public void SendPhotoXD(long chat, InputOnlineFile photo, string caption) 
            => SendOrThrow(Client.SendPhotoAsync    (chat, photo, caption));

        public void SendAnimaXD(long chat, InputOnlineFile photo, string caption) 
            => SendOrThrow(Client.SendAnimationAsync(chat, photo, caption: caption));

        private static void SendOrThrow(Task task)
        {
            task.Wait();
            if (task.IsFaulted) throw new Exception();
        }

        public async Task RunOrThrow(Task task, long chat, int message)
        {
            try
            {
                await task;
            }
            catch
            {
                EditMessage(chat, message, $"произошла ашыпка {FAIL_EMOJI_2.PickAny()}");
                throw;
            }
        }

        public void SendErrorDetails(long chat, string command, string errorMessage)
        {
            var path = UniquePath(Dir_Temp, "error.txt");
            var text = string.Format(FF_ERROR_REPORT, command, GetRandomASCII(), errorMessage);
            File.WriteAllText(path, text);
            using var stream = File.OpenRead(path);
            SendDocument(chat, new InputOnlineFile(stream, "произошла ашыпка.txt"));
        }

        public int PingChat(long chat, string text = "😏", bool notify = true)
        {
            try
            {
                var task = Client.SendTextMessageAsync(chat, text, ParseMode.Html, disableNotification: !notify);

                task.Wait();
                if (task.IsFaulted) throw new Exception(task.Exception?.Message);

                return task.Result.MessageId;
            }
            catch (Exception e)
            {
                LogError($"{chat} >> Can't ping --> " + e.Message);
                return ChatCanBeRemoved(e) ? -1 : -2;
            }
        }

        private static bool ChatCanBeRemoved(Exception e) =>
            e.Message.Contains("Forbidden")
         || e.Message.Contains("chat not found")
         || e.Message.Contains("rights to send");
    }
}