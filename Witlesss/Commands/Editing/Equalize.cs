using System.Linq;

namespace Witlesss.Commands.Editing
{
    public class Equalize : FileEditingCommand
    {
        // /eq [frequency, Hz] [gain, dB] [width, Hz]
        protected override void Execute()
        {
            if (Text.Contains(' '))
            {
                var args = Text.Split(' ').Skip(1).Take(3).ToArray();

                var f = double.TryParse(                  args[0],      out var v1) ? v1 : 100;
                var g = double.TryParse(args.Length > 1 ? args[1] : "", out var v2) ? v2 : 10;
                var w = double.TryParse(args.Length > 2 ? args[2] : "", out var v3) ? v3 : 2000;

                Bot.Download(FileID, Chat, out var path, out var type);

                SendResult(Memes.EQ(path, new[] { f, g, w }), type);
                Log($"{Title} >> EQ [{f} Hz, {g} dB, {w} Hz]");
            }
            else
                Bot.SendMessage(Chat, EQ_MANUAL);
        }

        protected override string AudioFileName => SongNameOr($"Bassboosted by {Sender}.mp3");
        protected override string VideoFileName => $"piece_fap_club ft. DJ {Sender}.mp4";
    }
}