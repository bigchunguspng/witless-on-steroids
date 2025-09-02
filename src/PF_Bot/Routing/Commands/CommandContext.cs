using PF_Bot.Core.Chats;
using PF_Bot.Core.Generation;
using PF_Bot.Telegram;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PF_Bot.Routing.Commands;

public class CommandContext
{
    private static readonly Regex _command = new(@"^\/\S+");
    private static readonly string _botUsernameStart = Bot.Username.Remove(7);

    public Message Message      { get; }
    public string  Title        { get; }
    public string? Text         { get; private set; }
    /// <summary> Lowercase command with "/" symbol and bot username removed. </summary>
    public string? Command      { get; private set; }
    /// <summary> All text excluding the command and the following " " or "\n". </summary>
    public string? Args         { get; private set; }

    // todo remove 2 below - needed only in router
    /// <summary> Whether THIS bot was mentioned in the command explicitly or NO BOTS were mentioned. </summary>
    public bool    IsForMe      { get; private set; }
    /// <summary> Whether SOME bot was mentioned in the command explicitly. </summary>
    public bool    BotMentioned { get; private set; }

    public long    Chat => Message.Chat.Id;
    private int? Thread => Message.IsAutomaticForward  ? Message.Id              // channel post
                         : Message.IsTopicMessage      ? Message.MessageThreadId // forum thread message
                         : Message.ReplyToMessage?.Id ?? Message.MessageThreadId;

    public bool ChatIsPrivate => Message.Chat.Type == ChatType.Private;

    public MessageOrigin Origin => (Chat, Thread);

    protected CommandContext(CommandContext context)
    {
        Message = context.Message;
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
        Title = message.GetChatTitle();

        UseText(message.GetTextOrCaption());
    }

    public void UseText(string? text)
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

    private Copypaster?        _baka;
    public  Copypaster Baka => _baka ??= ChatManager.GetBaka(Chat);


    public static WitlessContext From(CommandContext context, ChatSettings baka) => new(context, baka);
    public static WitlessContext FromMessage(Message message, ChatSettings baka) => new(message, baka);

    private WitlessContext(CommandContext context, ChatSettings settings) : base(context) => Settings = settings;
    private WitlessContext(Message        message, ChatSettings settings) : base(message) => Settings = settings;
}