using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Witlesss.Commands.Core;

public class CommandContext
{
    private static readonly Regex _command = new(@"^\/\S+", RegexOptions.IgnoreCase);
    private static readonly string _botUsernameStart = Bot.Username.Remove(7);

    public Message Message  { get; }
    public long    Chat     { get; }
    public string  Title    { get; }
    public string? Text     { get; }
    public string? Command  { get; }
    public string? Args     { get; }
    public bool    IsForMe  { get; }

    public bool ChatIsPrivate => Message.Chat.Type == ChatType.Private;

    private readonly bool _noBotMentioned;
    public bool BotMentioned => !_noBotMentioned;


    protected CommandContext(CommandContext context)
    {
        Message = context.Message;
        Chat = context.Chat;
        Title = context.Title;
        Text = context.Text;
        Command = context.Command;
        Args = context.Args;
        IsForMe = context.IsForMe;

        _noBotMentioned = context._noBotMentioned;
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
            var lower = match.Value.ToLower();
            Command = lower.Replace(Bot.Username, "");
            _noBotMentioned = !lower.Contains('@') || !lower.Contains("bot");
            IsForMe = _noBotMentioned || lower.Contains(_botUsernameStart);

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