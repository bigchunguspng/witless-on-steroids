using System.Diagnostics.CodeAnalysis;

namespace Witlesss;

public static class ChatService
{
    public static readonly ChatList SussyBakas = JsonIO.LoadData<ChatList>(File_Chats);

    public static bool WitlessExist
        (this long chat, [NotNullWhen(true)] out Witless? baka)
        => SussyBakas.TryGetValue(chat, out baka);

    public static bool WitlessExist
        (this long chat)
        => SussyBakas.ContainsKey(chat);

    public static bool TryAddChat(long chat, Witless baka) => SussyBakas.TryAdd(chat, baka);
    public static void RemoveChat(long chat)               => SussyBakas.Remove(chat);

    public static void SaveChatsDB()
    {
        JsonIO.SaveData(SussyBakas, File_Chats);
        Log("CHATLIST SAVED", ConsoleColor.Green);
    }

    public static async void StartAutoSaveAsync(TimeSpan interval)
    {
        while (true)
        {
            await Task.Delay(interval);
            SaveBakas();
        }
    }

    public static void SaveBakas          () => ForEachChat(baka => baka.SaveChangesOrUnloadIfUseless());
    public static void SaveBakasBeforeExit() => ForEachChat(baka => baka.SaveChanges());

    private static void ForEachChat(Action<Witless> action)
    {
        lock (SussyBakas.Sync) SussyBakas.Values.ForEach(action);
    }
}