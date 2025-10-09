using PF_Bot.Features_Web.Piracy.Core;
using PF_Bot.Routing_Legacy.Commands;
using PF_Bot.Routing.Commands;
using Telegram.Bot.Types;

namespace PF_Bot.Features_Main.Edit.Commands
{
    public abstract class FileEditor : CommandHandlerAsync
    {
        [Flags]
        protected enum SupportedFileTypes
        {
            Photo = 1,
            Video = 2,
            Audio = 4,
            URL   = 8,
        }

        protected FileBase  File = null!;
        protected string    Ext  = null!;
        protected MediaType Type;

        // RUN

        protected override async Task Run()
        {
            var fileProvided =
                LocalFileProvided()
             || GetFileFrom(Message)
             || GetFileFrom(Message.ReplyToMessage);
            if (fileProvided) await Execute();
            else                 SendManual();
        }

        protected abstract Task Execute();

        protected void SendManual()
        {
            var manual = SyntaxManual == null
                ? string.Format(EDIT_MANUAL,     SuportedMedia)
                : string.Format(EDIT_MANUAL_SYN, SuportedMedia, SyntaxManual);
            SendManual(manual);
        }

        protected virtual string? SyntaxManual => null;
        protected abstract SupportedFileTypes SupportedTypes { get; }

        private string SuportedMedia => string.Join(", ", GetSuportedMediaIcons());

        private IEnumerable<string> GetSuportedMediaIcons()
        {
            if (SupportedTypes.HasFlag(SupportedFileTypes.Video)) yield return @"🎬";
            if (SupportedTypes.HasFlag(SupportedFileTypes.Photo)) yield return @"📸";
            if (SupportedTypes.HasFlag(SupportedFileTypes.Audio)) yield return @"🎧";
            if (SupportedTypes.HasFlag(SupportedFileTypes.URL  )) yield return @"📎";
        }

        // GET FILE

        private bool GetFileFrom(Message? message)
            => message != null && MessageContainsFile(message);

        protected virtual bool MessageContainsFile(Message m)
            => (SupportedTypes.HasFlag(SupportedFileTypes.Video) && GetVideoFileID(m))
            || (SupportedTypes.HasFlag(SupportedFileTypes.Photo) && GetPhotoFileID(m))
            || (SupportedTypes.HasFlag(SupportedFileTypes.Audio) && GetAudioFileID(m))
            || (SupportedTypes.HasFlag(SupportedFileTypes.URL  ) && GetVideoURL   (m));

        private bool LocalFileProvided()
        {
            if (Input.HasValue == false) return false;

            var input = Input.Value;
            Ext = input.Extension;

            if      (Ext is ".jpg" or ".png" or ".webp") Type = MediaType.Photo;
            else if (Ext is ".mp3" or ".ogg" or ".wav" ) Type = MediaType.Audio;
            else if (Ext is ".mp4" or ".gif" or ".webm") Type = MediaType.Video;
            else return false;

            GetFileStrategy = GetLocalFile;
            return true;
        }

        // GET FILE HELPERS

        private bool GetVideoFileID(Message m)
        {
            if      (m.Video      != null) (Type, File, Ext) = (MediaType.Video, m.Video    , ".mp4" );
            else if (m.Animation  != null) (Type, File, Ext) = (MediaType.Anime, m.Animation, ".mp4" );
            else if (m.VideoNote  != null) (Type, File, Ext) = (MediaType.Round, m.VideoNote, ".mp4" );
            else if (m.HasVideoSticker ()) (Type, File, Ext) = (MediaType.Anime, m.Sticker !, ".webm");
            else if (m.HasAnimeDocument()) (Type, File, Ext) = (MediaType.Anime, m.Document!, m.Document!.FileName.GetExtension_Or(".gif"));
            else if (m.HasVideoDocument()) (Type, File, Ext) = (MediaType.Video, m.Document!, m.Document!.FileName.GetExtension_Or(".webm"));
            else return false;

            GetFileStrategy = DownloadFile;
            return true;
        }

        private bool GetAudioFileID(Message m)
        {
            if      (m.Audio      != null) (Type, File, Ext) = (MediaType.Audio, m.Audio    , m.Audio    .FileName.GetExtension_Or(".mp3"));
            else if (m.Voice      != null) (Type, File, Ext) = (MediaType.Audio, m.Voice    , ".ogg");
            else if (m.HasAudioDocument()) (Type, File, Ext) = (MediaType.Audio, m.Document!, m.Document!.FileName.GetExtension_Or(".wav"));
            else return false;

            GetFileStrategy = DownloadFile;
            return true;
        }

        private bool GetPhotoFileID(Message m)
        {
            if      (m.Photo      != null) (Type, File, Ext) = (MediaType.Photo, m.Photo[^1], ".jpg");
            else if (m.HasImageSticker ()) (Type, File, Ext) = (MediaType.Stick, m.Sticker !, ".webp");
            else if (m.HasImageDocument()) (Type, File, Ext) = (MediaType.Photo, m.Document!, m.Document!.FileName.GetExtension_Or(".png"));
            else return false;

            GetFileStrategy = DownloadFile;
            return true;
        }

        protected bool GetAnyFileID(Message m)
        {
            if      (m.Photo     != null) (File, Ext) = (m.Photo[^1], ".jpg");
            else if (m.Audio     != null) (File, Ext) = (m.Audio    , m.Audio   .FileName.GetExtension_Or(".mp3"));
            else if (m.Video     != null) (File, Ext) = (m.Video    , ".mp4");
            else if (m.Animation != null) (File, Ext) = (m.Animation, ".mp4");
            else if (m.HasImageSticker()) (File, Ext) = (m.Sticker! , ".webp");
            else if (m.HasVideoSticker()) (File, Ext) = (m.Sticker! , ".webm");
            else if (m.Voice     != null) (File, Ext) = (m.Voice    , ".ogg");
            else if (m.VideoNote != null) (File, Ext) = (m.VideoNote, ".mp4");
            else if (m.Document  != null) (File, Ext) = (m.Document , m.Document.FileName.GetExtension_Or(".png"));
            else return false;

            GetFileStrategy = DownloadFile;
            return true;
        }

        private bool GetVideoURL(Message m)
        {
            var text = m.GetTextOrCaption();
            if (text is null) return false;

            var entity = m.GetURL();
            if (entity is null) return false;

            var url = text.Substring(entity.Offset, entity.Length);
            Type = MediaType.Video;
            Ext = ".mp4";
            GetFileStrategy = () => DownloadFileByURL(url);
            return true;
        }

        // DOWNLOAD / GET FILE

        private  Func<Task<FilePath>>             GetFileStrategy = null!;
        protected     Task<FilePath> GetFile() => GetFileStrategy.Invoke();

        //

        private       Task<FilePath> GetLocalFile() => Task.FromResult(Input!.Value);

        private async Task<FilePath> DownloadFile()
        {
            var path = await Bot.Download(File, Origin, Ext);
            if (path.FileSizeInBytes > 4_000_000)
            {
                MessageToEdit = Bot.PingChat(Origin, PROCESSING.PickAny().XDDD());
            }

            return path;
        }

        private async Task<FilePath> DownloadFileByURL(string url)
        {
            MessageToEdit = Bot.PingChat(Origin, PLS_WAIT[Random.Shared.Next(5)]);

            var path = await new DownloadVideoTask(url, Context).RunAsync();

            Bot.EditMessage(Chat, MessageToEdit, PROCESSING.PickAny().XDDD());

            return path;
        }

        // SEND

        protected void SendResult(string result)
        {
            if (MessageToEdit > 0)
            {
                Bot.DeleteMessageAsync(Chat, MessageToEdit);
                MessageToEdit = 0;
            }

            var name = Type == MediaType.Audio
                ? AudioFileName
                : VideoFileName;
            SendFile(result, Type, name);
        }

        protected virtual string VideoFileName => "piece_fap_bot.mp3";
        protected virtual string AudioFileName => "piece_fap_bot.mp4";

        protected string Sender => Message.GetSenderName().ValidFileName();
        protected string SongNameOr(string s) => Message.GetSongNameOr(s);
    }
}