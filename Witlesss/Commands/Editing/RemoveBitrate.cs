using System;

namespace Witlesss.Commands.Editing
{
    public class RemoveBitrate : AudioVideoCommand
    {
        public override void Run()
        {
            if (NothingToProcess()) return;

            var value = 15;
            if (Text.HasIntArgument(out int b)) value = Math.Clamp(b, 0, 21);

            Bot.Download(FileID, Chat, out string path, out var type);

            string result = Memes.RemoveBitrate(path, value + 30); // 30 - 51
            SendResult(result, type, VideoFilename, AudioFilename);
            Log($"{Title} >> DAMN [*]");

            string AudioFilename() => SongNameOr($"Damn, {Sender}.mp3");
            string VideoFilename() => $"piece_fap_club-{value}.mp4";
        }
    }
}