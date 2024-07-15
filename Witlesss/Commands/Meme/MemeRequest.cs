namespace Witlesss.Commands.Meme;

public class MemeRequest(string dummy, bool empty, string command)
{
    /// <summary>
    /// <b>True</b> if both message text and default options are null.
    /// </summary>
    public readonly bool Empty = empty;

    /// <summary>
    /// Lowercase command text w/o bot username.
    /// </summary>
    public readonly string Command = command;

    /// <summary>
    /// A combination of command and default options.
    /// </summary>
    public string Dummy = dummy;
}