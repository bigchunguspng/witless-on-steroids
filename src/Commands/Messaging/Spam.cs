namespace Witlesss.Commands.Messaging;

public class Spam : SyncCommand
{
    private readonly Regex _days = new(@"a(\d+)"), _size = new(@"s(\d+)");

    protected override void Run()
    {
        if (Message.From?.Id != Config.AdminID)
        {
            Bot.SendMessage(Chat, "LOL XD)0)");
            return;
        }

        var messageId = Message.ReplyToMessage is { } reply ? reply.MessageId : -1;

        var textProvided = Args is not null;
        var copyProvided = messageId >= 0;

        if (!textProvided && !copyProvided)
        {
            Bot.SendMessage(Chat, "<code>/spam[g/aN/sB] [reply / message]</code>");
            return;
        }

        var groupsOnly = Command!.Contains('g');

        var size = _size.ExtractGroup(1, Command, int.Parse, 2_000_000);
        var days = _days.ExtractGroup(1, Command, int.Parse, 28);

        var chat = Chat;
        var text = Args!;
        var bakas = GetBakas(size, groupsOnly, TimeSpan.FromDays(days));

        if (textProvided) Task.Run(() => SendSpam(bakas, text));
        else              Task.Run(() => CopySpam(bakas, chat, messageId));
    }

    private static void SendSpam(IEnumerable<Witless> bakas, string text)
    {
        foreach (var witless in bakas)
        {
            Bot.SendMessage(witless.Chat, text, preview: true);
            LogSpam(witless.Chat);
        }
    }

    private static void CopySpam(IEnumerable<Witless> bakas, long chat, int messageId)
    {
        foreach (var witless in bakas)
        {
            Bot.CopyMessage(witless.Chat, chat, messageId);
            LogSpam(witless.Chat);
        }
    }

    private static IEnumerable<Witless> GetBakas(int minSize, bool groupsOnly, TimeSpan lastActivity)
    {
        return ChatsDealer.SussyBakas.Values.Where(x =>
        {
            var path = x.FilePath;
            if (File.Exists(path))
            {
                var file = new FileInfo(path);
                return file.Length >= minSize
                    && (!groupsOnly || x.Chat.ChatIsNotPrivate())
                    && file.LastWriteTime.HappenedWithinLast(lastActivity);
            }

            return false;
        });
    }

    private static void LogSpam(long chat) => Log($"MAIL SENT << {chat}", ConsoleColor.Yellow);
}