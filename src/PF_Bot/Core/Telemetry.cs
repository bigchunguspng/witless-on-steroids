using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PF_Bot.Core;

public enum CommandResultStatus // todo rename ProcessingStatus
{
    OK,   // all's good, according to keikaku
    MAN,  // bad syntax -> manual's sent
    BAD,  // 404 and similar
    FAIL, // error was thrown ((9
    DENY, // massive skill issue
}

public enum AutoType
{
    TEXT,
    MEME,
    AUTO,
    PIPE,
}

/* ==== Legend

-/T/PSGVAD/? = message types (none, text, file type, unknown)

AB = A - message.ReplyTo, B - message (that triggered action)
AB = B replies to A
TT = text reply to text
PT = text reply to photo
-T = just text

*/

/* ==== Templates

COMMAND:                   ->         /
10/14 21:43:13.280 | 12345 -> OK   -T /stickers
10/14 21:42:13.828 |  CHAT -> OK   PT /toplarg@bot text
10/14 21:42:13.828 |  CHAT -> MAN  -P /im implode

AUTO:                      <-         @
10/14 21:42:13.828 |  CHAT <- OK   -P @MEME  25% /mememm!
10/14 21:42:13.828 |  CHAT <- OK   PP @MEME 150% /toplarg text
10/14 21:42:13.828 |  CHAT <- FAIL -V @PIPE 48XA /scale 0.5
10/14 21:42:13.828 |  CHAT <- FAIL -V @AUTO 48XA /pipe scale 0.5 > /nuke > /damn 15
10/14 21:42:13.828 |  CHAT <- OK   -T @TEXT  20%

CALLBACK:                  ->      *
10/14 21:42:13.828 |  CHAT -> OK   *bi - 8 10

INLINE:                    --      @
10/14 21:42:13.828 |  USER -- OK   @bot funny
10/14 21:42:13.828 |  USER -- OK   @bot a sfx

BOT EVENTS                 >>
10/14 21:42:13.828 | START >> @bot | Adidas
10/14 21:42:13.828 |  SAVE >> CHATS 1280 | PACKS   15 | SAVE    3 | DROP    2
10/14 21:42:13.828 | ADMIN >> /w lol kek
10/14 21:42:13.828 |  EXIT >> @bot | Adidas

*/

public static class BigBrother
{
    private static readonly FileLogger_Batch _logger = new (File_Log);

    // TELEGRAM

    public static void LogCommand
        (long chat, CommandResultStatus status, Message message, string? input)
    {
        var (A, B) = GetMessageType(message);
        LogTelegram(chat, $"-> {status,-4} {A}{B} {input}");
    }

    public static void LogAuto
        (long chat, CommandResultStatus status, Message message, AutoType type, int chance, string? input = null)
    {
        var (A, B) = GetMessageType(message);
        LogTelegram(chat, $"<- {status,-4} {A}{B} @{type} {chance,3}% {input}");
    }

    public static void LogAuto
        (long chat, CommandResultStatus status, Message message, AutoType type, string id_4char, string? input = null)
    {
        var (A, B) = GetMessageType(message);
        LogTelegram(chat, $"<- {status,-4} {A}{B} @{type} {id_4char} {input}");
    }

    public static void LogCallback
        (long chat, CommandResultStatus status, string? data)
    {
        LogTelegram(chat, $"-> {status,-4} *{data}");
    }

    public static void LogInline
        (long user, CommandResultStatus status, string input)
    {
        LogTelegram(user, $"-- {status,-4} {App.Bot.Username} {input}");
    }

    private static void LogTelegram(long id, string text)
    {
        var id_last5digits = id.ToString("#00000").AsSpan(^5);
        Log(id_last5digits, text.Replace("\n", "[N]"));
    }

    //

    private static (char A, char B) GetMessageType(Message message) =>
    (
        GetMessageTypeChar(message.ReplyToMessage),
        GetMessageTypeChar(message)
    );

    private static char GetMessageTypeChar
        (Message? m) => m == null ? '-' : m.Type switch
    {
        MessageType.Text      => 'T',
        MessageType.Photo     => 'P',
        MessageType.Sticker   => 'S',
        MessageType.Animation => 'G',
        MessageType.Video     => 'V',
        MessageType.VideoNote => 'V',
        MessageType.Audio     => 'A',
        MessageType.Voice     => 'A',
        MessageType.Document  => 'D',
        _                     => '?',
    };

    // BOT EVENTS

    public static void Log_SAVE
        (int chats, int packs, int saved, int dropped)
        => Log(" SAVE", $">> CHATS {chats,4} | PACKS {packs,4} | SAVE {saved,4} | DROP {dropped,4}");

    public static void Log_ADMIN
        (string input)
        => Log("ADMIN", $">> {input}");

    public static void Log_START
        ()
        => Log("START", $">> {App.Bot.Username} | {App.Bot.Me.FirstName}");

    public static void Log_EXIT
        ()
        => Log(" EXIT", $">> {App.Bot.Username} | {App.Bot.Me.FirstName}\n");

    //

    private static void Log(ReadOnlySpan<char> id_5char, string text)
    {
        _logger.Log($"{DateTime.Now:MM'/'dd' 'HH:mm:ss.fff} | {id_5char} {text}");
    }

    public static void Write() => _logger.Write();
}