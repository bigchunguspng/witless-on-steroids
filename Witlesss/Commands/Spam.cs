using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Witlesss.Commands;

public class Spam : Command
{
    public override void Run()
    {
        if (Message.From?.Id != Config.AdminID)
        {
            Bot.SendMessage(Chat, "LOL XD)0)");
            return;
        }

        var split = Text.Split(' ', 3);
        if (split.Length < 3)
        {
            Bot.SendMessage(Chat, "<code>/spam [min size] [message]</code>");
            return;
        }

        var size = int.TryParse(split[1], out var x) ? x : 2_000_000;

        Task.Run(() => SendSpam(size, split[2]));
    }

    public static void SendSpam(int size = 2_000_000, string text = null)
    {
        try
        {
            var message = text ?? File.ReadAllText("spam.txt");
            var bakas = Bot.SussyBakas.Values.Where(x =>
            {
                if (File.Exists(x.Path))
                {
                    var file = new FileInfo(x.Path);
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