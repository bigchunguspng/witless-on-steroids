using PF_Bot.Features_Main.Edit.Core;
using PF_Tools.FFMpeg;

namespace PF_Bot.Features_Main.Edit.Commands.Filter;

public class Volume : FileEditor_AudioVideo
{
    private string _arg = null!;

    protected override string SyntaxManual => "/man_vol";

    protected override async Task Execute()
    {
        if (Args != null)
        {
            _arg = Args.Split(' ', 2)[0];

            var input = await GetFile();
            var output = input.GetOutputFilePath("vol", Ext);

            await FFMpeg.Args()
                .Input(input)
                .Out(output, o => o
                    .AF($"volume='{_arg}':eval=frame")
                    .FixVideo_Playback()
                    .Options(FFMpegOptions.Out_cv_copy))
                .FFMpeg_Run();

            SendResult(output);
            Log($"{Title} >> VOLUME [{_arg}]");
        }
        else
            SendManual(VOLUME_MANUAL);
    }

    protected override string AudioFileName => SongNameOr($"{Sender} Sound Effect.mp3");
    protected override string VideoFileName => _arg.Length < 4 ? $"VOLUME-{_arg.ValidFileName()}.mp4" : "VERY-LOUD-ICE-CREAM.mp4";
}