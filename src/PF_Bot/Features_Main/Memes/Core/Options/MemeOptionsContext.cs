namespace PF_Bot.Features_Main.Memes.Core.Options;

public class MemeOptionsContext(bool empty, string buffer, string? options, string? commandOptions)
{
    /// <b>True</b> if both message text and chat default options are null. <br/>
    /// In such case, default values are used.
    public readonly bool Empty = empty;

    /// A combination of command and default options. <br/>
    /// Can be modified during parsing!
    public string Buffer = buffer;

    /// Options used. Useful for logging.
    public readonly string? Options = options;

    /// Options provided via command (opposed to default options).
    public readonly string? CommandOptions = commandOptions;
}