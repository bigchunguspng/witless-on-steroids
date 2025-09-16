namespace PF_Bot.Handlers.Memes.Core;

public class MemeRequest(string dummy, bool empty, string command, string? options)
{
    /// <b>True</b> if both message text and default options are null.
    public readonly bool Empty = empty;

    /// Lowercase command text w/o bot username.
    public readonly string Command = command;

    /// A combination of command and default options.
    public string Dummy = dummy;
    
    /// Options used.
    public readonly string? Options = options;
}