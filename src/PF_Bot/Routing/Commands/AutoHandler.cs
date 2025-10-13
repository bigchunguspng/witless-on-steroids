using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Routing.Messages;
using Telegram.Bot.Types;

namespace PF_Bot.Routing.Commands;

// auto = [expr 0][;] [expr N]
// expr = [types][N%]:[pipe]
// pipe = [section 0][ > ][section N]
// sect = [command][options] [args]

public static class AutoHandler
{
    private static readonly Regex
        _rgx_handler = new(@"([pvagus]+)(\d{1,3}%)?:\s*(.+)", RegexOptions.Compiled);

    // todo investigate: cache has duplicate entries ?
    private static readonly LimitedCache<long, Dictionary<char, List<(int Percent, string Command)>>> Cache = new(32);

    public static void ClearCache(long chat)
    {
        if (Cache.Contains(chat, out var dictionary)) dictionary.Clear();
    }

    public static string? TryGetMessageHandler(MessageContext context, ChatSettings data)
    {
        var expression = data.Options![MemeType.Auto];
        if (expression is null) return null;

        var handlers = Cache.Contains(context.Chat, out var cached) && cached.Count > 0
            ? cached
            : Parse(expression, context.Chat);

        foreach (var type in handlers.Keys)
        {
            var handler = handlers[type].FirstOrDefault(x => Fortune.LuckyFor(x.Percent));
            if (handler != default && MessageMatches(type, context.Message))
            {
                if (type is not 'u')
                    return handler.Command;

                var split = handler.Command.SplitN(2);
                var command = split[0];
                var args = split.Length > 1 ? split[1] : null;
                return $"{command} {GetURL(context.Message)} {args}"; // e.g. /cut URL 300
            }
        }

        return null;
    }

    private static Dictionary<char, List<(int Percent, string Command)>> Parse(string expression, long chat)
    {
        var handlers = new Dictionary<char, List<(int Percent, string Command)>>();

        var matches = expression.Split(";", StringSplitOptions.RemoveEmptyEntries)
            .Select(x => _rgx_handler.Match(x.Trim())).Where(x => x.Success);

        foreach (var match in matches)
        {
            var types   = match.ExtractGroup(1, s => s, "");
            var percent = match.ExtractGroup(2, s => int.Parse(s.TrimEnd('%')), 100);
            var command = match.ExtractGroup(3, s => s, "");
            foreach (var type in types)
            {
                if (handlers.ContainsKey(type).Janai())
                    handlers[type] = [];

                handlers[type].Add((percent, command));
            }
        }

        Cache.Add(chat, handlers);

        return handlers;
    }

    private static bool MessageMatches(char type, Message message) => type switch
    {
        'p' => CheckPhoto(message),
        'v' => CheckVideo(message),
        'a' => CheckAudio(message),
        'g' => CheckGIF  (message),
        'u' => CheckURL  (message),
        's' => CheckStick(message),
        _ => throw new ArgumentException(),
    };

    private static bool CheckPhoto(Message message)
    {
        return message.Photo != null
            || message.HasImageDocument();
    }

    private static bool CheckVideo(Message message)
    {
        return message.Video     != null
            || message.VideoNote != null
            || message.HasVideoDocument();
    }

    private static bool CheckAudio(Message message)
    {
        return message.Audio != null 
            || message.Voice != null 
            || message.HasAudioDocument();
    }

    private static bool CheckGIF(Message message)
    {
        return message.Animation != null
            || message.HasVideoSticker()
            || message.HasAnimeDocument();
    }

    private static bool CheckURL(Message message)
    {
        return message.GetTextOrCaption() != null 
            && message.GetURL() != null;
    }

    private static bool CheckStick(Message message)
    {
        return message.HasImageSticker();
    }

    private static string GetURL(Message message)
    {
        var text = message.GetTextOrCaption()!;
        var url  = message.GetURL()!;
        return text.Substring(url.Offset, url.Length);
    }
}