using PF_Bot.Routing.Commands;
using Telegram.Bot.Types;

namespace PF_Bot.Features.Edit.Core
{
    public abstract class FileEditingCommand : AsyncCommand
    {
        protected FileBase File = default!;
        protected string   Ext  = default!;
        protected MediaType Type;

        // RUN

        protected override async Task Run()
        {
            if (MessageHasSomethingToProcess()) await Execute();
        }

        protected abstract Task Execute();

        private bool MessageHasSomethingToProcess()
        {
            if (GetFileFrom(Message) || GetFileFrom(Message.ReplyToMessage)) return true;

            SendManual();
            return false;
        }

        protected void SendManual()
        {
            var manual = SyntaxManual is null
                ? string.Format(EDIT_MANUAL,     SuportedMedia)
                : string.Format(EDIT_MANUAL_SYN, SuportedMedia, SyntaxManual);
            Bot.SendMessage(Origin, manual);
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
            if      (m.Video      != null) (Type, File, Ext) = (MediaType.Video, m.Video    , ".mp4" );
            else if (m.Animation  != null) (Type, File, Ext) = (MediaType.Anime, m.Animation, ".mp4" );
            else if (m.VideoNote  != null) (Type, File, Ext) = (MediaType.Round, m.VideoNote, ".mp4" );
            else if (m.HasVideoSticker ()) (Type, File, Ext) = (MediaType.Anime, m.Sticker !, ".webm");
            else if (m.HasAnimeDocument()) (Type, File, Ext) = (MediaType.Anime, m.Document!, m.Document!.FileName.GetExtension_Or(".gif"));
            else if (m.HasVideoDocument()) (Type, File, Ext) = (MediaType.Video, m.Document!, m.Document!.FileName.GetExtension_Or(".webm"));
            else return false;

            return true;
        }

        protected bool GetAudioFileID(Message m)
        {
            if      (m.Audio      != null) (Type, File, Ext) = (MediaType.Audio, m.Audio    , m.Audio    .FileName.GetExtension_Or(".mp3"));
            else if (m.Voice      != null) (Type, File, Ext) = (MediaType.Audio, m.Voice    , ".ogg");
            else if (m.HasAudioDocument()) (Type, File, Ext) = (MediaType.Audio, m.Document!, m.Document!.FileName.GetExtension_Or(".wav"));
            else return false;

            return true;
        }

        protected bool GetPhotoFileID(Message m)
        {
            if      (m.Photo      != null) (Type, File, Ext) = (MediaType.Photo, m.Photo[^1], ".jpg");
            else if (m.HasImageSticker ()) (Type, File, Ext) = (MediaType.Stick, m.Sticker !, ".webp");
            else if (m.HasImageDocument()) (Type, File, Ext) = (MediaType.Photo, m.Document!, m.Document!.FileName.GetExtension_Or(".png"));
            else return false;

            return true;
        }

        protected Task<FilePath> DownloadFile() => Bot.Download(File, Origin, Ext);

        // SEND

        protected void SendResult(string result)
        {
            using var stream = System.IO.File.OpenRead(result);
            if      (Type == MediaType.Photo) Bot.SendPhoto    (Origin, InputFile.FromStream(stream));
            else if (Type == MediaType.Stick) Bot.SendSticker  (Origin, InputFile.FromStream(stream));
            else if (Type == MediaType.Audio) Bot.SendAudio    (Origin, InputFile.FromStream(stream, AudioFileName));
            else if (Type == MediaType.Anime) Bot.SendAnimation(Origin, InputFile.FromStream(stream, VideoFileName));
            else if (Type == MediaType.Video) Bot.SendVideo    (Origin, InputFile.FromStream(stream, VideoFileName));
            else if (Type == MediaType.Round) Bot.SendVideoNote(Origin, InputFile.FromStream(stream));
        }

        protected virtual string VideoFileName => "piece_fap_bot.mp3";
        protected virtual string AudioFileName => "piece_fap_bot.mp4";

        protected string Sender => Message.GetSenderName().ValidFileName();
        protected string SongNameOr(string s) => Message.GetSongNameOr(s);
    }
}