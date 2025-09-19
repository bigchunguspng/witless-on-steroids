using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using PF_Bot.Core.Text;
using PF_Tools.Copypaster;
using PF_Tools.Copypaster.Helpers;

namespace PF_Bot.Core.Chats;

public static class ChatManager
{
    private const byte MAX_IDLE_BEFORE_UNLOAD = 10;

    public static readonly SyncDictionary<long, Copypaster>   LoadedBakas = new();
    public static readonly SyncDictionary<long, ChatSettings> SettingsDB
        =  JsonIO.LoadData<SyncDictionary<long, ChatSettings>>(File_Chats);


    // PATHS

    public static FilePath GetPackPath
        (long chat) => Path.Combine(Dir_Chat, $"{chat}{Ext_Pack}");

    public static FilePath GetPacksFolder
        (long chat, bool @private) => @private 
        ? GetPrivatePacksFolder(chat) 
        : Dir_Fuse;
    public static FilePath GetFilesFolder
        (long chat, bool @private) => @private 
        ? GetPrivateFilesFolder(chat) 
        : Dir_History;

    public static FilePath GetPrivatePacksFolder
        (long chat) => Dir_Fuse   .Combine(chat.ToString());
    public static FilePath GetPrivateFilesFolder
        (long chat) => Dir_History.Combine(chat.ToString());


    // CHATLIST / SETTINGS

    public static bool KnownsChat(long chat)
        => SettingsDB.ContainsKey(chat);

    public static bool KnownsChat(long chat, [NotNullWhen(true)] out ChatSettings? settings)
        => SettingsDB.TryGetValue(chat, out settings);

    public static bool TryAddChat(long chat, bool privateChat)
        => SettingsDB.TryAdd(chat, ChatSettingsFactory.CreateFor(privateChat));

    public static void RemoveChat(long chat)
        => SettingsDB.Remove(chat);

    public static void SaveChatsDB()
    {
        SettingsDB.Lock(x => JsonIO.SaveData(x, File_Chats));
        Log("CHATLIST SAVED", LogLevel.Info, LogColor.Lime);
    }


    // PACKS / BAKAS

    public static bool BakaIsLoaded(long chat)
        => LoadedBakas.ContainsKey(chat);

    public static bool BakaIsLoaded(long chat, [NotNullWhen(true)] out Copypaster? baka)
        => LoadedBakas.TryGetValue(chat, out baka);

    /// <b>HIGH MEMORY USAGE!</b> Call this to load <see cref="GenerationPack"/> into memory.
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static Copypaster GetBaka(long chat)
    {
        return LoadedBakas.TryGetValue(chat, out var baka) ? baka : LoadBaka(chat);
    }

    // SAVE ALL

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
                Bakas_SaveDirty_UnloadIdle();
            }
            catch (Exception e)
            {
                LogError($"AUTOSAVE FAIL | {e.GetErrorMessage()}");
            }
        }
    }

    public static void Bakas_SaveDirty_UnloadIdle()
    {
        Bakas_SaveDirty();
        Bakas_UnloadIdle();
    }

    public static void Bakas_SaveDirty
        () => LoadedBakas.ForEachPair(x => SaveBaka(x.Key, x.Value));

    // SAVE / LOAD

    /// Saves <see cref="baka"/> if it's dirty.
    public static void SaveBaka(long chat, Copypaster baka)
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

    private static Copypaster LoadBaka(long chat)
    {
        try
        {
            Copypaster baka = null!;

            var log = PackIO_MeasureSpeed(chat, path =>
            {
                baka = new Copypaster(GenerationPackIO.Load(path));
                LoadedBakas.Add(chat, baka);
            });

            Log($"DIC LOAD | {chat,14} | {log}", LogLevel.Info, LogColor.Fuchsia);

            return baka;
        }
        catch
        {
            LogError($"CAN'T LOAD DIC | {chat,14}");
            throw;
        }
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

    // UNLOAD

    private static void Bakas_UnloadIdle
        () => LoadedBakas.ForEachPair(x => UnloadBaka_IfIdle(x.Key, x.Value));

    private static void UnloadBaka_IfIdle(long chat, Copypaster baka)
    {
        if (baka.Idle >= MAX_IDLE_BEFORE_UNLOAD) UnloadBaka(chat);
        else
            baka.BumpIdle();
    }

    private static void UnloadBaka(long chat)
    {
        LoadedBakas.Remove(chat);
        Log($"DIC DROP | {chat,14}", LogLevel.Info, LogColor.Yellow);
    }

    // CLEAR / DELETE

    public static void ClearPack(long chat, Copypaster baka)
    {
        baka.ClearPack();
        SaveBaka(chat, baka);
    }

    public static void DeletePack(long chat)
    {
        UnloadBaka(chat);
        File.Delete(GetPackPath(chat));
    }
}