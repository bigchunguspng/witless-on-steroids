using System.Diagnostics.CodeAnalysis;

namespace Witlesss;

public static class ChatsDealer
{
    public  static readonly ChatList SussyBakas;

    static ChatsDealer()
    {
        SussyBakas = JsonIO.LoadData<ChatList>(File_Chats);
    }

    public static bool WitlessExist(long chat, [NotNullWhen(true)] out Witless? baka)
    {
        return SussyBakas.TryGetValue(chat, out baka);
    }

    public static bool WitlessExist(long chat)
    {
        return SussyBakas.ContainsKey(chat);
    }

    public static void SaveChatList()
    {
        JsonIO.SaveData(SussyBakas, File_Chats);
        Log(LOG_CHATLIST_SAVED, ConsoleColor.Green);
    }

    public static async void StartAutoSaveLoop(int minutes)
    {
        while (true)
        {
            await Task.Delay(60000 * minutes);
            SaveBakas();
        }
    }

    public static void SaveBakas           () => ForEachChat(witless => witless.SaveChangesOrUnloadIfUseless());
    public static void SaveBakasBeforeExit () => ForEachChat(witless => witless.SaveChanges());

    private static void ForEachChat(Action<Witless> action)
    {
        lock (SussyBakas.Sync) SussyBakas.Values.ForEach(action);
    }

    public static void RemoveChat(long id) => SussyBakas.Remove(id);
}