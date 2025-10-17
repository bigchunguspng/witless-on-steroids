using System.Text;
using PF_Bot.Core;
using PF_Bot.Routing.Callbacks;
using PF_Bot.Routing.Commands;
using Telegram.Bot.Types.ReplyMarkups;

namespace PF_Bot.Commands;

// /help == /man
// /help        -> main menu
// /help ffmpeg -> page exist ? navigated to page : ^
// /help 44     -> ^
// /help_44     -> ^
// man - 44     -> ^ (ffmpeg)

public class Help_Callback : CallbackHandler
{
    protected override Task Run()
    {
        RTFM.SendManualPage(Origin, Content, Message.Id);
        return Task.CompletedTask;
    }
}

public class Help : CommandHandlerBlocking
{
    protected override void Run()
    {
        var query = Args ?? GetOptionArgs() ?? "";
        RTFM.SendManualPage(Origin, query);
        Log($"{Title} >> MAN {query}");
    }

    private string? GetOptionArgs()
    {
        var    i  =     Options.IndexOf('_');
        return i >= 0 ? Options.Substring(i + 1) : null;
    }
}

public static class RTFM
{
    public static void SendManualPage(MessageOrigin origin, string query, int message = -1)
    {
        var path = FindPage(query);
        var page = new ManualPage(path);

        var text     = page.GetPageContent();
        var keyboard = page.GetNavigationKeyboard();

        App.Bot.SendOrEditMessage(origin, text, message, keyboard);
    }

    /// <param name="query"> page code / a part of page title. </param>
    private static FilePath FindPage(string query)
        => Directory.GetFiles(Dir_Manual,   $"{query} *").FirstOrDefault()
        ?? Directory.GetFiles(Dir_Manual, $"* *{query}*").FirstOrDefault()
        ?? Directory.GetFiles(Dir_Manual,         "0 *", _options).First();

    private static readonly EnumerationOptions
        _options = new() { MatchCasing = MatchCasing.CaseInsensitive };
}

public readonly ref struct ManualPage
{
    private readonly string             _path;    // "Static/Manual/44 FFMpeg.html"
    private readonly ReadOnlySpan<char> _address; // "44"
    private readonly bool               _mainPage;

    public ManualPage(string path)
    {
        _path = path;

        var name = Path.GetFileName(path.AsSpan());
        _address = name.Slice(0, name.IndexOf(' '));
        _mainPage = _address is "0";
    }

    // BUTTONS

    public InlineKeyboardMarkup GetNavigationKeyboard()
    {
        var keyboard = new List<List<InlineKeyboardButton>>();

        var buttons = GetButtons();

        var odd = buttons.Count.IsOdd();
        if (odd) keyboard.Add([buttons[0]]);

        var rows = buttons.Count / 2;
        var start = odd ? 1 : 0;
        for (var i = 0; i < rows; i++)
        {
            var a = buttons[start + i];
            var b = buttons[start + i + rows];
            keyboard.Add([a, b]);
        }

        if (_mainPage.Janai()) // append "back" button
        {
            var data = _address.Length == 1 ? "0" : _address[..^1];
            var back = InlineKeyboardButton.WithCallbackData("Назад", GetCallbackData(data));
            keyboard.Add([back]);
        }

        return new InlineKeyboardMarkup(keyboard);
    }

    private List<InlineKeyboardButton> GetButtons()
    {
        var pattern = _mainPage
            ?            "? *"  //          [1 digit] [title]
            : $"{_address}? *"; // [address][1 digit] [title]

        var files = Directory.GetFiles(Dir_Manual, pattern);
        var names = files
            .Select(Path.GetFileNameWithoutExtension)
            .OfType<string>();

        if (_mainPage)
            names = names.Where(file => file.StartsWith('0').Janai());
        
        return names
            .Select(file =>
            {
                var bits = file.Split(' ', 2);
                var code = bits[0];
                var name = bits[1];
                return InlineKeyboardButton.WithCallbackData(name, GetCallbackData(code));
            })
            .ToList();
    }

    private static string GetCallbackData
        (ReadOnlySpan<char> code) => $"{Registry.CallbackKey_Manual} - {code}";

    // CONTENT

    public string GetPageContent()
    {
        var sb = new StringBuilder();

        // HEADER
        sb.Append("📖 <u><b>").Append(GetPageTitle("0"));
        var jumps = _mainPage ? 0 : _address.Length;
        for (var i = 0; i < jumps; i++)
        {
            var jumpAddres = _address.Slice(0, 1 + i);
            sb.Append(" » ").Append(GetPageTitle(jumpAddres));
        }
        sb.Append("</b></u>");

        if (_mainPage.Janai())
        {
            sb.Append(" #<code>");
            sb.Append(_address);
            sb.Append("</code>");
        }

        // BODY
        sb.Append("\n\n").Append(File.ReadAllText(_path));

        return sb.ToString();
    }

    private static ReadOnlySpan<char> GetPageTitle(ReadOnlySpan<char> address)
    {
        var file = Directory.GetFiles(Dir_Manual, $"{address} *").First();
        var name = Path.GetFileNameWithoutExtension(file);
        var title_start = name.IndexOf(' ') + 1;
        return name.AsSpan(start: title_start);
    }

/*  == HOW IT WORKS == (Example)

Pages in Static/Manual/: 0 Main, 1 A, 2 B, 3 C, 31 D, 32 E

PAGE   | HEADER           | BUTTONS
0 Main | Main             |  1 A,  2 B,  3 C
3 C    | Main » C #3      |       31 D, 32 E, 0 Back
32 E   | Main » C » F #32 |                   3 Back  */
}