namespace PF_Bot.Terminal;

public class ConsoleContext
{
    public string Command { get; }
    public string? Args   { get; }

    private static readonly Regex
        _r_input = new(@"^\/(\S+)(?:\s+(.+))?", RegexOptions.Compiled);

    public ConsoleContext(string input)
    {
        var match = _r_input.Match(input);
        Command = match.ExtractGroup(1, s => s, "?");
        Args    = match.ExtractGroup(2, s => s);
    }
}