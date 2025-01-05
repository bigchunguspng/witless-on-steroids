using System.Runtime.CompilerServices;
using Telegram.Bot;
using Telegram.Bot.Types;
using Sound = (string Id, string FileId, string Text);

namespace Witlesss.Services.Sounds;

public class SoundDB
{
    public static readonly SoundDB Instance = new();

    // DATA

    private readonly List<Sound> _sounds;

    private SoundDB()
    {
        var sw = GetStartedStopwatch();

        _sounds = [];

        if (File.Exists(File_Sounds) == false) return;
        using var reader = File.OpenText(File_Sounds);
        while (true)
        {
            var line = reader.ReadLine();
            if (line is null) break;

            if (line.Length == 0 || line.StartsWith('#')) continue;

            var args = line.Split(' ', 3);
            _sounds.Add((args[0], args[1], args[2]));
        }

        Log($"[SoundDB] >> LOADED ({sw.Elapsed.ReadableTimeShort()})", color: 11);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void SaveData(List<Sound> buffer)
    {
        if (buffer.Count == 0) return;

        var skip = _sounds.Count;
        _sounds.AddRange(buffer);
        buffer.Clear();

        using var writer = File.AppendText(File_Sounds);
        foreach (var sound in _sounds.Skip(skip))
        {
            writer.WriteLine($"{sound.Id} {sound.FileId} {sound.Text}");
        }
        Log($"[SoundDB] >> ADDED {_sounds.Count - skip} SOUNDS", color: 10);
    }

    // SEARCH

    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Sound> Search(string query)
    {
        var words = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var pickChance = Math.Max(1, 5000 / _sounds.Count);
        var filtered = string.IsNullOrWhiteSpace(query)
            ? _sounds.Where(_ => LuckyFor(pickChance))
            : _sounds.Where(x => words.All(w => x.Text.ToLower().Contains(w)));
        return filtered.Take(50);
    }

    // UPLOAD

    public async Task UploadSingle(string fileId, string fileName, MessageOrigin origin)
    {
        var path = Path.Combine(Dir_Temp, $"sound-{DateTime.Now.Ticks}");
        var file = Path.Combine(path, fileName);
        Directory.CreateDirectory(path);
        await Bot.Instance.DownloadFile(fileId, file, origin);
        await UploadMany(path);
    }

    public async Task UploadMany(string directory)
    {
        var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
        int count = 0, total = files.Length;
        var buffer = new List<Sound>(total);
        Directory.CreateDirectory(Dir_Temp);
        foreach (var file in files)
        {
            var name = Path.GetFileName(file);
            var text = Path.GetFileNameWithoutExtension(name);
            try
            {
                var voice = await UploadFile(file, Config.SoundChannel);
                buffer.Add((voice.FileUniqueId, voice.FileId, text));
                Log($"[SoundDB] << {++count, 3} / {total} {voice.FileUniqueId}", color: 11);

                if (count % 10 == 0) SaveData(buffer);
            }
            catch (Exception e)
            {
                total--;
                LogError($"[SoundDB] >> Can't have [{name}] in Detroit X_X --> {e.Message}");
            }
        }

        SaveData(buffer);
    }

    /// <summary>
    /// Uploads file to Telegram server and returns its ID.
    /// </summary>
    private async Task<Voice> UploadFile(string path, long channel)
    {
        var temp = Path.Combine(Dir_Temp, $"{Guid.NewGuid()}.ogg");
        var opus = await path.UseFFMpeg((0, null)).ToVoice().OutAs(temp);

        await using var stream = File.OpenRead(opus);
        var message = await Bot.Instance.Client.SendVoice(channel, stream);
        return message.Voice!;
    }
}

public static class SoundExtensions
{
    private static readonly Regex _tags = new(@"#\S+\s");

    public static string GetTitle(this Sound sound)
    {
        var text = sound.Text;
        return text.StartsWith('#') ? _tags.Replace(text, string.Empty) : text;
    }
}