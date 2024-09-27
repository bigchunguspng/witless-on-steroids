using Witlesss.Commands.Meme.Core;

namespace Witlesss;

public static class ChatSettingsFactory
{
    public static ChatSettings CreateFrom(CommandContext context)
    {
        var privateChat = context.ChatIsPrivate;
        return new ChatSettings(context.Chat)
        {
            Type     = GetRandomMemeType(),
            Quality  = 75,
            Speech   = (privateChat ? 100 : 15).ClampByte(),
            Pics     = (privateChat ? 100 : 20).ClampByte(),
            Stickers =  privateChat,
        };
    }

    private static MemeType GetRandomMemeType()
    {
        var chance = Random.Shared.Next(4);
        return chance.IsEven()
            ? MemeType.Meme     // 50%
            : (chance >> 1).IsEven()
                ? MemeType.Dg   // 25%
                : MemeType.Dp;  // 25%
    }
}