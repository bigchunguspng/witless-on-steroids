using System;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Witlesss.Backrooms.Helpers;
using Witlesss.MediaTools;

namespace Witlesss.Commands.Editing
{
    public class RemoveBitrate : FileEditingCommand
    {
        private int _value;

        protected override async Task Execute()
        {
            _value = Context.HasIntArgument(out var x) ? Math.Clamp(x, 0, 21) : 15;

            var (path, type) = await Bot.Download(FileID, Chat);

            var result = IsPhoto
                ? CompressImage(path)
                : await FFMpegXD.RemoveBitrate(path, _value + 30); // 30 - 51

            SendResult(result, type);
            Log($"{Title} >> DAMN [*]");
        }

        private string CompressImage(string path)
        {
            var directory = Path.GetDirectoryName(path);
            var name = Path.GetFileNameWithoutExtension(path);
            var output = UniquePath(directory, name + "-DAMN.jpg");
            var exe = "imagemagick";
            var args = $"convert \"{path}\" -compress JPEG -quality {22 - _value} \"{output}\""; // 1 - 22
            SystemHelpers.StartedReadableProcess(exe, args).WaitForExit();
            return output;
        }

        protected override string AudioFileName => SongNameOr($"Damn, {Sender}.mp3");
        protected override string VideoFileName => $"piece_fap_club-{_value}.mp4";

        protected override bool MessageContainsFile(Message m)
        {
            return GetVideoFileID(m) || GetAudioFileID(m) || GetPhotoFileID(m);
        }
    }
}