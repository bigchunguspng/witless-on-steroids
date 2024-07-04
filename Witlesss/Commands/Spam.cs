using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Witlesss.Commands;

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
        if (args is null || args.Length < 2)
        {
            Bot.SendMessage(Chat, "<code>/spam [min size] [message]</code>");
            return;
        }

        var size = int.TryParse(args[0], out var x) ? x : 2_000_000;

        Task.Run(() => SendSpam(size, args[2]));
    }

    public static void SendSpam(int size = 2_000_000, string? text = null)
    {
        try
        {
            var message = text ?? File.ReadAllText(Paths.File_Spam);
            var bakas = ChatsDealer.SussyBakas.Values.Where(x =>
            {
                if (File.Exists(x.FilePath))
                {
                    var file = new FileInfo(x.FilePath);
                    return file.Length > size && file.LastWriteTime.HappenedWithinLast(TimeSpan.FromDays(28));
                }
                return false;
            });
            foreach (var witless in bakas)
            {
                Bot.SendMessage(witless.Chat, message, preview: false);
                Log($"MAIL SENT << {witless.Chat}", ConsoleColor.Yellow);
            }
        }
        catch (Exception e)
        {
            LogError("SoRRY, CAN'T SPAM, BRO x_x " + e.Message);
        }
    }
}