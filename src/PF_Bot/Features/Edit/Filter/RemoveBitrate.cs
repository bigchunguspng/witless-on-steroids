using PF_Bot.Backrooms.Helpers;
using PF_Bot.Features.Edit.Core;
using PF_Bot.Features.Edit.Shared;
using PF_Bot.Routing.Commands;
using PF_Bot.Tools_Legacy.Technical;
using PF_Tools.FFMpeg;
using PF_Tools.ProcessRunning;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PF_Bot.Features.Edit.Filter
{
    public class RemoveBitrate : AudioVideoPhotoCommand
    {
        private int    _value;
        private Quality ValueAsQuality => new ((byte)(100 - _value * 100 / 21F));

        protected override async Task Execute()
        {
            _value = Context.HasIntArgument(out var x) ? Math.Clamp(x, 0, 21) : 21;

            var input = await DownloadFile();

            var result = Type switch
            {
                MediaType.Photo => await CompressImage(input),
                MediaType.Stick => ImageSaver.SaveImageWebp(Image.Load<Rgba32>(input), GetOutPath(input), GetJpegQuality()),
                _               => await CompressVideoAudio(input),
            };

            SendResult(result);
            Log($"{Title} >> DAMN [*]");
        }

        private async Task<string> CompressVideoAudio(FilePath input)
        {
            var (output, probe, options) = await input.InitEditing("DAMN", Ext);

            if (probe.HasVideo)
            {
                options
                    .SetCRF(GetVideoCRF())
                    .MP4_EnsureValidSize(probe.GetVideoStream());
            }

            if (probe.HasAudio)
            {
                var audio = probe.GetAudioStream();
                var bitrate = ValueAsQuality.GetAudioBitrate_kbps(audio.Bitrate);
                options.Options($"-b:a {bitrate}k");
                if (probe.HasVideo == false) options.Options("-f mp3");
            }

            options.Fix_AudioVideo(probe);

            await FFMpeg.Command(input, output, options).FFMpeg_Run();

            return output;
        }

        private async Task<string> CompressImage(string input)
        {
            var output = GetOutPath(input);
            var args = $"\"{input}\" -compress JPEG -quality {GetJpegQuality()} \"{output}\"";
            
            var processResult = await ProcessRunner.Run(MAGICK, args);
            if (processResult.Failure) throw new ProcessException(MAGICK, processResult);

            return output;
        }

        private string GetOutPath
            (FilePath input) => input.GetOutputFilePath("DAMN", "jpg");

        // QUALITY
        // value = [0..21]

        private int GetVideoCRF   () => _value + 30;        //  30 - 51     crf: 0 lossless - 51 lowest quality
        private int GetJpegQuality() => 22 - _value;        //  22 -  1     quality: 1 - 100

        protected override string AudioFileName => SongNameOr($"Damn, {Sender}.mp3");
        protected override string VideoFileName => $"piece_fap_bot-damn-{_value}.mp4";
    }
}