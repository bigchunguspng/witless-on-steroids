using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Witlesss;

public static class ChatsDealer
{
    private static readonly FileIO<ChatList> ChatsIO;
    public  static readonly ChatList SussyBakas;

    private static readonly Regex _pack = new($@"{Paths.Prefix_Pack}-(-?\d+).json");

    static ChatsDealer()
    {
        ChatsIO = new FileIO<ChatList>(Paths.File_Chats);
        SussyBakas = ChatsIO.LoadData();
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
        ChatsIO.SaveData(SussyBakas);
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

    public static void SaveBakas           () => ForEachChat(witless => witless.SaveChangesOrUnloadInactive());
    public static void SaveBakasBeforeExit () => ForEachChat(witless => witless.SaveChanges());

    private static void ForEachChat(Action<Witless> action)
    {
        lock (SussyBakas.Sync) SussyBakas.Values.ForEach(action);
    }

    public static void RemoveChat(long id) => SussyBakas.Remove(id);
}