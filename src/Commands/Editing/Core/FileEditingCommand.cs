using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands.Editing.Core
{
    public abstract class FileEditingCommand : AsyncCommand
    {
        protected string FileID = default!;
        protected bool IsPhoto;

        // RUN

        protected override async Task Run()
        {
            if (MessageHasSomethingToProcess()) await Execute();
        }

        protected abstract Task Execute();

        private bool MessageHasSomethingToProcess()
        {
            if (ChatIsBanned()) return false;

            if (GetFileFrom(Message) || GetFileFrom(Message.ReplyToMessage)) return true;

            SendManual();
            return false;
        }

        protected virtual bool ChatIsBanned() => Bot.ThorRagnarok.ChatIsBanned(Chat);

        protected void SendManual()
        {
            var manual = SyntaxManual is null
                ? string.Format(EDIT_MANUAL,     SuportedMedia)
                : string.Format(EDIT_MANUAL_SYN, SuportedMedia, SyntaxManual);
            Bot.SendMessage(Chat, manual);
        }

        protected virtual string? SyntaxManual => null;
        protected abstract string SuportedMedia { get; }

        // GET FILE

        private bool GetFileFrom(Message? message)
            => message is not null && MessageContainsFile(message);

        protected virtual bool MessageContainsFile(Message m)
            => GetVideoFileID(m) || GetAudioFileID(m);

        // GET FILE HELPERS

        protected bool GetVideoFileID(Message m)
        {
            if      (m.Video     is not null) FileID = m.Video    .FileId;
            else if (m.Animation is not null) FileID = m.Animation.FileId;
            else if (m.HasVideoSticker    ()) FileID = m.Sticker !.FileId;
            else if (m.VideoNote is not null) FileID = m.VideoNote.FileId;
            else return false;

            return true;
        }

        protected bool GetAudioFileID(Message m)
        {
            if      (m.Audio     is not null) FileID = m.Audio    .FileId;
            else if (m.Voice     is not null) FileID = m.Voice    .FileId;
            else if (m.HasAudioDocument   ()) FileID = m.Document!.FileId;
            else return false;

            return true;
        }

        protected bool GetPhotoFileID(Message m)
        {
            if      (m.Photo    is not null) FileID = m.Photo[^1].FileId;
            else if (m.HasImageSticker   ()) FileID = m.Sticker !.FileId;
            else if (m.HasImageDocument  ()) FileID = m.Document!.FileId;
            else return false;

            IsPhoto = true;

            return true;
        }

        // SEND

        protected void SendResult(string result, MediaType type)
        {
            using var stream = File.OpenRead(result);
            if                      (IsPhoto) Bot.SendPhoto    (Chat, new InputOnlineFile(stream));
            else if (type == MediaType.Audio) Bot.SendAudio    (Chat, new InputOnlineFile(stream, AudioFileName));
            else if (type == MediaType.Video) Bot.SendAnimation(Chat, new InputOnlineFile(stream, VideoFileName));
            else if (type == MediaType.Movie) Bot.SendVideo    (Chat, new InputOnlineFile(stream, VideoFileName));
            else if (type == MediaType.Round) Bot.SendVideoNote(Chat, new InputOnlineFile(stream));
        }

        protected virtual string VideoFileName => "piece_fap_club.mp3";
        protected virtual string AudioFileName => "piece_fap_club.mp4";

        protected string Sender => ValidFileName(Message.GetSenderName());
        protected string SongNameOr(string s) => Message.GetSongNameOr(s);
    }
}