using System;
using System.Threading.Tasks;

namespace Witlesss.Commands.Editing
{
    public class RemoveBitrate : FileEditingCommand
    {
        private int _value;

        protected override async Task Execute()
        {
            _value = Context.HasIntArgument(out var x) ? Math.Clamp(x, 0, 21) : 15;

            var (path, type) = await Bot.Download(FileID, Chat);

            var result = await Memes.RemoveBitrate(path, _value + 30); // 30 - 51
            SendResult(result, type);
            Log($"{Title} >> DAMN [*]");
        }
        
        protected override string AudioFileName => SongNameOr($"Damn, {Sender}.mp3");
        protected override string VideoFileName => $"piece_fap_club-{_value}.mp4";
    }
}