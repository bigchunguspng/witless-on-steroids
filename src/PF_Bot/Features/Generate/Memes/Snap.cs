using PF_Bot.Backrooms.Types.SerialQueue;
using PF_Bot.Features.Generate.Memes.Core;
using PF_Bot.Tools_Legacy.MemeMakers;
using PF_Bot.Tools_Legacy.MemeMakers.Shared;
using static PF_Bot.Backrooms.Helpers.OptionsParsing;

namespace PF_Bot.Features.Generate.Memes;

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

    protected override bool ResultsAreRandom => SnapChat.RandomOffset || SnapChat.FontWizard.UseRandom;

    protected override void ParseOptions()
    {
        SnapChat.RandomOffset = CheckAndCut(Request, _random);

        SnapChat.CustomColorBack.CheckAndCut(Request);
        SnapChat.CustomColorText.CheckAndCut(Request);
        SnapChat.FontWizard     .CheckAndCut(Request);

        SnapChat.MinSizeMultiplier  = GetInt(Request, _fontMS,  10, group: 2);
        SnapChat.FontSizeMultiplier = GetInt(Request, _fontSM, 100);
        SnapChat.CardOpacity        = GetInt(Request, _opacity, 62);
        SnapChat.CardOffset         = GetInt(Request, _offset,  50);

        SnapChat.WrapText = !CheckAndCut(Request, _nowrap);
    }

    protected override string GetMemeText(string? text)
    {
        var generate = string.IsNullOrEmpty(text);
        var capitalize = CheckCaps(Request, _caps, generate);

        var caption = generate ? Baka.Generate() : text!;

        return capitalize ? caption.InLetterCase(LetterCase.Upper) : caption;
    }

    private static readonly Regex _random  = new(@"^\/snap\S*(!!)\S*");
    private static readonly Regex _opacity = new(@"^\/snap\S*?(\d{1,3})(%)\S*");
    private static readonly Regex _offset  = new(@"^\/snap\S*?(\d{1,3})(!)\S*");
    private static readonly Regex _fontSM  = new(@"^\/snap\S*?(\d{1,3})("")\S*");
    private static readonly Regex _fontMS  = new(@"^\/snap\S*?(min)(\d{1,3})("")\S*");
}