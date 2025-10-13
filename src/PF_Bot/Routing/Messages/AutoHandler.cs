using PF_Bot.Features_Aux.Settings.Core;
using Telegram.Bot.Types;
using HandlerChance = (int Percent, string Handler); // handler = pipe

namespace PF_Bot.Routing.Messages;

// auto = [expr 0][;] [expr N]
// expr = [types][N%]:[pipe]
// pipe = [section 0][ > ][section N]
// sect = [command][options] [args]

public class AutoHandlerMap : Dictionary<char, List<HandlerChance>>;

public static class AutoHandler
{
    private static readonly Regex
        _rgx_handler = new(@"([pvagus]+)(?:(\d{1,3})%)?:\s*(.+)", RegexOptions.Compiled);

    // todo investigate: cache has duplicate entries ?
    private static readonly LimitedCache<long, AutoHandlerMap> Cache = new(32);

    public static void ClearCache(long chat)
    {
        if (Cache.Contains(chat, out var map)) map.Clear();
    }

    // EXPRESSION PARSING

    public static LinkedList<string>? TryGetMessageHandler(MessageContext context, ChatSettings data)
    {
        var expression = data.Options![MemeType.Auto];
        if (expression is null) return null;

        var handlers = Cache.Contains(context.Chat, out var map) && map.Count > 0
            ? map
            : Parse_AndCache(expression, context.Chat);

        foreach (var type in handlers.Keys)
        {
            var handler = handlers[type].FirstOrDefault(x => Fortune.LuckyFor(x.Percent));
            if (handler != default && MessageMatches(type, context.Message))
            {
                var sections = handler.Handler.Split(">").Select(x => x.Trim());
                var pipe = new LinkedList<string>(sections);

                if (type == 'u' && pipe.First is { } first)
                {
                    var split = handler.Handler.SplitN(2);
                    var command = split[0];
                    var args = split.Length > 1 ? split[1] : null;

                    first.Value = $"{command} {GetURL(context)} {args}"; // e.g. /cut URL 300
                }

                return pipe;
            }
        }

        return null;
    }

    private static AutoHandlerMap Parse_AndCache(string definition, long chat)
    {
        var map = new AutoHandlerMap();

        var expressions = definition
            .Split(";", StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim());
        var matches = expressions
            .Select(x => _rgx_handler.Match(x))
            .Where(x => x.Success);

        foreach (var match in matches)
        {
            var types   = match.ExtractGroup(1, s => s, "");
            var percent = match.ExtractGroup(2, int.Parse, 100);
            var handler = match.ExtractGroup(3, s => s, "");
            foreach (var type in types)
            {
                if (map.ContainsKey(type).Janai())
                    map[type] = [];

                map[type].Add((percent, handler));
            }
        }

        Cache.Add(chat, map);

        return map;
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

    private static string GetURL(MessageContext ctx)
    {
        var url = ctx.Message.GetURL()!;
        return ctx.Text!.Substring(url.Offset, url.Length);
    }
}