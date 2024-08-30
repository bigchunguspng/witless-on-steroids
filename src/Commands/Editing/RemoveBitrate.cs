using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Witlesss.Commands.Editing
{
    public class RemoveBitrate : AudioVideoPhotoCommand
    {
        private int _value;

        protected override async Task Execute()
        {
            _value = Context.HasIntArgument(out var x) ? Math.Clamp(x, 0, 21) : 21;

            var path = await Bot.Download(FileID, Chat, Ext);

            var result = Type switch // image: 1 - 22   video: 30 - 51
            {
                MediaType.Photo => CompressImage(path),
                MediaType.Stick => ImageSaver.SaveImageWebp(Image.Load<Rgba32>(path), GetOutPath(path), 22 - _value),
                _               => await path.UseFFMpeg().Compress(_value + 30).Out("-DAMN", Ext)
            };

            SendResult(result);
            Log($"{Title} >> DAMN [*]");
        }

        private string CompressImage(string path) // todo compress stickers as webp
        {
            var output = GetOutPath(path);
            var exe = "magick";
            var args = $"\"{path}\" -compress JPEG -quality {22 - _value} \"{output}\"";
            SystemHelpers.StartReadableProcess(exe, args).WaitForExit();
            return output;
        }

        private string GetOutPath(string path)
        {
            var dir  = Path.GetDirectoryName           (path);
            var name = Path.GetFileNameWithoutExtension(path);
            return UniquePath(dir, name + "-DAMN.jpg");
        }

        protected override string AudioFileName => SongNameOr($"Damn, {Sender}.mp3");
        protected override string VideoFileName => $"piece_fap_club-{_value}.mp4";
    }
}