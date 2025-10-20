using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PF_Bot.Core;
using PF_Bot.Features_Aux.Packs;
using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Features_Main.Media.Core;
using PF_Bot.Routing.Commands;
using PF_Tools.Graphics;
using Telegram.Bot.Types;

namespace PF_Bot.Commands.Debug;

public class DebugMessage : CommandHandlerBlocking
{
    private readonly Regex
        _rgx_jsonFileId   = new(@"""file_id"": ?""(.+?)""",                  RegexOptions.Compiled),
        _rgx_jsonFileName = new(@"""file_name"": ?""(.+?)""",                RegexOptions.Compiled),
        _rgx_jsonFileSize = new(@"""file_size"": ?(\d+)",                    RegexOptions.Compiled),
        _rgx_jsonWH       = new(@"""width"": ?(\d+),\s+?""height"": ?(\d+)", RegexOptions.Compiled);

    protected override void Run()
    {
        if (Args != null)
        {
            var response = Args.SplitN(2)[0][0] switch
            {
                'm' => GetResourceUsage(),
                'p' => GetPacksInfo(),
                'r' => GetRedditInfo(),
                'e' => GetEmojiInfo(),
                'g' => GetGIFs_TagsInfo(),
                'a' => GetAudioTagsInfo(),
                _ => DEBUG_EX_MANUAL,
            };
            Bot.SendMessage(Origin, response);
            return;
        }

        var message = Message.ReplyToMessage;
        if (message == null)
        {
            SendManual(DEBUG_MANUAL);
            return;
        }

        var json = JsonSerializer.Serialize(message, JsonOptions);
        var id   = _rgx_jsonFileId  .Matches(json).LastOrDefault();
        var name = _rgx_jsonFileName.Matches(json).LastOrDefault();
        var size = _rgx_jsonFileSize.Matches(json).LastOrDefault();
        var wh   = _rgx_jsonWH      .Matches(json);
        if (Options.Contains('!') && id is { Success: true } && size is { Success: true })
        {
            var fileId = id.Groups[1].Value;
            var fileSize = long.Parse(size.Groups[1].Value);
            var fileName = name?.Groups[1].Value;
            var resolutions = FormatResolutions(wh);

            var sb = new StringBuilder(GetFileSizeEmoji(fileSize)).Append(' ').Append(fileSize.ReadableFileSize());
            if (resolutions.Length > 0)
                sb.Append("\n🎬 ").Append(resolutions);
            if (fileName != null)
                sb.Append("\n ✍️ <i>").Append(fileName).Append("</i>");
            sb.Append("\n<code>").Append(fileId).Append("</code>");

            Bot.SendMessage(Origin, sb.ToString());
            Log($"{Title} >> DEBUG [!]");
        }
        else
        {
            var fileName = $"Message-{message.Id}-{message.Chat.Id}.json";
            var path = Path.Combine(Dir_Temp, fileName);

            Directory.CreateDirectory(Dir_Temp);
            File.WriteAllText(path, json);

            using var stream = File.OpenRead(path);
            Bot.SendDocument(Origin, InputFile.FromStream(stream, fileName.Replace("--", "-")));
            Log($"{Title} >> DEBUG");
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = NewtonsoftJsonCompatibleEncoder.Encoder,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true,
    };

    private static string GetFileSizeEmoji(long size) => size switch
    {
        > 1024 * 1024 * 20 => "😵",
        > 1024 * 1024 * 5  => "😤",
        > 1024 * 1024 * 1  => "👌",
        _                  => "👍",
    };

    private static string FormatResolutions(MatchCollection matches)
    {
        var resolutions = matches.Where(x => x.Success).Select(x => $"{x.Groups[1].Value}x{x.Groups[2].Value}").Distinct();
        return string.Join(", ", resolutions);
    }


    // EXTENDED

    private static string GetResourceUsage()
    {
        using var process = Process.GetCurrentProcess();
        var memory = process.PrivateMemorySize64.ReadableFileSize();
        return $"🐏 <u>RAM USAGE</u>: {memory}";
    }

    private static string GetPacksInfo()
    {
        var chats  = ChatManager.Chats.Count;
        var loaded = PackManager.Bakas.Count;
        var total  = PackManager.PacksTotal;
        var packs = string.Join('\n', PackManager.Bakas.Select(x => x.Key.ToString()));
        return $"""
                💬 <u>CHATS KNOWN</u>: {chats}
                📝 <u>PACKS LOADED</u>: {loaded}/{total}
                <blockquote expandable>{packs}</blockquote>
                """;
    }

    private static string GetRedditInfo()
    {
        if (App.LoggedIntoReddit.Janai()) return "😎 Not initialized 👌";

        var posts   = App.Reddit.PostsCached;
        var queries = App.Reddit.QueriesCached;
        var cache   = App.Reddit.DebugCache();

        var list = string.Join('\n', cache.Select(x => $"{x.Count} - <code>{x.Query}</code>"));
        return $"🛒 <u>REDDIT CACHE</u>: {posts} POSTS, {queries} QUERIES\n<blockquote expandable>{list}</blockquote>";
    }

    private string GetEmojiInfo()
    {
        return $"🏀 <u>EMOJI CACHE</u>: {EmojiTool.EmojisCache_Count} шт.";
    }

    private string GetGIFs_TagsInfo()
    {
        var tags = GIF_DB.Instance.GetTopTags(300);
        var join = string.Join(", ", tags.Select(x => $"{x.Count}×{x.Tag}"));
        return $"📹 <u>TOP {tags.Count} TAGS (GIFs)</u>\n{join}";
    }

    private string GetAudioTagsInfo()
    {
        var tags = SoundDB.Instance.GetTopTags(300);
        var join = string.Join(", ", tags.Select(x => $"{x.Count}×{x.Tag}"));
        return $"🎙 <u>TOP {tags.Count} TAGS (Sounds)</u>\n{join}";
    }
}