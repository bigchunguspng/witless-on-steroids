using PF_Bot.Features_Main.Edit.Core;
using PF_Bot.Features_Main.Edit.Helpers;
using PF_Bot.Routing.Commands;
using PF_Tools.FFMpeg;
using PF_Tools.ProcessRunning;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PF_Bot.Features_Main.Edit.Commands.Filter
{
    public class Damn : FileEditor_AudioVideoPhoto
    {
        private int    _value;
        private Quality ValueAsQuality => new ((byte)(100 - _value * 100 / 21F));

        protected override async Task Execute()
        {
            _value = Args.TryParseAsInt(out var x) ? Math.Clamp(x, 0, 21) : 21;

            var input = await GetFile();
            var output = input.GetOutputFilePath("DAMN", Type is MediaType.Photo ? "jpg" : Ext);

            var compressTask = Type switch
            {
                MediaType.Photo => CompressImage     (input, output),
                MediaType.Stick => CompressStick     (input, output),
                _               => CompressVideoAudio(input, output),
            };

            await compressTask;

            SendResult(output);
            Log($"{Title} >> DAMN [*]");
        }

        private async Task CompressStick(FilePath input, FilePath output)
        {
            using var image = Image.Load<Rgba32>(input);
            await ImageSaver.SaveImageWebp(image, output, GetJpegQuality());
        }

        private async Task CompressVideoAudio(FilePath input, FilePath output)
        {
            var (_, probe, options) = await input.InitEditing("DAMN", Ext);

            var video = probe.HasVideo && probe.GetVideoStream().IsLikelyImage.Janai();
            if (video)
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
                if (video.Janai()) options.Options("-f mp3");
            }

            options.Fix_AudioVideo(probe);

            await FFMpeg.Command(input, output, options).FFMpeg_Run();
        }

        private async Task CompressImage(string input, FilePath output)
        {
            var quality = GetJpegQuality().GetImageQuality();
            var args = $"\"{input}\" -compress JPEG -quality {quality} \"{output}\"";
            
            var processResult = await ProcessRunner.Run(MAGICK, args);
            if (processResult.Failure) throw new ProcessException(MAGICK, processResult);
        }

        // QUALITY
        // value = [0..21]

        private int     GetVideoCRF   () => _value + 30;         //  30 - 51     crf: 0 lossless - 51 lowest quality
        private Quality GetJpegQuality() => (byte)(22 - _value); //  22 -  1     quality: 1 - 100

        protected override string AudioFileName => SongNameOr($"Damn, {Sender}.mp3");
        protected override string VideoFileName => $"piece_fap_bot-damn-{_value}.mp4";
    }
}