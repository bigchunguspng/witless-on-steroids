using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using PF_Bot.Backrooms.Helpers;
using PF_Bot.Features.Media.MediaDB;
using PF_Bot.Routing.Commands;
using PF_Bot.State.Chats;
using PF_Bot.Tools_Legacy.MemeMakers.Shared;
using PF_Bot.Tools_Legacy.RedditSearch;
using Telegram.Bot.Types;

namespace PF_Bot.Features.Help;

public class DebugMessage : SyncCommand
{
    private readonly Regex _jsonFileId   = new(@"""file_id"": ?""(.+?)""");
    private readonly Regex _jsonFileName = new(@"""file_name"": ?""(.+?)""");
    private readonly Regex _jsonFileSize = new(@"""file_size"": ?(\d+)");
    private readonly Regex _jsonWH       = new(@"""width"": ?(\d+),\s+?""height"": ?(\d+)");

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
            Bot.SendMessage(Origin, DEBUG_MANUAL);
            return;
        }

        var json = JsonSerializer.Serialize(message, JsonOptions);
        var id   = _jsonFileId  .Matches(json).LastOrDefault();
        var name = _jsonFileName.Matches(json).LastOrDefault();
        var size = _jsonFileSize.Matches(json).LastOrDefault();
        var wh   = _jsonWH      .Matches(json);
        if (Command!.Contains('!') && id is { Success: true } && size is { Success: true })
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

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
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
        var loaded = ChatManager.LoadedBakas.Count;
        var total  = ChatManager.SettingsDB .Count;
        var packs = string.Join('\n', ChatManager.LoadedBakas.Select(x => x.Key.ToString()));
        return $"📝 <u>PACKS LOADED</u>: {loaded}/{total}\n<blockquote expandable>{packs}</blockquote>";
    }

    private static string GetRedditInfo()
    {
        if (ConsoleUI.LoggedIntoReddit == false) return "😎 Not initialized 👌";

        var p = RedditTool.Instance.PostsCached;
        var q = RedditTool.Instance.QueriesCached;
        var c = RedditTool.Instance.DebugCache();
        return $"🛒 <u>REDDIT CACHE</u>: {p} POSTS, {q} QUERIES\n<blockquote expandable>{c}</blockquote>";
    }

    private string GetEmojiInfo()
    {
        return $"🏀 <u>EMOJI CACHE</u>: {EmojiTool.EmojisCached} шт.";
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