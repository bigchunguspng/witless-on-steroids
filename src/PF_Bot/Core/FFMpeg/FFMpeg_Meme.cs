using PF_Bot.Features.Edit.Shared;
using PF_Bot.Features.Generate.Memes.Core;
using PF_Tools.FFMpeg;
using SixLabors.ImageSharp;

namespace PF_Bot.Core.FFMpeg;

public class FFMpeg_Meme(FFProbeResult probe, MemeFileRequest memeFileRequest, Tools_Legacy.FFMpeg.VideoMemeRequest request)
{
    private readonly string  _input = memeFileRequest.SourcePath;
    private readonly string _output = memeFileRequest.TargetPath;

    private readonly FFMpegArgs             _args = new();
    private readonly FFMpegOutputOptions _options = new();

    public FFMpegArgs Demo(Size image, Point point)
    {
        return AddDemotivatorFilter(image, point);
    }

    public FFMpegArgs D300(Size image, Point point, Size frame)
    {
        _options.Resize(frame);
        return AddDemotivatorFilter(image, point);
    }

    private FFMpegArgs AddDemotivatorFilter(Size s, Point p)
    {
        _args.Filter(GetFpsFixFilter());
        _args.Filter($"[0:v]scale={s.Width}:{s.Height}[vid]");
        _args.Filter($"[pic][vid]overlay={p.X}:{p.Y}:format=rgb");
        Press(request.Press);
        AddArguments();

        return _args;
    }

    public FFMpegArgs Meme(Size size)
    {
        _args.Filter($"[0:v]scale={size.Width}:{size.Height}[vid]");
        _args.Filter($"[vid][1:v]overlay=0:0:format=rgb");
        Press(request.Press);
        AddArguments();

        return _args;
    }

    public FFMpegArgs When(Size size, Rectangle crop, Point point)
    {
        _args.Filter(GetFpsFixFilter());
        _args.Filter($"[0:v]scale={size.Width}:{size.Height}[v0]");
        _args.Filter($"[v0]crop={crop.Width}:{crop.Height}:{crop.X}:{crop.Y}[vid]");
        _args.Filter($"[pic][vid]overlay={point.X}:{point.Y}:format=rgb");
        Press(request.Press);
        AddArguments();

        return _args;
    }

    private string GetFpsFixFilter
        () => $"[1:v]fps={GetFramerate().Format()}[pic]";

    private double GetFramerate()
    {
        var framerate = probe.GetVideoStream().AvgFramerate;
        return framerate is float.NaN ? 13 : Math.Max(framerate, 13); // magic moment
    }

    private void Press(float press)
    {
        if (press == 0) return;

        _args.FilterAppend($"scale=ceil((iw*{press})/2)*2:ceil((ih*{press})/2)*2");
        _args.FilterAppend($"scale=ceil((iw/{press})/2)*2:ceil((ih/{press})/2)*2");
    }

    private void AddArguments()
    {
        _args.Input(_input);
        _args.Input(request.Caption);
        _args.Out(_output, _options);

        Compress();
    }

    private void Compress()
    {
        _options.FixVideo_Playback();

        var factor = request.Quality;
        if (factor >  0) _options.Options(FFMpegOptions.Out_cv_libx264).Options($"-crf {factor}");
        if (factor > 23) _options.Options($"-b:a {154 - 3 * factor}k"); // todo relative bitrate
    }
}