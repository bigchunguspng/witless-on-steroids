using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PF_Bot.Telegram
{
    public partial class Bot
    {
        public void SendOrEditMessage(MessageOrigin origin, string text, int messageId, InlineKeyboardMarkup? buttons)
        {
            if (messageId < 0) SendMessage(origin,                 text, buttons);
            else               EditMessage(origin.Chat, messageId, text, buttons);
        }

        // SEND

        public void SendMessage(MessageOrigin origin, string text, bool preview = false)
        {
            var task = Client.SendMessage
            (
                origin.Chat, text, ParseMode.Html,
                replyParameters: origin.Thread,
                linkPreviewOptions: GetPreviewOptions(preview)
            );
            TrySend(origin.Chat, task, "message");
        }

        public void SendMessage(MessageOrigin origin, string text, InlineKeyboardMarkup? inline, bool preview = false)
        {
            var task = Client.SendMessage
            (
                origin.Chat, text, ParseMode.Html,
                replyParameters: origin.Thread,
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
                replyParameters: replyTo,
                linkPreviewOptions: GetPreviewOptions(preview)
            );
            TrySend(chat.Identifier ?? 0, task, "message");
        }

        public void CopyMessage(long chat, long fromChat, int messageId, int? replyTo = null)
        {
            var task = Client.CopyMessage
            (
                chat, fromChat, messageId,
                replyParameters: replyTo
            );
            TrySend(chat, task, "message", "copy");
        }

        public void CopyMessage(ChatId chat, long fromChat, int messageId, int? replyTo = null)
        {
            var task = Client.CopyMessage
            (
                chat, fromChat, messageId,
                replyParameters: replyTo
            );
            TrySend(chat.Identifier ?? 0, task, "message", "copy");
        }

        public async void ReactAsync(ChatId chat, int messageId, ReactionType[]? reaction)
        {
            try
            {
                await Client.SetMessageReaction(chat, messageId, reaction);
            }
            catch (Exception e)
            {
                LogError($"{chat} >> Can't set reaction | {e.GetErrorMessage()}");
            }
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
        {
            try
            {
                if (id <= 0) return;

                await Client.DeleteMessage(chat, id);
            }
            catch (Exception e)
            {
                LogError($"{chat} >> Can't delete message | {e.GetErrorMessage()}");
            }
        }

        // SEND MEDIA

        public void SendPhoto     (MessageOrigin og, InputFile file) => TrySend(og.Chat, Client.SendPhoto     (og.Chat, file, replyParameters: og.Thread), "photo");
        public void SendVideo     (MessageOrigin og, InputFile file) => TrySend(og.Chat, Client.SendVideo     (og.Chat, file, replyParameters: og.Thread), "video");
        public void SendVoice     (MessageOrigin og, InputFile file) => TrySend(og.Chat, Client.SendVoice     (og.Chat, file, replyParameters: og.Thread), "voice");
        public void SendVideoNote (MessageOrigin og, InputFile file) => TrySend(og.Chat, Client.SendVideoNote (og.Chat, file, replyParameters: og.Thread), "videonote");
        public void SendAnimation (MessageOrigin og, InputFile file) => TrySend(og.Chat, Client.SendAnimation (og.Chat, file, replyParameters: og.Thread), "animation");
        public void SendDocument  (MessageOrigin og, InputFile file) => TrySend(og.Chat, Client.SendDocument  (og.Chat, file, replyParameters: og.Thread), "document");
        public void SendSticker   (MessageOrigin og, InputFile file) => TrySend(og.Chat, Client.SendSticker   (og.Chat, file, replyParameters: og.Thread), "sticker");
        public Message[]? SendAlbum
            (MessageOrigin og, IEnumerable<IAlbumInputMedia> album) 
            => TrySend(og.Chat, Client.SendMediaGroup(og.Chat, album, replyParameters: og.Thread), "album");

        public void SendAudio(MessageOrigin og, InputFile audio, string? art = null)
        {
            using var cover = File.OpenRead(art ?? File_DefaultAlbumCover);
            var thumb = InputFile.FromStream(cover, "xd");
            TrySend(og.Chat, Client.SendAudio(og.Chat, audio, thumbnail: thumb, replyParameters: og.Thread), "audio");
        }


        private static T? TrySend<T>(long chat, Task<T> task, string what, string action = "send")
        {
            var result = default(T);
            try
            {
                task.Wait();
                result = task.IsFaulted ? throw new Exception(task.Exception?.Message) : task.Result;
            }
            catch (Exception e)
            {
                LogError($"{chat} >> Can't {action} {what} | {e.GetErrorMessage()}");
            }

            return result;
        }

        public async Task<Message> SendPhoto_OrThrow(MessageOrigin og, InputFile photo, string caption)
            => await Client.SendPhoto    (og.Chat, photo, caption, replyParameters: og.Thread);

        public async Task<Message> SendAnima_OrThrow(MessageOrigin og, InputFile anima, string caption)
            => await Client.SendAnimation(og.Chat, anima, caption, replyParameters: og.Thread);

        // todo separate technical and user-driven usage
        public int PingChat(MessageOrigin origin, string text = "😏", bool notify = true)
        {
            try
            {
                var task = Client.SendMessage
                (
                    origin.Chat, text, ParseMode.Html,
                    replyParameters: origin.Thread,
                    disableNotification: !notify
                );

                task.Wait();
                if (task.IsFaulted) throw new Exception(task.Exception?.Message);

                return task.Result.MessageId;
            }
            catch (Exception e)
            {
                LogError($"{origin.Chat} >> Can't ping | {e.GetErrorMessage()}");
                return ChatCanBeRemoved(e) ? -1 : -2;
            }
        }

        private static bool ChatCanBeRemoved(Exception e) =>
            e.Message.Contains("Forbidden")
         || e.Message.Contains("chat not found")
         || e.Message.Contains("rights to send");

        private static LinkPreviewOptions GetPreviewOptions(bool showPreview) => new() { IsDisabled = !showPreview };
    }
}