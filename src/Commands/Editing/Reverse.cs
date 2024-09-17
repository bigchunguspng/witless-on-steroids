namespace Witlesss.Commands.Editing
{
    public class Reverse : AudioVideoCommand
    {
        protected override async Task Execute()
        {
            var path = await DownloadFile();

            SendResult(await path.UseFFMpeg(Chat).Reverse().Out("-Reverse", Ext));
            Log($"{Title} >> REVERSED [<<]");
        }
        
        protected override string AudioFileName => SongNameOr($"Kid Named {Sender}.mp3");
        protected override string VideoFileName { get; } = "piece_fap_bot-reverse.mp4";
    }
}