using System.Linq;
using System.Threading.Tasks;
using Witlesss.MediaTools;

namespace Witlesss.Commands.Editing
{
    public class Equalize : FileEditingCommand
    {
        // /eq [frequency, Hz] [gain, dB] [width, Hz]
        protected override async Task Execute()
        {
            if (Args is null)
            {
                Bot.SendMessage(Chat, EQ_MANUAL);
            }
            else
            {
                var args = Args.Split(' ').Take(3).ToArray();

                var f = double.TryParse(args[0], out var v1) ? v1 : 100;
                var g = double.TryParse(args.Length > 1 ? args[1] : "", out var v2) ? v2 : 10;
                var w = double.TryParse(args.Length > 2 ? args[2] : "", out var v3) ? v3 : 2000;

                var (path, type) = await Bot.Download(FileID, Chat);

                SendResult(await FFMpegXD.EQ(path, [f, g, w]), type);
                Log($"{Title} >> EQ [{f} Hz, {g} dB, {w} Hz]");
            }
        }

        protected override string AudioFileName => SongNameOr($"Bassboosted by {Sender}.mp3");
        protected override string VideoFileName => $"piece_fap_club ft. DJ {Sender}.mp4";
    }
}