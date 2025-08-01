using System.Runtime.CompilerServices;
using Telegram.Bot.Types;
using Media = (string Id, string FileId, string Text, string LowercaseText);

namespace Witlesss.Services.Sounds;

public abstract class MediaDB<T> where T : FileBase
{
    private readonly List<Media> _files = [];

    protected abstract string Name { get; }
    protected abstract string What { get; }
    protected abstract string WhatSingle { get; }
    protected abstract string DB_Path { get; }

    protected R LoadDB<R>() where R : MediaDB<T>
    {
        var sw = GetStartedStopwatch();

        if (File.Exists(DB_Path) == false)
        {
            using var _ = File.Create(DB_Path);
        }

        using var reader = File.OpenText(DB_Path);
        while (true)
        {
            var line = reader.ReadLine();
            if (line is null) break;

            if (line.Length == 0 || line.StartsWith('#')) continue;

            var args = line.Split(' ', 3);
            _files.Add((args[0], args[1], args[2], args[2].ToLower()));
        }

        Log($"[{Name}] >> LOADED ({sw.Elapsed.ReadableTimeShort()})", color: LogColor.Yellow);

        return (R)this;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void SaveData(List<Media> buffer)
    {
        if (buffer.Count == 0) return;

        var skip = _files.Count;
        _files.AddRange(buffer);
        buffer.Clear();

        using var writer = File.AppendText(DB_Path);
        foreach (var file in _files.Skip(skip))
        {
            writer.WriteLine($"{file.Id} {file.FileId} {file.Text}");
        }
        Log($"[{Name}] >> ADDED {_files.Count - skip} {What}", color: LogColor.Lime);
    }

    // SEARCH

    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Media> Search(string? query)
    {
        var filtered = string.IsNullOrWhiteSpace(query)
            ? GetRandomFiles()
            : GetFilesByQuery(query);
        return filtered.Take(50);
    }

    private IEnumerable<Media> GetRandomFiles()
    {
        var pickChance = Math.Max(1, 5000 / _files.Count);
        return _files.Where(_ => LuckyFor(pickChance));
    }

    private IEnumerable<Media> GetFilesByQuery(string query)
    {
        var words = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var except = words.Where(x => x.Length > 1 && x.StartsWith('!')).ToArray();
        if (except.Length == 0)
        {
            return _files.Where(x => words.All(w => x.LowercaseText.Contains(w)));
        }
        else
        {
            var includeWords = words.Except(except).ToArray();
            var excludeWords = except.Select(x => x.Substring(1)).ToArray();
            return _files.Where(x =>
            {
                return includeWords.All(w => x.LowercaseText.Contains(w))
                    && excludeWords.Any(w => x.LowercaseText.Contains(w)) == false;
            });
        }
    }

    // UPLOAD

    public async Task UploadSingle(string fileId, string fileName, MessageOrigin origin)
    {
        var path = Path.Combine(Dir_Temp, $"{WhatSingle}-{DateTime.Now.Ticks}");
        var file = Path.Combine(path, fileName);
        Directory.CreateDirectory(path);
        await Bot.Instance.DownloadFile(fileId, file, origin);
        await UploadMany(path);
    }

    public async Task UploadMany(string directory)
    {
        var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
        int count = 0, total = files.Length;
        var buffer = new List<Media>(total);
        Directory.CreateDirectory(Dir_Temp);
        foreach (var file in files)
        {
            var name = Path.GetFileName(file);
            var text = Path.GetFileNameWithoutExtension(name);
            try
            {
                var tgFile = await UploadFile(file, Config.SoundChannel);
                buffer.Add((tgFile.FileUniqueId, tgFile.FileId, text, text.ToLower()));
                Log($"[{Name}] << {++count, 3} / {total} {tgFile.FileUniqueId}", color: LogColor.Yellow);

                if (count % 10 == 0) SaveData(buffer);
            }
            catch (Exception e)
            {
                total--;
                LogError($"[{Name}] >> Can't have [{name}] in Detroit X_X --> {e.Message}");
            }
        }

        SaveData(buffer);
    }

    /// <summary>
    /// Uploads file to Telegram server and returns its ID.
    /// </summary>
    protected abstract Task<T> UploadFile(string path, long channel);
}

public static class MediaExtensions
{
    private static readonly Regex _tags = new(@"#\S+\s");

    public static string GetTitle(this Media media)
    {
        var text = media.Text;
        return text.StartsWith('#') ? _tags.Replace(text, string.Empty) : text;
    }
}