namespace Witlesss.Commands.Editing
{
    public class Reverse : AudioVideoCommand
    {
        protected override async Task Execute()
        {
            var (path, type) = await Bot.Download(FileID, Chat);
            
            SendResult(await FFMpegXD.Reverse(path), type);
            Log($"{Title} >> REVERSED [<<]");
        }
        
        protected override string AudioFileName => SongNameOr($"Kid Named {Sender}.mp3");
        protected override string VideoFileName { get; } = "piece_fap_reverse.mp4";
    }
}