using PF_Tools.FFMpeg;

namespace PF_Bot.Features_Main.Edit.Core;

public record struct RandomizeOptions
(
    (int from, int to) rep_range,
    (int from, int to) nuke_range,
    byte rep_pc,
    byte nuke_pc,
    bool sorted
);

public class FFMpeg_Randomize(string input, FFProbeResult probe)
{
    public FFMpegArgs ApplyEffects
        (RandomizeOptions options, TimeSelection selection = default)
    {
        throw new NotImplementedException("You must be at least lvl 15 to use this feature!");
    }
}