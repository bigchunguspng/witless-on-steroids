using PF_Bot.Core;
using PF_Bot.Routing.Messages;
using Telegram.Bot.Types;

namespace PF_Bot.Routing.Commands;

/// Expected syntax: <c>/[command][ops][@bot][ops] [args]</c>
public class CommandContext : MessageContext
{
    /// Command name, lowercase. Options, '/', and bot mention are removed.
    public string  Command { get; }

    /// Command options, lowercase. Can contain '▒' (in the middle) if bot was mentioned.
    public string? Options { get; }

    /// Command arguments, case preserved.
    public string? Args    { get; }

    /// Local file to process. Used in pipes (chained command handlers).
    public FilePath? Input { get; }

    /// Whether THIS bot was mentioned
    public bool BotMentioned { get; }

    /// This constructor expects message text to start with "/command".
    public CommandContext(Message message, string command) : base(message)
    {
        Command = command;

        var command_end = 1 + command.Length;
        var space_index = Text!.IndexOfAny(_separators, command_end);
        if (space_index < 0)
            space_index = Text.Length;
        else
            Args = Text.Substring(space_index + 1);

        var options_length = space_index - command_end;
        if (options_length > 0)
        {
            var mention_start = Text.IndexOf(_botUsername, command_end, StringComparison.OrdinalIgnoreCase);
            if (mention_start < 0)
            {
                Options = Text.Substring(command_end, options_length);
            }
            else
            {
                BotMentioned = true;

                var mention_length = _botUsername.Length;
                if (mention_length == options_length)
                    return; // no options, just bot mention

                var mention_end = mention_start + mention_length;

                var op1_start = command_end;
                var op2_start = mention_end;
                var op1_length = mention_start - command_end;
                var op2_length =   space_index - mention_end;
                var op1 = Text.Substring(op1_start, op1_length);
                var op2 = Text.Substring(op2_start, op2_length);
                if (op1.Length + op2.Length > 0)
                {
                    Options =
                        op2.Length == 0 ? op1 : 
                        op1.Length == 0 ? op2 : $"{op1}▒{op2}";
                }
            }
        }
    }

    private static readonly char[] _separators  = [' ', '\n'];
    private static readonly string _botUsername = App.Bot.Username;
}