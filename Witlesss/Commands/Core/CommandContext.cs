﻿using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Witlesss.Commands.Core;

public class CommandContext
{
    private static readonly Regex _command = new(@"^\/\S+", RegexOptions.IgnoreCase);

    protected CommandContext(CommandContext context)
    {
        Message = context.Message;
        Chat = context.Chat;
        Title = context.Title;
        Text = context.Text;
        Command = context.Command;
        Args = context.Args;
        IsForMe = context.IsForMe;
    }

    public CommandContext(Message message)
    {
        Message = message;
        Chat = message.Chat.Id;
        Title = message.GetChatTitle();

        Text = message.GetTextOrCaption();
        if (Text is null) return;

        var match = _command.Match(Text);
        if (match.Success)
        {
            var lower = match.Value.ToLower();
            Command = lower.Replace(Config.BOT_USERNAME, "");
            IsForMe = !lower.Contains('@') || !lower.Contains("bot") || lower.Contains(Config.BOT_USERNAME.Remove(7));

            Args = match.Length == Text.Length ? null : Text.Substring(match.Length + 1);
        }
        else
        {
            Args = Text;
        }
    }

    public Message Message  { get; set; }
    public long    Chat     { get; set; }
    public string  Title    { get; set; }
    public string? Text     { get; set; }
    public string? Command  { get; set; }
    public string? Args     { get; set; }
    public bool    IsForMe  { get; set; }

    public bool ChatIsPrivate => Message.Chat.Type == ChatType.Private;

    /*public (string? text, long chat, string title) Deconstruct()
    {
        return (Text, Chat, Title);
    }*/
}

public class WitlessContext(CommandContext context, Witless baka) : CommandContext(context)
{
    public Witless Baka { get; set; } = baka;

    /*public (Witless baka, string? text, long chat, string title) Deconstruct()
    {
        return (Baka, Text, Chat, Title);
    }*/
}