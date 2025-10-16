using PF_Bot.Features_Main.Edit.Core;
using PF_Tools.FFMpeg;

namespace PF_Bot.Features_Main.Edit.Commands.Filter;

public class Sus : FileEditor_AudioVideo
{
    protected override string SyntaxManual => "/man_sus";

    protected override async Task Execute()
    {
        var parsing = (Args?.Split()).GetCutTimecodes(out var start, out var length);
        if (parsing.Failed() && Args != null)
        {
            SendManual(SUS_MANUAL);
            return;
        }

        var input = await GetFile();
        var (output, probe, options) = await input.InitEditing("Sus", Ext);

        length = Args == null             ? probe.Duration / 2D
            : (start + length).Ticks <= 0 ? probe.Duration
            : length;

        var args = FFMpeg.Command(input, output, options.Fix_AudioVideo(probe));

        if (probe.HasVideo) AddSusFilter(args, start, length, "v",  "", "v=1");
        if (probe.HasAudio) AddSusFilter(args, start, length, "a", "a", "v=0:a=1");

        await args.FFMpeg_Run();

        SendResult(output);
        Log($"{Title} >> SUS [>_<]");
    }

    private void AddSusFilter
    (
        FFMpegArgs args, TimeSpan start, TimeSpan length,
        string av, string a, string concat
    )
    {
        var ss =  start.TotalSeconds;
        var ls = length.TotalSeconds;
        args
            .Filter($"[0:{av}]{a}trim=start={ss}:duration={ls},{a}setpts=PTS-STARTPTS,{a}split=2[{av}0][{av}1]")
            .Filter($"[{av}1]{a}reverse,{a}setpts=PTS-STARTPTS[{av}r]")
            .Filter($"[{av}0][{av}r]concat=n=2:{concat}");
    }

    protected override string AudioFileName => SongNameOr($"Kid Named {WhenTheSenderIsSus()}.mp3");
    protected override string VideoFileName => "piece_fap_bot-sus.mp4";

    private string WhenTheSenderIsSus() => Sender.Length > 2 ? Sender[..2] + Sender[0] : Sender;
}