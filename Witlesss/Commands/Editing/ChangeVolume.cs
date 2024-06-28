using System.Threading.Tasks;
using Witlesss.MediaTools;

namespace Witlesss.Commands.Editing
{
    public class ChangeVolume : FileEditingCommand
    {
        private string _arg;

        protected override async Task Execute()
        {
            if (Args is null)
            {
                Bot.SendMessage(Chat, VOLUME_MANUAL);
            }
            else
            {
                _arg = Args.Split(' ', 2)[0];

                var (path, type) = await Bot.Download(FileID, Chat);

                SendResult(await FFMpegXD.ChangeVolume(path, _arg), type);
                Log($"{Title} >> VOLUME [{_arg}]");
            }
        }

        protected override string AudioFileName => SongNameOr($"{Sender} Sound Effect.mp3");
        protected override string VideoFileName => _arg.Length < 8 ? $"VOLUME-{ValidFileName(_arg)}.mp4" : "VERY-LOUD-ICE-CREAM.mp4";
    }
}