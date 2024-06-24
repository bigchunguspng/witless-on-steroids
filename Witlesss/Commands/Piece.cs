using System.IO;
using System.Text.RegularExpressions;
using Witlesss.Generation;

namespace Witlesss.Commands
{
    /// <summary>
    /// Post Id Easy Channel Exporter™? Maybe.
    /// </summary>
    public class Piece : SyncCommand
    {
        private readonly Regex _args = new(@"t.me\/[a-z0-9_]{5,32}\/\d+\s\S+");
        private readonly Regex _urls = new(@"t.me\/[a-z0-9_]{5,32}\/");

        private string _url = default!, _name = default!;
        private int _latest;
        
        protected override void Run()
        {
            if (WrongSyntax()) return;

            var cp = new Copypaster();
            for (int i = 1; i <= _latest; i++) cp.Eat(_url + i, out _);

            var path = Move.UniqueExtraDBsPath(_name);
            new FileIO<GenerationPack>(path).SaveData(cp.DB);

            Bot.SendMessage(Chat, string.Format(PIECE_RESPONSE, Path.GetFileNameWithoutExtension(path)));
            Log($"{Title} >> THE ONE PIECE IS REAL!!!");
        }

        private bool WrongSyntax()
        {
            if (Text == null) return true;

            var ok = _args.IsMatch(Text);
            if (ok)
            {
                _url = _urls.Match(Text).Value;
                var s = Text.Split(' ', 3);
                _name = s[^1].Replace(' ', '_');
                _latest = int.Parse(s[1].Split('/')[^1]);
            }
            else
                Bot.SendMessage(Chat, PIECE_MANUAL);

            return !ok;
        }
    }
}