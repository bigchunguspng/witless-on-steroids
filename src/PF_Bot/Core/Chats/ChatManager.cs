using System.Diagnostics.CodeAnalysis;

namespace PF_Bot.Core.Chats;

public static class ChatManager
{
    public static readonly SyncDictionary<long, ChatSettings> Chats
        =  JsonIO.LoadData<SyncDictionary<long, ChatSettings>>(File_Chats);


    public static bool Knowns(long chat)
        => Chats.ContainsKey(chat);

    public static bool Knowns(long chat, [NotNullWhen(true)] out ChatSettings? settings)
        => Chats.TryGetValue(chat, out settings);

    public static bool TryAdd(long chat, bool privateChat)
        => Chats.TryAdd(chat, ChatSettingsFactory.CreateFor(privateChat));

    public static void Remove(long chat)
        => Chats.Remove(chat);

    public static void SaveChats()
    {
        Chats.Lock(x => JsonIO.SaveData(x, File_Chats));
        Log("CHATLIST SAVED", LogLevel.Info, LogColor.Lime);
    }
}