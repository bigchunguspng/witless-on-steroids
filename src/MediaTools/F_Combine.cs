﻿using System.Text;
using SixLabors.ImageSharp;
using Witlesss.Commands.Meme.Core;
using Drawer = Witlesss.Memes.DemotivatorDrawer;
using FFO = FFMpegCore.FFMpegArgumentOptions;

namespace Witlesss.MediaTools;

public record VideoMemeRequest(int Quality, string Caption)
{
    public static VideoMemeRequest From
        (MemeFileRequest request, Image caption)
        => new(request.GetCRF(), ImageSaver.SaveImageTemp(caption));

    public static VideoMemeRequest From
        (MemeFileRequest request, string captionAsFile)
        => new(request.GetCRF(), captionAsFile);
}

public partial class F_Process
{
    private void AddInput(string path) => Arguments.AddFileInput(path);

    // EFFECTS

    // -i video -i image -filter_complex "[0:v]scale=620:420[vid];[vid][1:v]overlay=0:0"
    public F_Process Meme(VideoMemeRequest request, Size size) => ApplyEffects(o =>
    {
        var filter = $"[0:v]scale={size.Width}:{size.Height}[vid];[vid][1:v]overlay=0:0:format=rgb";
        BuildAndCompress(o, request, filter);
    });

    public F_Process When(VideoMemeRequest request, Size size, Rectangle crop, Point point, bool blur) => ApplyEffects(o =>
    {
        var filter = new StringBuilder();
        filter.Append(FixPicFps());
        filter.Append($"[0:v]scale={size.Width}:{size.Height}[v0];");
        filter.Append($"[v0]crop={crop.Width}:{crop.Height}:{crop.X}:{crop.Y}[vid];");
        filter.Append($"[pic][vid]overlay={point.X}:{point.Y}:format=rgb");
        if (blur) filter.Append(",gblur=1:1");
        BuildAndCompress(o, request, filter.ToString());
    });

    // -i video -i image -filter_complex "[0:v]scale=620:530[vid];[1:v][vid]overlay=50:50"
    public F_Process Demo(VideoMemeRequest request, Drawer drawer) => ApplyEffects(o =>
    {
        ArgsDemo(o, request, drawer.ImagePlacement.Size, drawer.ImagePlacement.Location);
    });

    public F_Process D300(VideoMemeRequest request, Size image, Point point, Size frame) => ApplyEffects(o =>
    {
        ArgsDemo(o, request, image, point);
        o.Resize(frame.Ok());
    });


    private void ArgsDemo(FFO o, VideoMemeRequest request, Size s, Point p)
    {
        var filter = $"{FixPicFps()}[0:v]scale={s.Width}:{s.Height}[vid];[pic][vid]overlay={p.X}:{p.Y}:format=rgb";
        BuildAndCompress(o, request, filter);
    }

    private string FixPicFps() => $"[1:v]fps={GetFramerate().Format()}[pic];";

    private void BuildAndCompress(FFO o, VideoMemeRequest request, string filterComplex)
    {
        AddInput(request.Caption);
        o.WithComplexFilter(filterComplex).FixPlayback();

        var factor = request.Quality;
        if (factor >  0) o.WithCompression(factor);
        if (factor > 23) o.WithAudioBitrate(154 - 3 * factor);
    }


    public Task<string> AddTrackMetadata(string art, string? artist, string title)
    {
        AddInput(art);
        var name = $"{(artist is null ? "" : $"{artist} - ")}{title}";
        var path = $"{Path.GetDirectoryName(Input)}/{name.ValidFileName('#')}.mp3";
        return ApplyEffects(o => MetadataArgs(o, artist, title)).OutAs(path);
    }

    private static void MetadataArgs(FFO o, string? artist, string title)
    {
        var sb = new StringBuilder();
        sb.Append("-map 0:0 -map 1:0 -c copy -id3v2_version 3 ");
        sb.Append("-metadata:s:v title=\"Album cover\" ");
        sb.Append("-metadata:s:v comment=\"Cover (front)\" ");
        if (artist is not null) sb.Append("-metadata artist=\"").Append(artist).Append("\" ");
        sb.Append                        ("-metadata title=\"" ).Append(title ).Append("\" ");

        o.WithCustomArgument(sb.ToString());
    }
}