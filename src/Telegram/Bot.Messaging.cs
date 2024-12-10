using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Witlesss.Telegram
{
    public partial class Bot
    {
        public void SendOrEditMessage(MessageOrigin origin, string text, int messageId, InlineKeyboardMarkup? buttons)
        {
            if (messageId < 0) SendMessage(origin,                 text, buttons);
            else               EditMessage(origin.Chat, messageId, text, buttons);
        }

        // SEND

        public void SendMessage(MessageOrigin origin, string text, bool preview = false, int? replyTo = null)
        {
            var task = Client.SendMessage
            (
                origin.Chat, text, ParseMode.Html,
                messageThreadId: origin.Thread,
                replyParameters: GetReplyParameters(replyTo),
                linkPreviewOptions: GetPreviewOptions(preview)
            );
            TrySend(origin.Chat, task, "message");
        }

        public void SendMessage(MessageOrigin origin, string text, InlineKeyboardMarkup? inline, bool preview = false)
        {
            var task = Client.SendMessage
            (
                origin.Chat, text, ParseMode.Html,
                messageThreadId: origin.Thread,
                replyMarkup: inline,
                linkPreviewOptions: GetPreviewOptions(preview)
            );
            TrySend(origin.Chat, task, "message [+][-]");
        }

        // SEND (admin)

        public void SendMessage(ChatId chat, string text, bool preview = false, int? replyTo = null)
        {
            var task = Client.SendMessage
            (
                chat, text, ParseMode.Html,
                replyParameters: GetReplyParameters(replyTo),
                linkPreviewOptions: GetPreviewOptions(preview)
            );
            TrySend(chat.Identifier ?? 0, task, "message");
        }

        public void CopyMessage(long chat, long fromChat, int messageId, int? replyTo = null)
        {
            var task = Client.CopyMessage
            (
                chat, fromChat, messageId,
                replyParameters: GetReplyParameters(replyTo)
            );
            TrySend(chat, task, "message", "copy");
        }

        public void CopyMessage(ChatId chat, long fromChat, int messageId, int? replyTo = null)
        {
            var task = Client.CopyMessage
            (
                chat, fromChat, messageId,
                replyParameters: GetReplyParameters(replyTo)
            );
            TrySend(chat.Identifier ?? 0, task, "message", "copy");
        }

        public void React(ChatId chat, int messageId, ReactionType[]? reaction)
        {
            var task = Client.SetMessageReaction(chat, messageId, reaction);
            TrySend(chat.Identifier ?? 0, task, "reaction", "set");
        }

        // EDIT

        public void EditMessage(long chat, int id, string text)
        {
            var task = Client.EditMessageText(chat, id, text, ParseMode.Html);
            TrySend(chat, task, "message", "edit");
        }

        public void EditMessage(long chat, int id, string text, InlineKeyboardMarkup? inline, bool preview = false)
        {
            var task = Client.EditMessageText
            (
                chat, id, text, ParseMode.Html,
                replyMarkup: inline,
                linkPreviewOptions: GetPreviewOptions(preview)
            );
            TrySend(chat, task, "message [+][-]", "edit");
        }

        // DELETE

        public async void DeleteMessageAsync(long chat, int id)
            => await Task.Run(() => DeleteMessage(chat, id));

        private void DeleteMessage(long chat, int id)
        {
            if (id <= 0) return;
            TrySend(chat, Client.DeleteMessage(chat, id), "message", "delete");
        }

        // SEND MEDIA

        public void SendPhoto(MessageOrigin og, InputFile photo)
            => TrySend(og.Chat, Client.SendPhoto(og.Chat, photo, messageThreadId: og.Thread), "photo");

        public void SendVideo(MessageOrigin og, InputFile video)
            => TrySend(og.Chat, Client.SendVideo(og.Chat, video, messageThreadId: og.Thread), "video");

        public void SendVoice(MessageOrigin og, InputFile voice)
            => TrySend(og.Chat, Client.SendVoice(og.Chat, voice, messageThreadId: og.Thread), "voice");

        public void SendVideoNote(MessageOrigin og, InputFile note)
            => TrySend(og.Chat, Client.SendVideoNote(og.Chat, note, messageThreadId: og.Thread), "videonote");

        public void SendAnimation(MessageOrigin og, InputFile animation)
            => TrySend(og.Chat, Client.SendAnimation(og.Chat, animation, messageThreadId: og.Thread), "animation");

        public void SendDocument(MessageOrigin og, InputFile document)
            => TrySend(og.Chat, Client.SendDocument(og.Chat, document, messageThreadId: og.Thread), "document");

        public void SendSticker(MessageOrigin og, InputFile sticker)
            => TrySend(og.Chat, Client.SendSticker(og.Chat, sticker, messageThreadId: og.Thread), "sticker");

        public void SendAlbum(MessageOrigin og, IEnumerable<IAlbumInputMedia> album)
            => TrySend(og.Chat, Client.SendMediaGroup(og.Chat, album, messageThreadId: og.Thread), "album");

        public void SendAudio(MessageOrigin og, InputFile audio, string? art = null)
        {
            using var cover = File.OpenRead(art ?? File_DefaultAlbumCover);
            var thumb = InputFile.FromStream(cover, "xd");
            TrySend(og.Chat, Client.SendAudio(og.Chat, audio, thumbnail: thumb, messageThreadId: og.Thread), "audio");
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

        public void SendPhotoXD(MessageOrigin origin, InputFile photo, string caption)
            => SendOrThrow(Client.SendPhoto    (origin.Chat, photo, caption, messageThreadId: origin.Thread));

        public void SendAnimaXD(MessageOrigin origin, InputFile photo, string caption)
            => SendOrThrow(Client.SendAnimation(origin.Chat, photo, caption, messageThreadId: origin.Thread));

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
                EditMessage(chat, message, GetSillyErrorMessage());
                throw;
            }
        }

        public string GetSillyErrorMessage() => $"произошла ашыпка {FAIL_EMOJI_2.PickAny()}";

        public void SendErrorDetails(MessageOrigin origin, string command, string errorMessage)
        {
            var path = UniquePath(Dir_Temp, "error.txt");
            var text = string.Format(FF_ERROR_REPORT, command, GetRandomASCII(), errorMessage);
            File.WriteAllText(path, text);
            using var stream = File.OpenRead(path);
            SendDocument(origin, InputFile.FromStream(stream, (string?)"произошла ашыпка.txt"));
        }

        // todo separate technical and user-driven usage
        public int PingChat(MessageOrigin origin, string text = "😏", bool notify = true)
        {
            try
            {
                var task = Client.SendMessage
                (
                    origin.Chat, text, ParseMode.Html,
                    messageThreadId: origin.Thread,
                    disableNotification: !notify
                );

                task.Wait();
                if (task.IsFaulted) throw new Exception(task.Exception?.Message);

                return task.Result.MessageId;
            }
            catch (Exception e)
            {
                LogError($"{origin.Chat} >> Can't ping --> " + e.Message);
                return ChatCanBeRemoved(e) ? -1 : -2;
            }
        }

        private static bool ChatCanBeRemoved(Exception e) =>
            e.Message.Contains("Forbidden")
         || e.Message.Contains("chat not found")
         || e.Message.Contains("rights to send");

        private static LinkPreviewOptions GetPreviewOptions(bool showPreview) => new() { IsDisabled = !showPreview };

        private static ReplyParameters? GetReplyParameters(int? replyTo)
        {
            return replyTo.HasValue ? new ReplyParameters { MessageId = replyTo.Value } : null;
        }
    }
}