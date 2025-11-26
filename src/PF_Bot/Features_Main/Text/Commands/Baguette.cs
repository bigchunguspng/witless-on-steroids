using System.Text;
using PF_Bot.Routing.Messages.Commands;

namespace PF_Bot.Features_Main.Text.Commands;

public class Baguette : CommandHandlerBlocking
{
    protected override void Run()
    {
        var length    = Options.MatchNumber().ExtractGroup(0, int.Parse, 3);
        var greentext = Options.Contains("b");

        var text = BuildText(length, greentext);

        Bot.SendMessage(Origin, text, preview: true);
        Log($"{Title} >> {(greentext ? ">GREENTEXT >" : "BUGURT #@#")}{length}");
    }

    private string BuildText(int length, bool greentext)
    {
        var sign = greentext ? '>' : '@';

        var lines = new string[length];
        lines[0] = Args ?? GenerateLine();
        for (var i = 1; i < length; i++) lines[i] = GenerateLine();

        var sb = new StringBuilder();
        if (greentext)
        {
            foreach (var line in lines) sb.Append('>').AppendLine(line);

            if (Fortune.IsOneIn(3)) // append "summary"
            {
                _ = Fortune.IsOneIn(2)
                    ? sb
                        .Append('\n')
                        .Append(GenerateLine())
                    : sb
                        .Append(GenerateLine().InLetterCase(LetterCase.Sentence));
            }
        }
        else // #@#
        {
            var lastLineIndex = length - 1;
            for (var i = 0; i < length; i++)
            {
                sb.Append(lines[i].ToUpper());
                if (i < lastLineIndex) sb.Append("\n@\n");
            }
        }

        return sb.ToString();

        string GenerateLine() => Baka.Generate().Before(sign).Trim();
    }
}