using PF_Bot.Backrooms.Helpers;
using PF_Bot.Features.Edit.Core;
using PF_Bot.Routing.Commands;
using PF_Bot.Tools_Legacy.Technical;
using PF_Tools.Backrooms.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PF_Bot.Features.Edit.Filter
{
    public class RemoveBitrate : AudioVideoPhotoCommand
    {
        private int _value;

        protected override async Task Execute()
        {
            _value = Context.HasIntArgument(out var x) ? Math.Clamp(x, 0, 21) : 21;

            var path = await DownloadFile();

            var result = Type switch // image: 1 - 22   video: 30 - 51
            {
                MediaType.Photo => CompressImage(path),
                MediaType.Stick => ImageSaver.SaveImageWebp(Image.Load<Rgba32>(path), GetOutPath(path), 22 - _value),
                _               => await path.UseFFMpeg(Origin).Compress(_value + 30).Out("-DAMN", Ext)
            };

            SendResult(result);
            Log($"{Title} >> DAMN [*]");
        }

        private string CompressImage(string path)
        {
            var output = GetOutPath(path);
            var exe = "magick";
            var args = $"\"{path}\" -compress JPEG -quality {22 - _value} \"{output}\"";
            ProcessRunner.StartReadableProcess(exe, args).WaitForExit();
            return output;
        }

        private string GetOutPath(string path)
        {
            var dir  = Path.GetDirectoryName           (path);
            var name = Path.GetFileNameWithoutExtension(path);
            return UniquePath(dir, name + "-DAMN.jpg");
        }

        protected override string AudioFileName => SongNameOr($"Damn, {Sender}.mp3");
        protected override string VideoFileName => $"piece_fap_bot-damn-{_value}.mp4";
    }
}