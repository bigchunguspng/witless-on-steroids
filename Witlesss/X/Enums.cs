namespace Witlesss.X
{
    public enum ColorMode { Color, White }

    public enum DgMode    { Square, Wide }

    public enum SpeedMode { Fast, Slow   }

    public enum SortingMode
    {
        Hot, New, Top, Rising, Controversial    // reddit
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

    public enum MemeType  { Dg, Meme }
}