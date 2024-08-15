using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Witlesss.Backrooms;

public static partial class Extensions
{
    [StringSyntax("Regex")] private const string EMOJI_REGEX
        = @"((\u00a9|\u00ae|\u203c|\u2049|\u2122|[\u2139-\u21aa]|\u3297|\u3299)\ufe0f|([\u231a-\u303d]|(\ud83c|\ud83d|\ud83e)[\ud000-\udfff])\ufe0f*\u200d*|[\d*#]\ufe0f\u20e3)+";

    public static readonly Regex EmojiRegex = new(EMOJI_REGEX);
    public static readonly Regex FFmpeg = new("ffmpeg|ffprobe", RegexOptions.IgnoreCase);

    // RANDOM

    public static bool IsOneIn(int x)
        => Random.Shared.Next(x) == 0;

    public static bool IsFirstOf(int a, int b)
        => Random.Shared.Next(a + b) < a;

    public static bool LuckyFor(int chance, int max = 100)
        => Random.Shared.Next(max) < chance;

    public static int RandomInt(int min, int max)
        => Random.Shared.Next(min, max + 1);

    public static double RandomDouble(double min, double max)
    {
        var k = 10_000d;
        return RandomInt((int)(min * k), (int)(max * k)) / k;
    }

    public static T PickRandom<T>(this ICollection<T> collection)
    {
        return collection.ElementAt(Random.Shared.Next(collection.Count));
    }

    // FORMAT

    public static string Format
        (this double value) => value.ToString(CultureInfo.InvariantCulture);

    // PATH

    public static string UniquePath(string path, bool extraCondition = false)
    {
        return UniquePath(Path.GetDirectoryName(path), Path.GetFileName(path), extraCondition);
    }

    public static string UniquePath(string? directory, string file, bool extraCondition = false)
    {
        // dir/file.txt
        // dir/file_A8.txt
        // dir/file_A8_62.txt

        var path = directory is null ? file : Path.Combine(directory, file);

        if (directory is not null) Directory.CreateDirectory(directory);

        if (!File.Exists(path) && extraCondition is false) return path;

        do
        {
            var index = path.LastIndexOf('.');
            var part1 = path.Remove(index); // directory/name
            var part2 = path.Substring(index); // .txt

            var xx = Random.Shared.Next(256).ToString("X2");
            path = $"{part1}_{xx}{part2}";
            if (!File.Exists(path)) return path;
        }
        while (true);
    }

    public static string ValidFileName(string text, char x = '_')
    {
        var chars = Path.GetInvalidFileNameChars();
        return chars.Aggregate(text, (current, c) => current.Replace(c, x));
    }

    // COMFIES

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var element in source) action(element);
    }

    public static T GetRandomMemeber<T>() where T : Enum
    {
        var values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(Random.Shared.Next(values.Length))!;
    }

    // MEDIA | todo find better way

    public static string ShortID           (string id) => id.Remove(62).Remove(2, 44);
    public static string ExtensionFromID   (string id) => ExtensionsIDs[id.Remove(2)];
    public static MediaType MediaTypeFromID(string id) => MediaTypes   [id.Remove(2)];

    private static readonly Dictionary<string, string> ExtensionsIDs = new()
    {
        { "BA", ".mp4" }, { "Cg", ".mp4" }, // Cg - animation
        { "CQ", ".mp3" }, { "Aw", ".mp3" }, // Aw - voice message / ogg
        { "BQ", ".wav" }, { "DQ", ".mp4" }, // DQ - video message
        { "Ag", ".jpg" }, { "CA", ".webm" } // CA - stickers
    };

    private static readonly Dictionary<string, MediaType> MediaTypes = new()
    {
        { "BA", MediaType.Movie }, { "DQ", MediaType.Round }, { "Cg", MediaType.Video }, { "CA", MediaType.Video },
        { "Aw", MediaType.Audio }, { "BQ", MediaType.Audio }, { "CQ", MediaType.Audio }, { "Ag", MediaType.Video }
    };

    //

    public static string HOURS_ED(int hours) => ED(hours, "", "а", "ов");
    public static string  MINS_ED(int mins ) => ED(mins,  "у", "ы", "");

    private static string ED(int x, string one, string twoFour, string any)
    {
        if (x % 10 > 4 || x % 10 == 0 || x is > 10 and < 15) return any;
        else if (x % 10 > 1) return twoFour;
        else return one;
    }

    //

    private static readonly Regex Errors = new(@"One or more errors occurred. \((\S*(\s*\S)*)\)");

    public static string GetFixedMessage(this Exception e)
    {
        var message = e.Message;
        var match = Errors.Match(message);
        return match.Success ? match.Groups[1].Value : message;
    }

    // FILE

    public static string FileSize(string path) => FileSize(SizeInBytes(path));

    public static string FileSize(long bytes)
    {
        long kbs = bytes / 1024;
        return kbs switch { < 1 => bytes + " байт", _ => kbs + " КБ" };
    }
    
    public static long SizeInBytes(string path) => File.Exists(path) ? new FileInfo(path).Length : 0;
    public static bool FileEmptyOrNotExist(string path) => !File.Exists(path) || SizeInBytes(path) == 0;
    public static void CreateFilePath(string path) => Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "");

    public static FileInfo[] GetFilesInfo(string path, bool recursive = false)
    {
        Directory.CreateDirectory(path);
        return new DirectoryInfo(path).GetFiles("*", recursive.ToSearchOption());
    }

    public static string[] GetFiles(string path, string pattern = "*", bool recursive = false)
    {
        Directory.CreateDirectory(path);
        return Directory.GetFiles(path, pattern, recursive.ToSearchOption());
    }

    private static SearchOption ToSearchOption
        (this bool recursive) => recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
}