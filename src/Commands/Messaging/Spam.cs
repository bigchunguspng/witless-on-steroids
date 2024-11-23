namespace Witlesss.Commands.Messaging;

public class Spam : SyncCommand
{
    private readonly Regex _days = new(@"a(\d+)"), _size = new(@"s(\d+)");

    protected override void Run()
    {
        if (!Message.SenderIsBotAdmin())
        {
            Bot.SendMessage(Chat, "LOL XD)0)");
            return;
        }

        var messageId = Message.ReplyToMessage is { } reply ? reply.Id : -1;

        var textProvided = Args is not null;
        var copyProvided = messageId >= 0;

        if (!textProvided && !copyProvided)
        {
            Bot.SendMessage(Chat, "<code>/spam[g/aN/sB] [reply / message]</code>");
            return;
        }

        var groupsOnly = Command!.Contains('g');

        var size = _size.ExtractGroup(1, Command, int.Parse, 00);
        var days = _days.ExtractGroup(1, Command, int.Parse, 28);

        var chat = Chat;
        var text = Args!;
        var bakas = GetChats(size, groupsOnly, TimeSpan.FromDays(days));

        if (textProvided) Task.Run(() => SendSpam(bakas, text));
        else              Task.Run(() => CopySpam(bakas, chat, messageId));
    }

    private static void SendSpam(IEnumerable<long> chats, string text)
    {
        foreach (var chat in chats)
        {
            Bot.SendMessage(chat, text, preview: true);
            LogSpam(chat);
        }
    }

    private static void CopySpam(IEnumerable<long> chats, long fromChat, int messageId)
    {
        foreach (var chat in chats)
        {
            Bot.CopyMessage(chat, fromChat, messageId);
            LogSpam(chat);
        }
    }

    private static IEnumerable<long> GetChats(int minSize, bool groupsOnly, TimeSpan lastActivity)
    {
        return ChatService.SettingsDB.Do(x => x.Keys.Where(chat =>
        {
            var path = ChatService.GetPath(chat);
            if (File.Exists(path))
            {
                var file = new FileInfo(path);
                return file.Length >= minSize
                    && (!groupsOnly || chat.ChatIsNotPrivate())
                    && file.LastWriteTime.HappenedWithinLast(lastActivity);
            }

            return false;
        }));
    }

    private static void LogSpam(long chat) => Log($"SPAM >> {chat}", LogLevel.Info, 11);
}