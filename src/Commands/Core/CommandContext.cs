using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Witlesss.Commands.Core;

public class CommandContext
{
    private static readonly Regex _command = new(@"^\/\S+");
    private static readonly string _botUsernameStart = Bot.Username.Remove(7);

    public Message Message      { get; }
    public long    Chat         { get; }
    public int?    Thread       { get; }
    public string  Title        { get; }
    public string? Text         { get; private set; }
    /// <summary> Lowercase command with "/" symbol and bot username removed. </summary>
    public string? Command      { get; private set; }
    /// <summary> All text excluding the command and the following " " or "\n". </summary>
    public string? Args         { get; private set; }
    /// <summary> Whether THIS bot was mentioned in the command explicitly or NO BOTS were mentioned. </summary>
    public bool    IsForMe      { get; private set; }
    /// <summary> Whether SOME bot was mentioned in the command explicitly. </summary>
    public bool    BotMentioned { get; private set; }

    public bool ChatIsPrivate => Message.Chat.Type == ChatType.Private;

    public MessageOrigin Origin => (Chat, Thread);

    protected CommandContext(CommandContext context)
    {
        Message = context.Message;
        Chat = context.Chat;
        Thread = context.Thread;
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
        Thread = message.IsAutomaticForward // channel post
            ? message.Id
            : message.IsTopicMessage        // forum thread message
                ? message.MessageThreadId
                : message.ReplyToMessage?.Id
               ?? message.MessageThreadId;

        SetTextItems(message.GetTextOrCaption());
    }

    public void ChangeText(string text) => SetTextItems(text);

    private void SetTextItems(string? text)
    {
        Text = text;

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
    public ChatSettings Settings { get; }

    private CopypasterProxy?        _baka;
    public  CopypasterProxy Baka => _baka ??= ChatService.GetBaka(Chat);


    public static WitlessContext From(CommandContext context, ChatSettings baka) => new(context, baka);
    public static WitlessContext FromMessage(Message message, ChatSettings baka) => new(message, baka);

    private WitlessContext(CommandContext context, ChatSettings settings) : base(context) => Settings = settings;
    private WitlessContext(Message        message, ChatSettings settings) : base(message) => Settings = settings;
}