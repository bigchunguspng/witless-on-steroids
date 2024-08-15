namespace Witlesss.Commands.Messaging;

public class Spam : SyncCommand
{
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

        var size = 2_000_000;
        var match1 = Regex.Match(Command, @"s(\d+)");
        if (match1.Success) size = int.Parse(match1.Groups[1].Value);

        var activeDays = 28;
        var match2 = Regex.Match(Command, @"a(\d+)");
        if (match2.Success) activeDays = int.Parse(match2.Groups[1].Value);

        var chat = Chat;
        var text = Args!;
        var bakas = GetBakas(size, groupsOnly, TimeSpan.FromDays(activeDays));

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