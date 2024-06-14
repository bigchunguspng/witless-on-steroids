using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands.Editing.Core
{
    public abstract class FileEditingCommand : AsyncCommand
    {
        private bool _isPhoto;

        protected string FileID = default!;

        protected virtual string Manual => DAMN_MANUAL; // todo replace other = with => since they are one time use

        protected override async Task Run()
        {
            if (ItHasSomethingToProcess()) await Execute();
        }

        private bool ItHasSomethingToProcess()
        {
            if (ChatIsBanned()) return false;

            if (GetFileFrom(Message) || GetFileFrom(Message.ReplyToMessage)) return true;

            SendManual();
            return false;
        }

        protected abstract Task Execute();

        protected virtual bool ChatIsBanned() => Bot.ThorRagnarok.ChatIsBanned(Chat);

        protected virtual void SendManual() => Bot.SendMessage(Chat, Manual);

        private bool GetFileFrom(Message? message)
        {
            return message is not null && MessageContainsFile(message);
        }

        protected virtual bool MessageContainsFile(Message m)
        {
            return GetVideoFileID(m) || GetAudioFileID(m);
        }

        protected bool GetVideoFileID(Message m)
        {
            if      (m.Video     is not null)          FileID = m.Video    .FileId;
            else if (m.Animation is not null)          FileID = m.Animation.FileId;
            else if (m.Sticker   is { IsVideo: true }) FileID = m.Sticker  .FileId;
            else if (m.VideoNote is not null)          FileID = m.VideoNote.FileId;
            else return false;

            return true;
        }
        protected bool GetAudioFileID(Message m)
        {
            if      (m.Audio     is not null)                  FileID = m.Audio   .FileId;
            else if (m.Voice     is not null)                  FileID = m.Voice   .FileId;
            else if (m.Document  is not null && MightBeWav(m)) FileID = m.Document.FileId;
            else return false;

            return true;
        }
        protected bool GetPhotoFileID(Message m)
        {
            if      (m.Photo    is not null)                              FileID = m.Photo[^1].FileId;
            else if (m.Sticker  is { IsVideo: false, IsAnimated: false }) FileID = m.Sticker  .FileId;
            else if (m.Document is not null && IsPicture(m.Document))     FileID = m.Document .FileId;
            else return false;

            _isPhoto = true;

            return true;
        }
        private static bool IsPicture(Document d) => d is { MimeType: "image/png" or "image/jpeg", Thumb: not null };
        private static bool MightBeWav(Message m) => m.Document!.FileName!.EndsWith(".wav");

        protected void SendResult(string result, MediaType type)
        {
            using var stream = File.OpenRead(result);
            if                     (_isPhoto) Bot.SendPhoto    (Chat, new InputOnlineFile(stream));
            else if (type == MediaType.Audio) Bot.SendAudio    (Chat, new InputOnlineFile(stream, AudioFileName));
            else if (type == MediaType.Video) Bot.SendAnimation(Chat, new InputOnlineFile(stream, VideoFileName));
            else if (type == MediaType.Movie) Bot.SendVideo    (Chat, new InputOnlineFile(stream, VideoFileName));
            else if (type == MediaType.Round) Bot.SendVideoNote(Chat, new InputOnlineFile(stream));
        }

        protected virtual string VideoFileName => "piece_fap_club.mp3";
        protected virtual string AudioFileName => "piece_fap_club.mp4";

        protected string Sender => ValidFileName(Message.GetSenderName());
        protected string SongNameOr(string s) => Message.GetSongNameOr(s);

        protected async Task<(string path, MediaType type, int waitMessage)> DownloadFileSuperCool()
        {
            if (FileID.StartsWith("http"))
            {
                var waitMessage = Bot.PingChat(Chat, PLS_WAIT_RESPONSE[Random.Shared.Next(5)]);

                var task = new DownloadVideoTask(FileID, Context).RunAsync();
                await Bot.RunSafelyAsync(task, Chat, waitMessage);

                Bot.EditMessage(Chat, waitMessage, XDDD(Pick(PROCESSING_RESPONSE)));

                return (await task, MediaType.Video, waitMessage);
            }
            else
            {
                var (path, type) = await Bot.Download(FileID, Chat);

                var waitMessage = SizeInBytes(path) > 4_000_000
                    ? Bot.PingChat(Chat, XDDD(Pick(PROCESSING_RESPONSE)))
                    : -1;

                return (path, type, waitMessage);
            }
        }
    }
}