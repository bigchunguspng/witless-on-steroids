using Telegram.Bot.Extensions;
using Telegram.Bot.Types;

namespace Witlesss.Commands;

public class Htmlizer : SyncCommand
{
    private Mode _mode;

    public Htmlizer WithMode(Mode mode)
    {
        _mode = mode;
        return this;
    }

    protected override void Run()
    {
        var message = Message.ReplyToMessage ?? Message;

        var text = _mode is Mode.ToHtml ? HtmlText.Escape(message.ToHtml()) : message.GetTextOrCaption();
        if (text is null)
        {
            Bot.SendSticker(Chat, InputFile.FromFileId(LOL.PickAny()));
        }
        else
        {
            if (message == Message) text = text.SplitN(2)[1];

            Bot.SendMessage(Chat, _mode is Mode.ToHtml ? $"<pre>{text}</pre>" : text);
            Log($"{Title} >> {(_mode is Mode.ToHtml ? "<HTML/>" : "<TEXT/>")}");
        }
    }

    private readonly string[] LOL =
    [
        "CAACAgIAAxkBAAEEGlNnQh3DjCdy5y3TMILTAj9p5hIhggACmAwAAo3W-Enqi3-VH6it3TYE",
        "CAACAgIAAxkBAAEEGlFnQh2_UarJ_Fee3KkUM_8YAbScIwACNQ8AArUD8EmgLRUZVgABwr02BA",
        "CAACAgIAAxkBAAEEGk1nQh2N6ufzEV3b4QbDqeDc_Q6d5QAC5UUAAmzf0EklgSKp8ldSBDYE",
        "CAACAgIAAxkBAAEEGklnQhtaAvkAAbPIrwABafI6yHb6Y-2-AAJWWwACp7ipS56h-jkMd3XXNgQ",
        "CAACAgIAAxkBAAEEGjBnQhpxnO1KipgWm0rHf-nV2aBaKwACMTcAAhPGQEl9eGSMb7nH2zYE"
    ];

    public enum Mode
    {
        ToHtml,
        FromHtml
    }
}