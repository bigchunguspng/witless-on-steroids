using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        var args = Args?.Split(' ', 2);
        var messageId = Message.ReplyToMessage is { } reply ? reply.MessageId : -1;

        var sizeProvided = args is not null && args.Length > 0;
        var textProvided = args is not null && args!.Length >= 2;
        var copyProvided = messageId >= 0;

        if (!sizeProvided || !textProvided && !copyProvided)
        {
            Bot.SendMessage(Chat, "<code>/spam [min size] [message]</code>");
            return;
        }

        var size = int.TryParse(args![0], out var x) ? x : 2_000_000;
        var chat = Chat;

        if (textProvided) Task.Run(() => SendSpam(size, args[2]));
        else              Task.Run(() => CopySpam(size, chat, messageId));
    }

    public static void SendSpam(int size = 2_000_000, string? text = null)
    {
        try
        {
            var message = text ?? File.ReadAllText(File_Spam);
            foreach (var witless in GetBakas(size))
            {
                Bot.SendMessage(witless.Chat, message, preview: false);
                LogSpam(witless.Chat);
            }
        }
        catch (Exception e)
        {
            LogFail(e);
        }
    }

    private static void CopySpam(int size, long chat, int messageId)
    {
        try
        {
            foreach (var witless in GetBakas(size))
            {
                Bot.CopyMessage(witless.Chat, chat, messageId);
                LogSpam(witless.Chat);
            }
        }
        catch (Exception e)
        {
            LogFail(e);
        }
    }

    private static IEnumerable<Witless> GetBakas(int size)
    {
        return ChatsDealer.SussyBakas.Values.Where(x =>
        {
            var path = x.FilePath;
            if (File.Exists(path))
            {
                var file = new FileInfo(path);
                return file.Length > size && file.LastWriteTime.HappenedWithinLast(TimeSpan.FromDays(28));
            }
            return false;
        });
    }

    private static void LogSpam(long chat) => Log($"MAIL SENT << {chat}", ConsoleColor.Yellow);
    private static void LogFail(Exception e) => LogError("SoRRY, CAN'T SPAM, BRO x_x " + e.Message);
}