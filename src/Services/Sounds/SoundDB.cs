using System.Runtime.CompilerServices;
using Telegram.Bot;
using Sound = (string FileId, string Text);

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

            var args = line.Split(' ', 2);
            _sounds.Add((args[0], args[1]));
        }

        Log($"[SoundDB] >> LOADED ({sw.Elapsed.ReadableTimeShort()})", color: 11);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void SaveData(List<Sound> newSounds)
    {
        if (newSounds.Count == 0) return;

        var skip = _sounds.Count;
        _sounds.AddRange(newSounds);
        newSounds.Clear();

        using var writer = File.AppendText(File_Sounds);
        foreach (var sound in _sounds.Skip(skip))
        {
            writer.WriteLine($"{sound.FileId} {sound.Text}");
        }
        Log($"[SoundDB] >> ADDED {_sounds.Count - skip} SOUNDS", color: 10);
    }

    // SEARCH

    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Sound> Search(string query)
    {
        return _sounds.Where(x => x.Text.ToLower().Contains(query.ToLower())).Take(50);
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
                var fileId = await UploadFile(file, Config.SoundChannel);
                buffer.Add((fileId, text));
                Log($"[SoundDB] << {++count, 3} / {total} {ShortID(fileId)}", color: 11);

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
    private async Task<string> UploadFile(string path, long channel)
    {
        var temp = Path.Combine(Dir_Temp, $"{Guid.NewGuid()}.ogg");
        var opus = await path.UseFFMpeg((0, 0)).ToVoice().OutAs(temp);

        await using var stream = File.OpenRead(opus);
        var message = await Bot.Instance.Client.SendVoice(channel, stream);
        return message.Voice!.FileId;
    }
}