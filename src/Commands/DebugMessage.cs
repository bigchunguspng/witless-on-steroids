using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Telegram.Bot.Types;
using Witlesss.Memes.Shared;
using Witlesss.Services.Internet.Reddit;

namespace Witlesss.Commands;

public class DebugMessage : SyncCommand
{
    private readonly Regex _jsonFileId   = new(@"""file_id"": ?""(.+?)""");
    private readonly Regex _jsonFileSize = new(@"""file_size"": ?(\d+)");

    protected override void Run()
    {
        var admin = Message.SenderIsBotAdmin();
        if (admin && Args != null)
        {
            var response = Args.SplitN(2)[0][0] switch
            {
                'm' => GetResourceUsage(),
                'p' => GetPacksInfo(),
                'r' => GetRedditInfo(),
                'e' => GetEmojiInfo(),
                _ => DEBUG_ADMIN_MANUAL,
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

        var json = JsonSerializer.Serialize(message, _options);
        var id   = _jsonFileId  .Matches(json).LastOrDefault();
        var size = _jsonFileSize.Matches(json).LastOrDefault();
        if (Command!.Contains('!') && id is { Success: true } && size is { Success: true })
        {
            var fileId = id.Groups[1].Value;
            var fileSize = long.Parse(size.Groups[1].Value);
            var text =
                $"""
                 {GetFileSizeEmoji(fileSize)} {fileSize.ReadableFileSize()}
                 <code>{fileId}</code>
                 """;
            Bot.SendMessage(Origin, text);
            Log($"{Title} >> DEBUG [!]");
            return;
        }

        var name = $"Message-{message.Id}-{message.Chat.Id}.json";
        var path = Path.Combine(Dir_Temp, name);

        Directory.CreateDirectory(Dir_Temp);
        File.WriteAllText(path, json);
        using var stream = File.OpenRead(path);

        Bot.SendDocument(Origin, InputFile.FromStream(stream, name.Replace("--", "-")));
        Log($"{Title} >> DEBUG");
    }

    private readonly JsonSerializerOptions _options = new()
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


    // ADMIN

    private static string GetResourceUsage()
    {
        using var process = Process.GetCurrentProcess();
        var memory = process.PrivateMemorySize64.ReadableFileSize();
        return $"🐏 <u>RAM USAGE</u>: {memory}";
    }

    private static string GetPacksInfo()
    {
        var loaded = ChatService.LoadedBakas.Count;
        var total  = ChatService.SettingsDB .Count;
        var packs = string.Join('\n', ChatService.LoadedBakas.Select(x => x.Key.ToString()));
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
}