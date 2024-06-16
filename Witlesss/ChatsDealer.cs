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

    public static void LoadSomeBakas()
    {
        var twoHours = TimeSpan.FromHours(2);

        var files = new DirectoryInfo(Paths.Dir_Chat).GetFiles(Paths.Prefix_Pack + "-*.json"); // todo IL
        var selection = files
            .Where (file => file.Length < 4_000_000 && file.LastWriteTime.HappenedWithinLast(twoHours))
            .Select(file => long.Parse(_pack.Match(file.Name).Groups[1].Value));
        foreach (var chat in selection) LoadWitless(chat);
    }

    private static void LoadWitless(long chat)
    {
        if (SussyBakas.TryGetValue(chat, out var baka)) baka.LoadUnlessLoaded();
    }

    public static bool WitlessExist(long chat, [NotNullWhen(true)] out Witless? baka)
    {
        var exist = SussyBakas.TryGetValue(chat, out baka);
        if (exist) baka!.LoadUnlessLoaded();

        return exist;
    }

    public static bool WitlessExist(long chat)
    {
        var exist = SussyBakas.ContainsKey(chat);
        if (exist)  SussyBakas[chat].LoadUnlessLoaded();

        return exist;
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

    public static void SaveBakas           () => ForEachChat(witless => witless.SaveAndCount());
    public static void SaveBakasBeforeExit () => ForEachChat(witless => witless.Save());

    private static void ForEachChat(Action<Witless> action)
    {
        lock (SussyBakas.Sync) SussyBakas.Values.ForEach(action);
    }

    public static void RemoveChat(long id) => SussyBakas.Remove(id);
}