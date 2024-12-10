namespace Witlesss.Commands.Generation
{
    public class Bouhourt : WitlessSyncCommand
    {
        private readonly Regex _length = new(@"\d+");

        protected override void Run()
        {
            var length = _length.ExtractGroup(0, Command!, int.Parse, 3);
            var start = Args;

            var lines = new List<string>(length) { start ?? GenerateLine() };
            for (var i = 1; i < length; i++) lines.Add(GenerateLine());

            var result = string.Join("\n@\n", lines.Where(x => x != "")).Replace(" @ ", "\n@\n").ToUpper();
            Bot.SendMessage(Origin, result, preview: true);
            Log($"{Title} >> BUGURT #@#{length}");

            string GenerateLine() => Baka.Generate().Trim('@').TrimStart();
        }
    }
}