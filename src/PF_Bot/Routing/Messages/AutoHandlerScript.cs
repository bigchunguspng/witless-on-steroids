namespace PF_Bot.Routing.Messages;

public record WeightedOption(int Weight, string Content);

public class AutoHandlerScript
{
    private readonly Dictionary<string, List<WeightedOption>> Macros = new(); // name -> possible values
    private readonly Dictionary<char,   string>            Templates = new(); // file type -> input template

    // GET (RENDER)

    public IEnumerable<char> SupportedFileTypes => Templates.Keys;

    public string? GenerateInput(char type)
    {
        if (Templates.TryGetValue_Failed(type, out var template))
            return null;

        foreach (var (name, options) in Macros) // expand macros
        {
            var macroUsage = $"[{name}]";
            if (template.Contains(macroUsage).Janai())
                continue;

            var replacement = PickRandom(options);
            template = template.Replace(macroUsage, replacement);
        }

        return template;
    }

    private string PickRandom(List<WeightedOption> macros)
    {
        var totalWeight = macros.Sum(x => x.Weight);
        var r = Random.Shared.Next(totalWeight);
        foreach (var macro in macros)
        {
            if (macro.Weight > r)
            {
                return macro.Content;
            }

            r -= macro.Weight;
        }

        throw new UnexpectedException("ERROR IN WEIGHTED MACRO RANDOM SELECION");
    }

    // CREATE (PARSE)

    private static readonly Regex
        _r_wm      = new(    @"(?:(\d+)\s)?([\S\s]+)", RegexOptions.Compiled),
        _r_handler = new(@"([pvagusd]+):\s*([\S\s]+)", RegexOptions.Compiled);

    public static AutoHandlerScript Create(string raw)
    {
        var script = new AutoHandlerScript();

        // /set a   let m 50 meme, 25 top, 25 dp;   psg: [m]^^*3upim
        var statements = raw
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim());

        foreach (var statement in statements)
        {
            var let = statement.StartsWith("let", StringComparison.OrdinalIgnoreCase);
            if (let) // let m 50 meme, 25 top, 25 dp
            {
                var bits = statement.SplitN(3);
                var name = bits[1]; // m
                var body = bits[2]; // 50 meme, 25 top, 25 dp

                var options = body
                    .Split(", ", StringSplitOptions.RemoveEmptyEntries)
                    .Select(chunk =>
                    {
                        var match = _r_wm.Match(chunk);
                        var weight  = match.ExtractGroup(1, int.Parse, 1); // 50
                        var content = match.ExtractGroup(2, s => s, ""); // meme

                        return new WeightedOption(weight, content);
                    })
                    .ToList();

                script.Macros.Add(name, options);
            }
            else // psg: [m]^^*3upim
            {
                var match = _r_handler.Match(statement);
                var types    = match.ExtractGroup(1, s => s, ""); // psg
                var template = match.ExtractGroup(2, s => s, ""); // [m]^^*3upim
                foreach (var type in types)
                {
                    script.Templates.Add(type, template);
                }
            }
        }

        return script;
    }
}