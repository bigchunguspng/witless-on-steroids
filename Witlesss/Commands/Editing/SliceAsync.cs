using System.Threading.Tasks;

#pragma warning disable CS4014

namespace Witlesss.Commands.Editing
{
    public class SliceAsync : AsyncEditingCommand
    {
        protected override string MP3 { get; } = "sliced_by_piece_fap_bot.mp3";
        protected override string MP4 { get; } = "piece_fap_slice.mp4";

        public SliceAsync(MessageData message, string fileID) : base(message, fileID) { }

        public override async Task RunAsync()
        {
            await DownloadFileAsync();

            var result = Memes.Slice(Path);

            Task.Run(() => Bot.DeleteMessage(Chat, WaitMessage));

            SendResult(result, Type);
            Log($"{Title} >> SLICED [~/~]");
        }
    }
}