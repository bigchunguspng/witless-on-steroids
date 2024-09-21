namespace Witlesss.Commands.Editing
{
    public class Sus : AudioVideoCommand
    {
        protected override string SyntaxManual => "/man_sus";

        protected override async Task Execute()
        {
            var argless = false;
            var x = ArgumentParsing.GetCutTimecodes(Args?.Split());
            if (x.failed)
            {
                if (Args is not null)
                {
                    Bot.SendMessage(Chat, SUS_MANUAL);
                    return;
                }
                argless = true;
            }

            var span = new CutSpan(x.start, x.length);

            var path = await DownloadFile();

            if (argless) x.length = TimeSpan.MinValue;

            var result = await path.UseFFMpeg(Chat).Sus(span).Out("-Sus", Ext);
            SendResult(result);
            Log($"{Title} >> SUS [>_<]");
        }

        protected override string AudioFileName => SongNameOr($"Kid Named {WhenTheSenderIsSus()}.mp3");
        protected override string VideoFileName => "piece_fap_bot-sus.mp4";

        private string WhenTheSenderIsSus() => Sender.Length > 2 ? Sender[..2] + Sender[0] : Sender;
    }
}