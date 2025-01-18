using Witlesss.Backrooms.Types.SerialQueue;
using Witlesss.Commands.Meme.Core;
using Witlesss.Memes;
using Witlesss.Memes.Shared;
using static Witlesss.Backrooms.Helpers.OptionsParsing;

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
        SnapChat.CustomColorBack.CheckAndCut(Request);
        SnapChat.CustomColorText.CheckAndCut(Request);
        SnapChat.FontWizard .CheckAndCut(Request);

        SnapChat.MinSizeMultiplier  = GetInt(Request, _fontMS,  10, group: 2);
        SnapChat.FontSizeMultiplier = GetInt(Request, _fontSM, 100);
        SnapChat.CardOpacity        = GetInt(Request, _opacity, 62);
        SnapChat.CardOffset         = GetInt(Request, _offset,  50);

        SnapChat.WrapText         = !CheckAndCut(Request, _nowrap );
        SnapChat.BackInBlack      =  CheckAndCut(Request, _blackBG);
    }

    protected override string GetMemeText(string? text)
    {
        var generate = string.IsNullOrEmpty(text);
        var capitalize = CheckCaps(Request, _caps, generate);

        var caption = generate ? Baka.Generate() : text!;

        return capitalize ? caption.InLetterCase(LetterCase.Upper) : caption;
    }

    private static readonly Regex _blackBG = new(@"^\/snap\S*(ob)\S*");
    private static readonly Regex _opacity = new(@"^\/snap\S*?(\d{1,3})(%)\S*");
    private static readonly Regex _offset  = new(@"^\/snap\S*?(\d{1,3})(!)\S*");
    private static readonly Regex _fontSM  = new(@"^\/snap\S*?(\d{1,3})("")\S*");
    private static readonly Regex _fontMS  = new(@"^\/snap\S*?(min)(\d{1,3})("")\S*");

    // options:
    // - bg opacity 0-100, 62%
    // - offset up down, 0-100, 50%
    // random offset
}