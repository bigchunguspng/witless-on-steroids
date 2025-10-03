using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using PF_Bot.Features_Main.Text.Core;
using PF_Tools.Copypaster;
using PF_Tools.Copypaster.Helpers;
using PackProps = (long packSize, int wordCount);

namespace PF_Bot.Features_Aux.Packs.Core;

// Rules:
// 1. Only non-empty packs should be saved to disk.
// todo before commit: hang sync attr where needed.

public record FuseReport(string NewSize, string DeltaSize, string DeltaCount);
public record FeedReport(FuseReport Report, int Consumed);

public static class PackManager
{
    private const byte MAX_IDLE_BEFORE_UNLOAD = 10;

    /// Packs loaded into memory for quick access.
    public static readonly SyncDictionary<long, Copypaster> Bakas = new();

    // PATHS

    public static FilePath GetPackPath
        (long chat) => Path.Combine(Dir_Chat, $"{chat}{Ext_Pack}");

    public static FilePath GetPacksFolder
        (long chat, bool isPrivate) => isPrivate 
        ? GetPrivatePacksFolder(chat) 
        : Dir_Fuse;
    public static FilePath GetFilesFolder
        (long chat, bool isPrivate) => isPrivate 
        ? GetPrivateFilesFolder(chat) 
        : Dir_History;

    public static FilePath GetPrivatePacksFolder
        (long chat) => Dir_Fuse   .Combine(chat.ToString());
    public static FilePath GetPrivateFilesFolder
        (long chat) => Dir_History.Combine(chat.ToString());

    // GET

    public static bool BakaIsLoaded
        (long chat)
        => Bakas.ContainsKey(chat);

    public static bool BakaIsLoaded
        (long chat, [NotNullWhen(true)] out Copypaster? baka)
        => Bakas.TryGetValue(chat, out baka);

    /// <b>HIGH MEMORY USAGE!</b> Loads <see cref="GenerationPack"/> into memory if it's not loaded yet.
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static Copypaster GetBaka
        (long chat)
        => BakaIsLoaded(chat, out var baka)
            ? baka
            : Load(chat);

    // AUTOSAVE (and AUTODROP)

    public static void StartAutoSaveThread(TimeSpan interval)
    {
        var thread = new Thread(() => AutoSaveLoop(interval))
        {
            Name = "AutoSave",
            IsBackground = true,
        };

        thread.Start();
    }

    private static void AutoSaveLoop(TimeSpan interval)
    {
        while (true)
        {
            Thread.Sleep(interval);
            try
            {
                Bakas_SaveDirty_DropIdle();
            }
            catch (Exception e)
            {
                LogError($"AUTOSAVE FAIL | {e.GetErrorMessage()}");
            }
        }
    }

    public static void Bakas_SaveDirty_DropIdle()
    {
        Bakas_SaveDirty();
        Bakas_DropIdle();
    }

    public static void Bakas_SaveDirty
        () => Bakas.ForEachPair(Save);

    private static void Bakas_DropIdle
        () => Bakas.ForEachPair((chat, baka) =>
    {
        if (baka.Idle >= MAX_IDLE_BEFORE_UNLOAD) Drop(chat);
        else
            baka.BumpIdle();
    });

    // LOAD / SAVE / DROP

    private static Copypaster Load(long chat)
    {
        try
        {
            Copypaster baka = null!;

            var log = PackIO_MeasureSpeed(chat, path =>
            {
                baka = new Copypaster(GenerationPackIO.Load(path));
                Bakas.Add(chat, baka);
            });

            Log($"DIC LOAD | {chat,14} | {log}", LogLevel.Info, LogColor.Fuchsia);

            return baka;
        }
        catch
        {
            LogError($"DIC LOAD | {chat,14} | FAILED");
            throw;
        }
    }

    /// Saves <see cref="baka"/> if it's dirty.
    public static void Save(long chat, Copypaster baka) // todo should we pass baka?
    {
        if (baka.IsDirty.Janai()) return;

        var log = PackIO_MeasureSpeed(chat, path =>
        {
            lock (baka)
            {
                Dir_Chat.EnsureDirectoryExist();
                GenerationPackIO.Save_WithTemp(baka.Pack, path);
                baka.ResetDirty();
            }
        });

        Log($"DIC SAVE | {chat,14} | {log}", LogLevel.Info, LogColor.Lime);
    }

    private static void Drop(long chat)
    {
        Bakas.Remove(chat);
        Log($"DIC DROP | {chat,14}", LogLevel.Info, LogColor.Yellow);
    }

    private static string
        PackIO_MeasureSpeed
        (long chat, Action<FilePath> packIO_action)
    {
        var sw = Stopwatch.StartNew();
        var path = GetPackPath(chat);

        packIO_action(path);

        var elapsed = sw.Elapsed;
        var bytes = path.FileSizeInBytes;
        var bytes_ps = bytes / elapsed.TotalSeconds;
        var size  = bytes   .ReadableFileSize();
        var speed = bytes_ps.ReadableFileSpeed();

        return $"{elapsed.ReadableTime(),10} | {size,9} | {speed,10}";
    }

    // FUSE / FEED

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static FuseReport Fuse
        (long chat, Copypaster baka, GenerationPack pack)
    {
        var path = GetPackPath(chat);

        var a = MeasureDick(chat, baka, path);
        var sw = Stopwatch.StartNew();
        baka.Fuse(pack);
        sw.Log("/fuse (pack)");
        var b = MeasureDick(chat, baka, path);

        return GetReport(a, b);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static FeedReport Feed
        (long chat, Copypaster baka, IEnumerable<string> texts, int wordLimit)
    {
        var path = GetPackPath(chat);

        var a = MeasureDick(chat, baka, path);
        var sw = Stopwatch.StartNew();
        var consumed = texts
            .Where(text => text.IsNotNull_NorWhiteSpace()
                        && text.Count(c => c == ' ' || c == '\n') < wordLimit)
            .Count(baka.Eat);
        sw.Log("/fuse (file)");
        var b = MeasureDick(chat, baka, path);

        return new FeedReport(GetReport(a, b), consumed);
    }

    private static PackProps MeasureDick // 😂🤣🤣🤣👌
        (long chat, Copypaster baka, FilePath path)
    {
        Save(chat, baka);
        return (path.FileSizeInBytes, baka.Pack.VocabularyCount);
    }

    private static FuseReport GetReport(PackProps a, PackProps b) => new
    (
        NewSize:     b.packSize                .ReadableFileSize(),
        DeltaSize:  (b.packSize  - a.packSize ).ReadableFileSize(),
        DeltaCount: (b.wordCount - a.wordCount).Format_bruh_1k_100k_1M("💨")
    );

    // MOVE

    /// Moves pack file to the public or private archive folder.
    /// If the pack is in the memory, it's saved and unloaded.
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static string? Move(long chat, string name, bool publish)
    {
        if (BakaIsLoaded(chat, out var baka))
        {
            Save(chat, baka);
            Drop(chat);
        }

        var source = GetPackPath(chat);
        if (source.FileExists)
        {
            var subdir = publish ? null : chat.ToString();
            var target = GetExtraPackPath(name, subdir);

            File.Move(source, target);

            return target.FileNameWithoutExtension;
        }
        else
            return null; // nothing to move
    }

    public static FilePath GetExtraPackPath(string name, string? subdir = null)
    {
        var name_safe = name.Replace(' ', '-').ValidFileName('-');
        var directory = subdir == null
            ? Dir_Fuse
            : Dir_Fuse.Combine(subdir);
        var suffix = name_safe is "info" // todo - test !@*
            ? $"_{Desert.GetSand(2)}"
            : null;

        return directory
            .EnsureDirectoryExist()
            .Combine($"{name_safe}{suffix}{Ext_Pack}")
            .MakeUnique();
    }

    // DELETE

    /// Irreversibly deletes the pack file!
    public static void Delete(long chat)
    {
        if (BakaIsLoaded(chat)) Drop(chat);

        File.Delete(GetPackPath(chat));
    }
}