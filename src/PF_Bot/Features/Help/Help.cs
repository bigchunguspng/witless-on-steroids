using System.Text;
using PF_Bot.Backrooms.Helpers;
using PF_Bot.Routing.Commands;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PF_Bot.Features.Help;

public class Help : SyncCommand
{
    // /help == /man
    // /help        -> main menu
    // /help ffmpeg -> page exist ? navigated to page : ^
    // /help 44     -> ^
    // /help_44     -> ^
    // man - 44     -> ^ (ffmpeg)
    protected override void Run()
    {
        var args = Args ?? (Command!.Contains('_') ? Command.Substring(Command.IndexOf('_') + 1) : "");
        Bot.SendMessage(Origin, GetManualPage(args, out var address), GetKeyboard(address));
    }

    public void HandleCallback(CallbackQuery query, string[] data)
    {
        var text = GetManualPage(data[1], out var address);
        Bot.EditMessage(query.GetChat(), query.GetMessage(), text, GetKeyboard(address));
    }

    // number path / search query
    private string GetManualPage(string path, out string address)
    {
        var options = new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive };
        var file = Directory.GetFiles(Dir_Manual,   $"{path} *").FirstOrDefault()
                ?? Directory.GetFiles(Dir_Manual, $"* *{path}*").FirstOrDefault()
                ?? Directory.GetFiles(Dir_Manual,         "0 *", options).First();

        var name = Path.GetFileNameWithoutExtension(file);

        address = name.Remove(name.IndexOf(' '));

        return BuildHeader(address).Append(File.ReadAllText(file)).ToString();
    }

    private StringBuilder BuildHeader(string address)
    {
        var paths = address
            .Select((_, i) => address[..i].Length == 0 ? "0" : address[..i]).ToList();
        if (address != "0") paths.Add(address);

        var sb = new StringBuilder("📖 <u><b>");
        for (var i = 0; i < paths.Count; i++)
        {
            var file = Directory.GetFiles(Dir_Manual, $"{paths[i]} *").First();
            var name = Path.GetFileNameWithoutExtension(file);
            sb.Append(name.AsSpan(name.IndexOf(' ') + 1));
            if (i < paths.Count - 1) sb.Append(" » ");
        }
        sb.Append("</b></u>");

        if (address != "0") sb.Append(" #<code>").Append(address).Append("</code>");
        return sb.Append("\n\n");
    }

    private static InlineKeyboardMarkup GetKeyboard(string address)
    {
        var isMainPage = address == "0";
        var files = Directory.GetFiles(Dir_Manual, isMainPage ? "? *" : $"{address}? *");
        var buttons = files
            .Select(Path.GetFileNameWithoutExtension)
            .OfType<string>()
            .Where(x => !(isMainPage && x.StartsWith('0')))
            .Select(x =>
            {
                var split = x.Split(' ', 2);
                return InlineKeyboardButton.WithCallbackData(split[1], CallbackData(split[0]));
            })
            .ToList();

        var keyboard = new List<List<InlineKeyboardButton>>();
        var odd = buttons.Count.IsOdd();
        if (odd) keyboard.Add([buttons[0]]);

        var rows = buttons.Count / 2;
        var start = odd ? 1 : 0;
        for (var i = 0; i < rows; i++)
        {
            keyboard.Add([buttons[start + i], buttons[start + i + rows]]);
        }

        if (!isMainPage)
        {
            var data = address.Length == 1 ? "0" : address[..^1];
            var button = InlineKeyboardButton.WithCallbackData("Назад", CallbackData(data));
            keyboard.Add([button]);
        }
        return new InlineKeyboardMarkup(keyboard);

        string CallbackData(string code) => $"man - {code}";
    }
}