using Newtonsoft.Json;
using PF_Bot.Core.Internet.Boards;
using Telegram.Bot.Extensions;

namespace PF_Bot.Backrooms.Helpers;

public static class BoardHelpers
{
    private static readonly Regex
        _rgx_URL = new(@"(?:\S+(?::[\/\\])\S+)|(?:<.+\/.*>)", RegexOptions.Compiled);

    public static void Print4chan() => PrintMenu(new BoardService().GetBoardList(File_4chanHtmlPage));
    public static void Print2chan() => PrintMenu(new PlankService().GetBoardList(File_2chanHtmlPage));

    public static void PrintMenu(IEnumerable<BoardGroup> menu)
    {
        foreach (var group in menu)
        {
            Print($"{group.Title}{(group.IsNSFW ? " (18+)" : "")}");
            foreach (var board in group.Boards)
            {
                Print($"\t{board.Title,-25}{board.URL,-30}{(board.IsNSFW ? "18+" : "")}");
            }
        }
    }

    /// Checks if the filename describes a <b>single thread</b> discussion.
    /// <param name="name">filename part without date and time</param>
    public static bool FileNameIsThread(string name) => name.Contains('.') && name.Contains(".zip").Janai();

    /// Returns a page view of JSON files of previously eaten threads.
    public static IEnumerable<string> GetJsonList(FileInfo[] files, int page = 0, int perPage = 10)
    {
        if (files.Length == 0)
        {
            yield return "*пусто*";
            yield break;
        }

        foreach (var file in files.Skip(page * perPage).Take(perPage))
        {
            var name = file.Name.Replace(".json", "");
            var size = file.Length.ReadableFileSize();
            yield return $"<code>{name}</code> | {size}";

            if (FileNameIsThread(name.Split(' ')[^1]))
            {
                yield return $"<blockquote expandable>{GetThreadPreview(file.FullName)}</blockquote>";
            }
        }
    }

    private static string GetThreadPreview(string path)
    {
        var serializer = ThreadSubjectDeserializer;
        using var stream = File.OpenText(path);
        using var reader = new JsonTextReader(stream);

        var post = serializer.Deserialize<List<string>>(reader)!.First();

        post = HtmlText.Escape(post);
        post = _rgx_URL.Replace(post, match => $"<a href=\"{match.Value}\">[deleted]</a>");

        if (post.Contains(": "))
        {
            var s = post.Split(": ", 2);
            post = $"<b>{s[0]}</b>: {s[1]}";
        }

        return post;
    }

    private static readonly JsonSerializer ThreadSubjectDeserializer = new()
    {
        Converters = { new FirstRowReader() }, DefaultValueHandling = DefaultValueHandling.Ignore
    };

    /// <summary> <b>READ-ONLY!</b> Returns only <b>THE FIRST</b> string from the list. </summary>
    private class FirstRowReader : JsonConverter<List<string>>
    {
        public override void WriteJson
            (JsonWriter writer, List<string>? value, JsonSerializer serializer)
            => throw new NotImplementedException();

        public override List<string> ReadJson
            (JsonReader reader, Type type, List<string>? list, bool hasValue, JsonSerializer serializer)
        {
            do     reader.Read();
            while (reader.TokenType != JsonToken.String);

            return [(string)reader.Value!];
        }
    }
}