using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Witlesss.Backrooms.Types;

namespace Witlesss.Telegram;

public static class ChatService
{
    public static readonly SyncDictionary<long, string>          PackPaths   = new();
    public static readonly SyncDictionary<long, CopypasterProxy> LoadedBakas = new();
    public static readonly SyncDictionary<long, ChatSettings>    SettingsDB
        =  JsonIO.LoadData<SyncDictionary<long, ChatSettings>>(File_Chats);


    public static string GetPath(long chat)
    {
        if (PackPaths.TryGetValue(chat, out var path) == false)
        {
            path = Path.Combine(Dir_Chat, $"{Prefix_Pack}-{chat}.json");
            PackPaths.Add(chat, path);
        }

        return path;
    }


    // SETTINGS

    public static bool Knowns(long chat)
        => SettingsDB.ContainsKey(chat);

    public static bool Knowns(long chat, [NotNullWhen(true)] out ChatSettings? settings)
        => SettingsDB.TryGetValue(chat, out settings);

    public static bool TryAddChat(long chat, ChatSettings settings) => SettingsDB.TryAdd(chat, settings);
    public static void RemoveChat(long chat)                        => SettingsDB.Remove(chat);

    public static void SaveChatsDB()
    {
        lock (SettingsDB.Sync) JsonIO.SaveData(SettingsDB, File_Chats);
        Log("CHATLIST SAVED", LogLevel.Info, LogColor.Lime);
    }


    // BAKAS

    public static bool BakaIsLoaded(long chat)
        => LoadedBakas.ContainsKey(chat);

    public static bool BakaIsLoaded(long chat, [NotNullWhen(true)] out CopypasterProxy? baka)
        => LoadedBakas.TryGetValue(chat, out baka);

    /// <summary>
    /// <b>HIGH MEMORY USAGE!</b> Use only when you actually need the <see cref="Generation.Pack.GenerationPack"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static CopypasterProxy GetBaka(long chat)
    {
        return LoadedBakas.TryGetValue(chat, out var baka) ? baka : LoadBaka(chat);
    }

    public static async void StartAutoSaveAsync(TimeSpan interval)
    {
        while (true)
        {
            await Task.Delay(interval);
            PerformAutoSave();
        }
    }

    public static void PerformAutoSave()
    {
        SaveBakas();
        UnloadUselessBakas();
    }

    public static void SaveBakas
        () => LoadedBakas.ForEachValue(baka => baka.SaveChanges());

    public static void UnloadUselessBakas
        () => LoadedBakas.ForEachPair(x => { if (x.Value.IsUselessEnough()) UnloadBaka(x.Key); });

    // LOAD / UNLOAD

    private static CopypasterProxy LoadBaka(long chat)
    {
        try
        {
            var baka = new CopypasterProxy(chat);
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

    private static void UnloadBaka(long chat)
    {
        LoadedBakas.Remove(chat);
        Log($"DIC DROP << {chat}", LogLevel.Info, LogColor.Yellow);
    }

    // DELETE / BACKUP

    public static void DeletePack(long chat)
    {
        UnloadBaka(chat);
        File.Delete(GetPath(chat));
    }

    public static void BackupPack(long chat)
    {
        if (BakaIsLoaded(chat, out var baka)) baka.SaveChanges();

        var file = new FileInfo(GetPath(chat));
        if (file.Length is <= 34 or >= 4_000_000) return; // don't backup empty and big ones

        var date = DateTime.Now.ToString("yyyy-MM-dd");
        var name = $"{Prefix_Pack}-{chat}.json";
        file.CopyTo(UniquePath(Path.Combine(Dir_Backup, date), name));
    }
}