using PF_Bot.Features_Main.Edit.Core;
using PF_Tools.FFMpeg;

namespace PF_Bot.Features_Main.Edit.Commands.Filter;

public class Equalize : FileEditor_AudioVideo
{
    protected override string SyntaxManual => "/man_eq";

    // /eq [frequency, Hz] [gain, dB] [width, Hz]
    protected override async Task Execute()
    {
        if (Args != null)
        {
            var args = Args.Split(' ').Take(3).ToArray();

            var frequency =                    args[0].TryParseF64_Invariant(out var v1) ? v1 : 100;
            var gain      = args.Length > 1 && args[1].TryParseF64_Invariant(out var v2) ? v2 : 15;
            var width     = args.Length > 2 && args[2].TryParseF64_Invariant(out var v3) ? v3 : 2000;

            var input = await GetFile();
            var output = input.GetOutputFilePath("EQ", Ext);

            await FFMpeg.Args()
                .Input(input)
                .Out(output, o => o
                    .AF($"equalizer=f={frequency}:g={gain}:t=h:width={width}")
                    .FixVideo_Playback()
                    .Options(FFMpegOptions.Out_cv_copy))
                .FFMpeg_Run();

            SendResult(output);
            Log($"{Title} >> EQ [{frequency} Hz, {gain} dB, {width} Hz]");
        }
        else
            SendManual(EQ_MANUAL);
    }

    protected override string AudioFileName => SongNameOr($"Bassboosted by {Sender}.mp3");
    protected override string VideoFileName => $"piece_fap_bot ft. DJ {Sender}.mp4";
}