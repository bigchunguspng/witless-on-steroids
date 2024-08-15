using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Witlesss.Commands.Core;

public class CommandContext
{
    private static readonly Regex _command = new(@"^\/\S+");
    private static readonly string _botUsernameStart = Bot.Username.Remove(7);

    public Message Message      { get; }
    public long    Chat         { get; }
    public string  Title        { get; }
    public string? Text         { get; }
    /// <summary> Lowercase command with "/" symbol and bot username removed. </summary>
    public string? Command      { get; }
    /// <summary> All text excluding the command and the following " " or "\n". </summary>
    public string? Args         { get; }
    /// <summary> True if THIS bot was mentioned in the command explicitly or NO BOTS were mentioned. </summary>
    public bool    IsForMe      { get; }
    /// <summary> True if SOME bot was mentioned in the command explicitly. </summary>
    public bool    BotMentioned { get; }

    public bool ChatIsPrivate => Message.Chat.Type == ChatType.Private;


    protected CommandContext(CommandContext context)
    {
        Message = context.Message;
        Chat = context.Chat;
        Title = context.Title;
        Text = context.Text;
        Command = context.Command;
        Args = context.Args;
        IsForMe = context.IsForMe;
        BotMentioned = context.BotMentioned;
    }
    
    protected CommandContext(Message message)
    {
        Message = message;
        Chat = message.Chat.Id;
        Title = message.GetChatTitle();
        Text = message.GetTextOrCaption();

        var match = _command.MatchOrNull(Text);
        if (match is { Success: true })
        {
            var command = match.Value.ToLower();
            Command = command.Replace(Bot.Username, "");
            var indexA = command.IndexOf('@');
            var indexB = command.LastIndexOf("bot", StringComparison.Ordinal);
            BotMentioned = indexA > 0 && indexB > 0 && indexB > indexA;
            IsForMe = !BotMentioned || command.Contains(_botUsernameStart);

            Args = match.Length == Text!.Length ? null : Text.Substring(match.Length + 1);
        }
        else
        {
            Args = Text;
        }
    }

    public static CommandContext FromMessage(Message message) => new(message);
}

public class WitlessContext : CommandContext
{
    public Witless Baka { get; }

    private WitlessContext(CommandContext context, Witless baka) : base(context) => Baka = baka;
    private WitlessContext(Message        message, Witless baka) : base(message) => Baka = baka;

    public static WitlessContext From(CommandContext context, Witless baka) => new(context, baka);
    public static WitlessContext FromMessage(Message message, Witless baka) => new(message, baka);
}