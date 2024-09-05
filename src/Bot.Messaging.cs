﻿using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Witlesss
{
    public partial class Bot
    {
        public void SendMessage(long chat, string text, bool preview = false, int? replyTo = null)
        {
            var task = Client.SendTextMessageAsync
                (chat, text, ParseMode.Html, disableWebPagePreview: !preview, replyToMessageId: replyTo);
            TrySend(task, chat, "message");
        }
        public void SendMessage(long chat, string text, InlineKeyboardMarkup? inline, bool preview = false)
        {
            var task = Client.SendTextMessageAsync
                (chat, text, ParseMode.Html, disableWebPagePreview: !preview, replyMarkup: inline);
            TrySend(task, chat, "message [+][-]");
        }

        public void CopyMessage(long chat, long fromChat, int messageId, int? replyTo = null)
        {
            var task = Client.CopyMessageAsync(chat, fromChat, messageId, replyToMessageId: replyTo);
            TrySend(task, chat, "message", "copy");
        }

        public void SendOrEditMessage(long chat, string text, int messageId, InlineKeyboardMarkup? buttons)
        {
            if (messageId < 0) SendMessage(chat, /*      */ text, buttons);
            else /*         */ EditMessage(chat, messageId, text, buttons);
        }

        // SEND MEDIA

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

        public void SendAudio(long chat, InputOnlineFile audio, string? art = null)
        {
            using var cover = File.OpenRead(art ?? File_DefaultAlbumCover);
            var task = Client.SendAudioAsync(chat, audio, thumb: new InputMedia(cover, "xd"));
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

        public void SendVideoNote(long chat, InputOnlineFile note)
        {
            var task = Client.SendVideoNoteAsync(chat, note);
            TrySend(task, chat, "videonote");
        }

        public void SendVoice(long chat, InputOnlineFile voice)
        {
            var task = Client.SendVoiceAsync(chat, voice);
            TrySend(task, chat, "voice");
        }

        public void SendAlbum(long chat, IEnumerable<IAlbumInputMedia> album)
        {
            var task = Client.SendMediaGroupAsync(chat, album);
            TrySend(task, chat, "album");
        }

        // EDIT / DELETE

        public void EditMessage(long chat, int id, string text)
        {
            var task = Client.EditMessageTextAsync
                (chat, id, text, ParseMode.Html);
            TrySend(task, chat, "message", "edit");
        }
        public void EditMessage(long chat, int id, string text, InlineKeyboardMarkup? inline, bool preview = false)
        {
            var task = Client.EditMessageTextAsync
                (chat, id, text, ParseMode.Html, disableWebPagePreview: !preview, replyMarkup: inline);
            TrySend(task, chat, "message [+][-]", "edit");
        }

        public async void DeleteMessageAsync(long chat, int id)
        {
            await Task.Run(() => DeleteMessage(chat, id));
        }

        public void DeleteMessage(long chat, int id)
        {
            if (id <= 0) return;
            var task = Client.DeleteMessageAsync(chat, id);
            TrySend(task, chat, "message", "delete");
        }


        private static void TrySend(Task task, long chat, string what, string action = "send", int patience = 5)
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
                if (reason.Contains("Server Error") && patience > 0)
                    TrySend(task, chat, what, action, patience - 1);
            }
        }
        
        public void SendPhotoXD(long chat, InputOnlineFile photo, string caption) => SendOrThrow(Client.SendPhotoAsync    (chat, photo, caption));
        public void SendAnimaXD(long chat, InputOnlineFile photo, string caption) => SendOrThrow(Client.SendAnimationAsync(chat, photo, caption: caption));

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
                EditMessageOnError(chat, message);
                throw;
            }
        }

        public async Task RunSafelyAsync(Task task, long chat, int message)
        {
            try // todo move it to async command class: message - field
            {
                await task;
                if (task.IsFaulted) throw new Exception(task.Exception?.Message);
            }
            catch (Exception e)
            {
                LogError($"BRUH -> {e.GetFixedMessage()}");

                if (message > 0) EditMessageOnError(chat, message);
            }
        }

        public void EditMessageOnError(long chat, int message)
        {
            EditMessage(chat, message, $"произошла ашыпка {FAIL_EMOJI_2.PickAny()}");
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