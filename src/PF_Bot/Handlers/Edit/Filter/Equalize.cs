using PF_Bot.Core.Editing;
using PF_Tools.FFMpeg;

namespace PF_Bot.Handlers.Edit.Filter
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

                var input = await DownloadFile();
                var output = input.GetOutputFilePath("EQ", Ext);

                await FFMpeg.Args()
                    .Input(input)
                    .Out(output, o => o
                        .AF($"equalizer=f={f}:g={g}:t=h:width={w}")
                        .FixVideo_Playback()
                        .Options(FFMpegOptions.Out_cv_copy))
                    .FFMpeg_Run();

                SendResult(output);
                Log($"{Title} >> EQ [{f} Hz, {g} dB, {w} Hz]");
            }
        }

        protected override string AudioFileName => SongNameOr($"Bassboosted by {Sender}.mp3");
        protected override string VideoFileName => $"piece_fap_bot ft. DJ {Sender}.mp4";
    }
}