using System.Threading.Tasks;

#pragma warning disable CS4014

namespace Witlesss.Commands.Editing
{
    public class CutAsync : AsyncEditingCommand
    {
        protected override string MP3 => SongNameOr($"((({Sender}))).mp3");
        protected override string MP4 { get; } = "cut_fap_club.mp4";

        private readonly CutSpan Span;

        public CutAsync(MessageData message, string fileID, CutSpan span) : base(message, fileID)
        {
            Span = span;
        }

        public override async Task RunAsync()
        {
            await DownloadFileAsync();

            var result = Memes.Cut(Path, Span);

            Task.Run(() => Bot.DeleteMessage(Chat, WaitMessage));

            SendResult(result, Type);
            Log($"{Title} >> CUT [8K-]");
        }
    }
}