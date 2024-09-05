using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands.Editing.Core
{
    public abstract class FileEditingCommand : AsyncCommand
    {
        protected string FileID = default!, Ext = default!;
        protected MediaType Type;

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
            if      (m.Video      != null) (Type, FileID, Ext) = (MediaType.Video, m.Video    .FileId, ".mp4" );
            else if (m.Animation  != null) (Type, FileID, Ext) = (MediaType.Anime, m.Animation.FileId, ".mp4" );
            else if (m.VideoNote  != null) (Type, FileID, Ext) = (MediaType.Round, m.VideoNote.FileId, ".mp4" );
            else if (m.HasVideoSticker ()) (Type, FileID, Ext) = (MediaType.Anime, m.Sticker !.FileId, ".webm");
            else if (m.HasAnimeDocument()) (Type, FileID, Ext) = (MediaType.Anime, m.Document!.FileId, m.Document.FileName.GetExtension(".gif"));
            else if (m.HasVideoDocument()) (Type, FileID, Ext) = (MediaType.Video, m.Document!.FileId, m.Document.FileName.GetExtension(".webm"));
            else return false;

            return true;
        }

        protected bool GetAudioFileID(Message m)
        {
            if      (m.Audio      != null) (Type, FileID, Ext) = (MediaType.Audio, m.Audio    .FileId, m.Audio   .FileName.GetExtension(".mp3"));
            else if (m.Voice      != null) (Type, FileID, Ext) = (MediaType.Audio, m.Voice    .FileId, ".ogg");
            else if (m.HasAudioDocument()) (Type, FileID, Ext) = (MediaType.Audio, m.Document!.FileId, m.Document.FileName.GetExtension(".wav"));
            else return false;

            return true;
        }

        protected bool GetPhotoFileID(Message m)
        {
            if      (m.Photo      != null) (Type, FileID, Ext) = (MediaType.Photo, m.Photo[^1].FileId, ".jpg");
            else if (m.HasImageSticker ()) (Type, FileID, Ext) = (MediaType.Stick, m.Sticker !.FileId, ".webp");
            else if (m.HasImageDocument()) (Type, FileID, Ext) = (MediaType.Photo, m.Document!.FileId, m.Document.FileName.GetExtension(".png"));
            else return false;

            return true;
        }

        // SEND

        protected void SendResult(string result)
        {
            using var stream = File.OpenRead(result);
            if      (Type == MediaType.Photo) Bot.SendPhoto    (Chat, new InputOnlineFile(stream));
            else if (Type == MediaType.Stick) Bot.SendSticker  (Chat, new InputOnlineFile(stream));
            else if (Type == MediaType.Audio) Bot.SendAudio    (Chat, new InputOnlineFile(stream, AudioFileName));
            else if (Type == MediaType.Anime) Bot.SendAnimation(Chat, new InputOnlineFile(stream, VideoFileName));
            else if (Type == MediaType.Video) Bot.SendVideo    (Chat, new InputOnlineFile(stream, VideoFileName));
            else if (Type == MediaType.Round) Bot.SendVideoNote(Chat, new InputOnlineFile(stream));
        }

        protected virtual string VideoFileName => "piece_fap_club.mp3";
        protected virtual string AudioFileName => "piece_fap_club.mp4";

        protected string Sender => Message.GetSenderName().ValidFileName();
        protected string SongNameOr(string s) => Message.GetSongNameOr(s);
    }
}