using Telegram.Bot.Types;

namespace PF_Bot.Routing.Messages;

public static class AutoHandler
{
    private static readonly Dictionary<long, AutoHandlerScript> Cache = new(32);

    public static void ClearCache(long chat)
    {
        Cache.Remove(chat);
    }

    // EXPRESSION PARSING

    /// Returns command-like input string in this format: <c>cmd[ops] [args]</c>
    public static string? TryGetHandlerInput(MessageContext context, string expression)
    {
        if (Cache.TryGetValue_Failed(context.Chat, out var script))
        {
            script = AutoHandlerScript.Create(expression);
            Cache.Add(context.Chat, script);
        }

        foreach (var type in script.SupportedFileTypes)
        {
            if (MessageMatches(type, context.Message).Janai())
                continue;

            var input = script.GenerateInput(type);
            if (input != null && type == 'u')
            {
                var split = input.SplitN(2);
                var command = split[0];
                var args = split.Length > 1 ? split[1] : null;

                input = $"{command} {GetURL(context)} {args}"; // e.g. /cut URL 300
            }

            return input;
        }

        return null;
    }

    // MEDIA

    private static bool MessageMatches(char type, Message message) => type switch
    {
        'p' => CheckPhoto(message),
        'v' => CheckVideo(message),
        'a' => CheckAudio(message),
        'g' => CheckGIF  (message),
        'u' => CheckURL  (message),
        's' => CheckStick(message),
        'd' => CheckDoc  (message),
        _ => throw new ArgumentException(),
    };

    private static bool CheckPhoto
        (Message message)
        => message.Photo != null
        || message.HasImageDocument();

    private static bool CheckVideo
        (Message message)
        => message.Video     != null
        || message.VideoNote != null
        || message.HasVideoDocument();

    private static bool CheckAudio
        (Message message)
        => message.Audio != null
        || message.Voice != null
        || message.HasAudioDocument();

    private static bool CheckGIF
        (Message message)
        => message.Animation != null
        || message.HasVideoSticker()
        || message.HasAnimeDocument();

    private static bool CheckURL
        (Message message)
        => message.GetTextOrCaption() != null
        && message.GetURL() != null;

    private static bool CheckStick
        (Message message)
        => message.HasImageSticker();

    private static bool CheckDoc
        (Message message)
        => message.Document != null;

    private static string GetURL(MessageContext ctx)
    {
        var url = ctx.Message.GetURL()!;
        return ctx.Text!.Substring(url.Offset, url.Length);
    }
}