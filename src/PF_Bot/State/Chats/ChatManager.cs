using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using PF_Bot.Backrooms.Types;
using PF_Bot.Generation;
using PF_Tools.Copypaster;

namespace PF_Bot.State.Chats;

public static class ChatManager
{
    public static readonly SyncDictionary<long, CopypasterProxy> LoadedBakas = new();
    public static readonly SyncDictionary<long, ChatSettings>    SettingsDB
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

    public static bool BakaIsLoaded(long chat, [NotNullWhen(true)] out CopypasterProxy? baka)
        => LoadedBakas.TryGetValue(chat, out baka);

    /// <summary>
    /// <b>HIGH MEMORY USAGE!</b> Use only when you actually need the <see cref="GenerationPack"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static CopypasterProxy GetBaka(long chat)
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
                PerformAutoSave();
            }
            catch (Exception e)
            {
                LogError($"AUTOSAVE >> FAIL >> {e}");
            }
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
            var baka = new CopypasterProxy(chat, GenerationPackIO.Load(GetPackPath(chat)));
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

    // DELETE

    public static void DeletePack(long chat)
    {
        UnloadBaka(chat);
        File.Delete(GetPackPath(chat));
    }
}