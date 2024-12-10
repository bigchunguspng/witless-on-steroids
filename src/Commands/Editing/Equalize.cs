namespace Witlesss.Commands.Editing
{
    public class Equalize : AudioVideoCommand
    {
        protected override string SyntaxManual => "/man_eq";

        // /eq [frequency, Hz] [gain, dB] [width, Hz]
        protected override async Task Execute()
        {
            if (Args is null)
            {
                Bot.SendMessage(Origin, EQ_MANUAL);
            }
            else
            {
                var args = Args.Split(' ').Take(3).ToArray();

                var f = double.TryParse(args[0], out var v1) ? v1 : 100;
                var g = double.TryParse(args.Length > 1 ? args[1] : "", out var v2) ? v2 : 15;
                var w = double.TryParse(args.Length > 2 ? args[2] : "", out var v3) ? v3 : 2000;

                var path = await DownloadFile();

                SendResult(await path.UseFFMpeg(Origin).EQ([f, g, w]).Out("-EQ", Ext));
                Log($"{Title} >> EQ [{f} Hz, {g} dB, {w} Hz]");
            }
        }

        protected override string AudioFileName => SongNameOr($"Bassboosted by {Sender}.mp3");
        protected override string VideoFileName => $"piece_fap_bot ft. DJ {Sender}.mp4";
    }
}