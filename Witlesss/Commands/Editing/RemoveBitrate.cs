using System;

namespace Witlesss.Commands.Editing
{
    public class RemoveBitrate : FileEditingCommand
    {
        private int _value;

        public override void Run()
        {
            if (NothingToProcess()) return;

            _value = Text.HasIntArgument(out var x) ? Math.Clamp(x, 0, 21) : 15;

            Bot.Download(FileID, Chat, out var path, out var type);

            var result = Memes.RemoveBitrate(path, _value + 30); // 30 - 51
            SendResult(result, type);
            Log($"{Title} >> DAMN [*]");
        }
        
        protected override string AudioFileName => SongNameOr($"Damn, {Sender}.mp3");
        protected override string VideoFileName => $"piece_fap_club-{_value}.mp4";
    }
}