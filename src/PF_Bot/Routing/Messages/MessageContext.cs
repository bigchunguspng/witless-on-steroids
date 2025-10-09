using Telegram.Bot.Types;

namespace PF_Bot.Routing.Messages;

public class MessageContext(Message message)
{
    public Message Message { get; } = message;
    public string  Title   { get; } = message.GetChatTitle();
    public string? Text    { get; } = message.GetTextOrCaption();

    public MessageOrigin Origin => Message.GetOrigin();
    public long          Chat   => Message.Chat.Id;
}