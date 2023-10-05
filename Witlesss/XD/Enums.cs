namespace Witlesss.XD
{
    public enum ColorMode { Color, White }

    public enum DgMode    { Square, Wide }

    public enum SpeedMode { Fast, Slow   }

    public enum SortingMode
    {
        Hot = 'h',  New = 'n',  Top = 't',
        Rising = 'r', Controversial = 'c'       // reddit
    }

    public enum LetterCaseMode
    {
        Lower, Upper, Sentence         // text formatting
    }

    public enum MediaType
    {
        Audio,
        Video,
        Movie, // video with sound
        Round  // video note
    }

    public enum MemeType  { Dg, Meme, Top, Dp }
}