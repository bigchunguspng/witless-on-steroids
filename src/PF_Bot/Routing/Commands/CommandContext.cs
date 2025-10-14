using System.Text.Json;
using PF_Bot.Core;
using PF_Bot.Features_Aux.Packs.Core;
using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Features_Main.Text.Core;
using PF_Bot.Routing.Messages;
using Telegram.Bot.Types;

namespace PF_Bot.Routing.Commands;

/// Expected syntax: <c>/[command][ops][@bot][ops] [args]</c>
public class CommandContext : MessageContext
{
    // COMMON

    /// Command name, lowercase. Options, '/', and bot mention are removed.
    public string  Command   { get; private set; }

    /// Command options, lowercase. Can contain '▒' (in the middle) if bot was mentioned.
    public string? Options   { get; private set; }

    /// Command arguments, case preserved.
    public string? Args      { get; private set; }

    /// Whether THIS bot was mentioned
    public bool BotMentioned { get; private set; }

    // PIPES

    /// Local file to process.
    /// Used in pipes (chained command handlers).
    public      FilePath ? Input  { get; set; }

    /// Processing results.
    /// Used in pipes (chained command handlers).
    public List<FilePath>? Output { get; set; }

    // KNOWN CHATS

    private ChatSettings?            _settings;
    public  ChatSettings Settings => _settings ??=
        ChatManager.Knowns(Chat, out var settings)
            ? settings
            : ChatSettingsFactory.GetTemporary();

    private Copypaster?        _baka;
    public  Copypaster Baka => _baka ??=
        ChatManager.Knowns(Chat)
            ? PackManager.GetBaka(Chat)
            : DementiaCopypaster.Instance;

    public  ChatSettings? Settings_Debug => _settings;
    public  Copypaster? Copypaster_Debug => _baka;

    // CTORS

    /// Automemes
    public CommandContext(Message message) : base(message)
    {
        Command = "@";
        Args = Text;
    }

    /// Autohandler
    public CommandContext(Message message, string command, string expression) : base(message)
    {
        Command = command;
        ParseText(0, command.Length, expression);
    }

    /// This constructor expects message text to start with "/command".
    public CommandContext(Message message, string command) : base(message)
    {
        Command = command;
        ParseText(1, command.Length, Text!);
    }

    private void ParseText(int command_offset, int command_length, string text)
    {
        var command_end = command_offset + command_length;
        var space_index = text.IndexOfAny(_separators, command_end);
        if (space_index < 0)
            space_index = text.Length;
        else
            Args = text.Substring(space_index + 1);

        var options_length = space_index - command_end;
        if (options_length > 0)
        {
            var mention_start = text.IndexOf(_botUsername, command_end, StringComparison.OrdinalIgnoreCase);
            if (mention_start < 0)
            {
                Options = text.Substring(command_end, options_length);
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
                var op1 = text.Substring(op1_start, op1_length);
                var op2 = text.Substring(op2_start, op2_length);
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

public class CommandContextJsonConverter : WriteOnlyJsonConverter<CommandContext>
{
    public override void Write
    (
        Utf8JsonWriter writer,
        CommandContext value,
        JsonSerializerOptions options
    ) => writer.WriteObject(() =>
    {
        writer.WriteObject("message", value.Message, options);
        writer.WriteNumber("chat", value.Chat);
        writer.WriteString("title", value.Title);
        writer.WriteString("text", value.Text);
        writer.WriteString("command", value.Command);
        writer.WriteString("options", value.Options);
        writer.WriteString("args", value.Args);
        writer.WriteBoolean("bot_mentioned", value.BotMentioned);

        if (value.Input != null)
            writer.WriteString("input", value.Input);

        if (value.Output != null)
            writer.WriteArray("output", () => value.Output.ForEach(x => writer.WriteStringValue(x)));

        if (value.Settings_Debug != null)
            writer.WriteObject("settings", value.Settings_Debug, options);

        if (value.Copypaster_Debug != null)
            writer.WriteObject("baka", () =>
            {
                var baka = value.Copypaster_Debug;
                var pack = value.Copypaster_Debug.Pack;
                writer.WriteNumber ("idle",  baka.Idle);
                writer.WriteBoolean("dirty", baka.IsDirty);
                writer.WriteNumber("count_special",    pack.SpecialCount);
                writer.WriteNumber("count_ordinal",    pack.OrdinalCount);
                writer.WriteNumber("count_vocabulary", pack.VocabularyCount);
            });
    });
}