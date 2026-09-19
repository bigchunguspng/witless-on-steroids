namespace PF_Bot.Features_Main.Text.Commands;

public class GenerateByLastWord : GenerateByFirstWord
{
    protected override async Task Run()
    {
        string word = null!, ending = null!;
        var src = Args ?? Message.ReplyToMessage?.GetTextOrCaption();
        var byWord = src != null;
        if (byWord)
        {
            var lines = src!.Split('\n');
            var words = lines[0].Split();
                
            word = words.Length == 1 ? words[0] : string.Join(' ', words[..2]);
            word = word.ToLower();

            ending = src[word.Length..];
        }

        var up = Options.Contains("up");
        var repeats = _rgx_repeat.ExtractGroup(0, Options, int.Parse, 1);
        var texts = new string[repeats];
        for (var i = 0; i < repeats; i++)
        {
            var mode = up ? LetterCase.Upper : GetMode(src);
            texts[i] = byWord
                ? Baka.GenerateByLast(word).InLetterCase(mode) + ending
                : Baka.GenerateBackwards().InLetterCase(mode);
        }

        await Task.Run(() =>
        {
            foreach (var text in texts) Bot.SendMessage(Origin, text, preview: true);
        });

        LogXD(Title, repeats, "FUNNY BY LAST WORD");
    }
}