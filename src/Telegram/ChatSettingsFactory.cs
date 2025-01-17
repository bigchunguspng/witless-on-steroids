using Witlesss.Commands.Meme.Core;

namespace Witlesss.Telegram;

public static class ChatSettingsFactory
{
    public static ChatSettings CreateFrom(CommandContext context)
    {
        var privateChat = context.ChatIsPrivate;
        return new ChatSettings()
        {
            Type     = GetRandomMemeType(),
            Quality  = 75,
            Speech   = (privateChat ? 100 : 15).ClampByte(),
            Pics     = (privateChat ? 100 : 20).ClampByte(),
            Stickers =  privateChat,
        };
    }

    private static MemeType GetRandomMemeType() => Random.Shared.Next(4) switch
    {
        0 => MemeType.Meme,
        1 => MemeType.Snap,
        2 => MemeType.Dg,
        _ => MemeType.Dp,
    };
}