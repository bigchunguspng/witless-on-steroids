using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using PF_Bot.Backrooms.Types;
using PF_Bot.Core.Generation;
using PF_Bot.Tools_Legacy.Technical;
using PF_Tools.Copypaster;

namespace PF_Bot.Core.Chats;

public static class ChatManager
{
    private const byte MAX_IDLE_BEFORE_UNLOAD = 10;

    public static readonly SyncDictionary<long, Copypaster>   LoadedBakas = new();
    public static readonly SyncDictionary<long, ChatSettings> SettingsDB
        =  JsonIO.LoadData<SyncDictionary<long, ChatSettings>>(File_Chats);


    // PATHS

    public static string GetPackPath
        (long chat) => Path.Combine(Dir_Chat, $"{chat}.pack");


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
        lock (SettingsDB.Sync) JsonIO.SaveData(SettingsDB, File_Chats);
        Log("CHATLIST SAVED", LogLevel.Info, LogColor.Lime);
    }


    // PACKS / BAKAS

    public static bool BakaIsLoaded(long chat)
        => LoadedBakas.ContainsKey(chat);

    public static bool BakaIsLoaded(long chat, [NotNullWhen(true)] out Copypaster? baka)
        => LoadedBakas.TryGetValue(chat, out baka);

    /// <summary>
    /// <b>HIGH MEMORY USAGE!</b> Use only when you actually need the <see cref="GenerationPack"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static Copypaster GetBaka(long chat)
    {
        return LoadedBakas.TryGetValue(chat, out var baka) ? baka : LoadBaka(chat);
    }

    public static void StartAutoSaveAsync(TimeSpan interval)
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
                LogError($"AUTOSAVE >> FAIL >> {e}");
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

    /// Saves <see cref="baka"/> if it's dirty.
    public static void SaveBaka(long chat, Copypaster baka)
    {
        if (!baka.IsDirty) return;

        var path = GetPackPath(chat);
        var temp = $"{path}~";
        lock (baka)
        {
            GenerationPackIO.Save(baka.Pack, path, temp);
            baka.ResetDirty();
        }
        Log($"DIC SAVE << {chat}", LogLevel.Info, LogColor.Lime);
    }

    // LOAD / UNLOAD

    private static Copypaster LoadBaka(long chat)
    {
        try
        {
            var baka = new Copypaster(GenerationPackIO.Load(GetPackPath(chat)));
            LoadedBakas.Add(chat, baka);
            Log($"DIC LOAD >> {chat}", LogLevel.Info, LogColor.Fuchsia);

            return baka;
        }
        catch
        {
            LogError($"CAN'T LOAD DIC >> {chat}");
            throw;
        }
    }

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
        Log($"DIC DROP << {chat}", LogLevel.Info, LogColor.Yellow);
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