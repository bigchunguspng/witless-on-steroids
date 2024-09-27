using System.Diagnostics.CodeAnalysis;
using Witlesss.Backrooms.Types;

namespace Witlesss;

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
        JsonIO.SaveData(SettingsDB, File_Chats);
        Log("CHATLIST SAVED", ConsoleColor.Green);
    }


    // BAKAS

    public static bool BakaIsLoaded(long chat)
        => LoadedBakas.ContainsKey(chat);

    public static bool BakaIsLoaded(long chat, [NotNullWhen(true)] out CopypasterProxy? baka)
        => LoadedBakas.TryGetValue(chat, out baka);

    /// <summary>
    /// <b>HIGH MEMORY USAGE!</b> Use only when you actually need the <see cref="Generation.Pack.GenerationPack"/>.
    /// </summary>
    public static CopypasterProxy GetBaka(long chat)
    {
        return LoadedBakas.TryGetValue(chat, out var baka) ? baka : LoadBaka(chat);
    }

    public static async void StartAutoSaveAsync(TimeSpan interval)
    {
        while (true)
        {
            await Task.Delay(interval);
            SaveBakas();
        }
    }

    public static void SaveBakasBeforeExit() => ForEachChat(baka => baka.SaveChanges());
    public static void SaveBakas()
    {
        ForEachChat(baka => baka.SaveChanges());
        var uselessChats = LoadedBakas.Where(x => x.Value.IsUselessEnough()).Select(x => x.Key);
        foreach (var chat in uselessChats) UnloadBaka(chat);
    }

    private static void ForEachChat(Action<CopypasterProxy> action)
    {
        lock (LoadedBakas.Sync) LoadedBakas.Values.ForEach(action);
    }

    private static CopypasterProxy LoadBaka(long chat)
    {
        var baka = new CopypasterProxy(chat);
        LoadedBakas.Add(chat, baka);
        Log($"DIC LOADED >> {chat}", ConsoleColor.Magenta);

        return baka;
    }

    public static void UnloadBaka(long chat)
    {
        LoadedBakas.Remove(chat);
        Log($"DIC UNLOAD << {chat}", ConsoleColor.Yellow);
    }

    public static void BackupAndDeletePack(long chat)
    {
        Backup(chat);
        DeletePack(chat);
    }

    public static void DeletePack(long chat)
    {
        UnloadBaka(chat);
        File.Delete(GetPath(chat));
    }

    public static void Backup(long chat)
    {
        if (BakaIsLoaded(chat, out var baka)) baka.SaveChanges();

        var file = new FileInfo(GetPath(chat));
        if (file.Length is <= 34 or >= 4_000_000) return; // don't backup empty and big ones

        var date = DateTime.Now.ToString("yyyy-MM-dd");
        var name = $"{Prefix_Pack}-{chat}.json";
        file.CopyTo(UniquePath(Path.Combine(Dir_Backup, date), name));
    }
}