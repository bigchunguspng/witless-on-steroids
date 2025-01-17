using Witlesss.Backrooms.Types.SerialQueue;
using Witlesss.Commands.Meme.Core;
using Witlesss.Memes;
using Witlesss.Memes.Shared;

namespace Witlesss.Commands.Meme;

public class Snap : MakeMemeCore<string>
{
    private static readonly SnapChat _snapChat = new();
    private static readonly SerialTaskQueue _queue = new();

    protected override SerialTaskQueue Queue => _queue;
    protected override IMemeGenerator<string> MemeMaker => _snapChat;

    protected override Regex _cmd { get; } = new(@"^\/snap(\S*)");

    protected override string VideoName => "piece_fap_bot-snap.mp4";

    protected override string Log_STR => "SNAP";
    protected override string Command => "/snap";
    protected override string Suffix  => "-Snap";

    protected override string? DefaultOptions => Data.Options?.Snap;


    protected override Task Run() => RunInternal("snap");

    protected override void ParseOptions()
    {
        throw new NotImplementedException();
    }

    protected override string GetMemeText(string? text)
    {
        throw new NotImplementedException();
    }
}