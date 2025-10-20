namespace PF_Bot.Features_Aux.Settings.Core;

public static class ChatSettingsFactory
{
    public static ChatSettings CreateFor(bool privateChat) => new()
    {
        Type     = GetRandomMemeType(),
        Quality  = 75,
        Speech   = (privateChat ? 100 : 15).ClampByte(),
        Pics     = (privateChat ? 100 : 20).ClampByte(),
        Stickers =  privateChat,
    };

    public static ChatSettings GetTemporary() => new()
    {
        Type     = GetRandomMemeType(),
        Quality  = 45,
        Speech   = 0,
        Pics     = 0,
        Stickers = false,
    };

    private static MemeType GetRandomMemeType() => Random.Shared.Next(4) switch
    {
        0 => MemeType.Meme,
        1 => MemeType.Snap,
        2 => MemeType.Dg,
        _ => MemeType.Dp,
    };
}