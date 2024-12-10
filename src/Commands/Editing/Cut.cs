namespace Witlesss.Commands.Editing
{
    public class Cut : AudioVideoUrlCommand
    {
        protected override string SyntaxManual => "/man_cut";

        protected override async Task Execute()
        {
            var args = Args?.Split().SkipWhile(x => x.StartsWith('/') || x.StartsWith("http")).ToArray();

            var x = ArgumentParsing.GetCutTimecodes(args);
            if (x.failed)
            {
                Bot.SendMessage(Origin, CUT_MANUAL);
                return;
            }

            var span = new CutSpan(x.start, x.length);

            var (path, waitMessage) = await DownloadFileSuperCool();

            var result = await path.UseFFMpeg(Origin).Cut(span).Out("-Cut", Ext);

            Bot.DeleteMessageAsync(Chat, waitMessage);

            SendResult(result);
            Log($"{Title} >> CUT [8K-]");
        }

        protected override string VideoFileName => "piece_fap_bot-cut.mp4";
        protected override string AudioFileName => SongNameOr($"((({Sender}))).mp3");
    }
}