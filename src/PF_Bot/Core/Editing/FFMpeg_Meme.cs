using PF_Bot.Core.Memes.Shared;
using PF_Bot.Features.Edit.Shared;
using PF_Tools.FFMpeg;
using SixLabors.ImageSharp;

namespace PF_Bot.Core.Editing;

public class FFMpeg_Meme(FFProbeResult probe, MemeFileRequest request, FilePath caption)
{
    private readonly string  _input = request.SourcePath;
    private readonly string _output = request.TargetPath;

    private readonly FFMpegArgs             _args = new();
    private readonly FFMpegOutputOptions _options = new();

    /// Caption on top of image, both same size.
    public FFMpegArgs Meme(Size s)
    {
        _args.Filter($"[0:v]scale={s.Width}:{s.Height}[vid]");
        _args.Filter("[vid][1:v]overlay=0:0:format=rgb");
        return AddArguments_And_Press();
    }

    /// Image on top of frame (caption), different size.
    public FFMpegArgs Demotivator(Size s, Point p)
    {
        _args.Filter(GetFpsFixFilter());
        _args.Filter($"[0:v]scale={s.Width}:{s.Height}[vid]");
        _args.Filter($"[pic][vid]overlay={p.X}:{p.Y}:format=rgb");
        return AddArguments_And_Press();
    }

    /// Image on top of frame (caption), different size, source can be cropped.
    public FFMpegArgs Top(Size s, Rectangle c, Point p)
    {
        _args.Filter(GetFpsFixFilter());
        _args.Filter($"[0:v]scale={s.Width}:{s.Height}");
        _args.FilterAppend($"crop={c.Width}:{c.Height}:{c.X}:{c.Y}[vid]");
        _args.Filter($"[pic][vid]overlay={p.X}:{p.Y}:format=rgb");
        return AddArguments_And_Press();
    }

    private string GetFpsFixFilter
        () => $"[1:v]fps={GetFramerate().Format()}[pic]";

    private double GetFramerate()
    {
        var framerate = probe.GetVideoStream().AvgFramerate;
        return framerate is float.NaN ? 13 : Math.Max(framerate, 13); // magic moment
    }

    private FFMpegArgs AddArguments_And_Press()
    {
        AddArguments();
        HydraulicPress(request.Press);
        AV_Compression(request.Quality);

        return _args;
    }

    private void AddArguments()
    {
        _args.Input(_input);
        _args.Input(caption);
        _args.Out(_output, _options);
    }

    private void HydraulicPress(float press)
    {
        if (press == 0) return;

        _args.FilterAppend($"scale=ceil((iw*{press})/2)*2:ceil((ih*{press})/2)*2");
        _args.FilterAppend($"scale=ceil((iw/{press})/2)*2:ceil((ih/{press})/2)*2");
    }

    private void AV_Compression(Quality quality)
    {
        _options
            .FixVideo_Playback()
            .SetCRF(quality.GetCRF());

        if (probe.HasAudio)
        {
            var audio = probe.GetAudioStream();
            var bitrate = quality.GetAudioBitrate_kbps(audio.Bitrate);
            _options.Options($"-b:a {bitrate}k");
        }
    }
}