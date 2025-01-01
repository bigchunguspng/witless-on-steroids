using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Witlesss.Backrooms;

public static partial class Extensions
{
    [StringSyntax("Regex")] private const string EMOJI_REGEX
        = @"((\u00a9|\u00ae|\u203c|\u2049|\u2122|[\u2139-\u21aa]|\u3297|\u3299)\ufe0f|([\u231a-\u303d]|(\ud83c|\ud83d|\ud83e)[\ud000-\udfff])\ufe0f*\u200d*|[\d*#]\ufe0f\u20e3)+";

    public static readonly Regex EmojiRegex = new(EMOJI_REGEX);
    public static readonly Regex FFmpeg = new("ffmpeg|ffprobe", RegexOptions.IgnoreCase);
    public static readonly Regex URL_Regex = new(@"(?:\S+(?::[\/\\])\S+)|(?:<.+\/.*>)", RegexOptions.Compiled);

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

    public static T PickAny<T>(this ICollection<T> collection)
    {
        return collection.ElementAt(Random.Shared.Next(collection.Count));
    }

    // FORMAT

    public static string Format(this double value) => value.ToString(CultureInfo.InvariantCulture);
    public static string Format(this float  value) => value.ToString(CultureInfo.InvariantCulture);

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

    public static string ValidFileName(this string text, char x = '_')
    {
        var chars = Path.GetInvalidFileNameChars();
        return chars.Aggregate(text, (current, c) => current.Replace(c, x));
    }

    public static bool FileNameIsInvalid(this string text)
    {
        return Path.GetInvalidFileNameChars().Any(text.Contains);
    }

    // COMFIES

    public static F_Process UseFFMpeg(this string path, MessageOrigin origin) => new(path, origin);

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var element in source) action(element);
    }

    public static T GetRandomMemeber<T>() where T : Enum
    {
        var values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(Random.Shared.Next(values.Length))!;
    }

    // MEDIA

    public static string ShortID(string id) => id.Remove(56).Remove(0, 23);

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
        return Errors.ExtractGroup(1, message, s => s, message)!;
    }

    // FILE

    public static string ReadableFileSize(this long bytes)
    {
        var kbs = bytes / 1024F;
        var mbs = kbs   / 1024F;
        return mbs >= 100
            ? $"{mbs:F1} МБ"
            : mbs >= 1
                ? $"{mbs:F2} МБ"
                : kbs >= 1
                    ? $"{kbs:F0} КБ"
                    : $"{bytes} байт";
    }

    public static string ReadableFileSize
        (this string path) => path.FileSizeInBytes().ReadableFileSize();

    public static long FileSizeInBytes
        (this string path) => File.Exists(path) ? new FileInfo(path).Length : 0;

    public static bool FileIsEmptyOrNotExist
        (this string path) => !File.Exists(path) || path.FileSizeInBytes() == 0;

    public static bool IsNestedPath
        (this string path) => path.Contains(Path.PathSeparator);

    public static void CreateFilePath
        (this string path) => Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "");

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