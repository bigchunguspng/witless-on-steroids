﻿using System.Drawing;
using FFMpegCore.Arguments;

namespace Witlesss.MediaTools
{
    #region VIDEO

    public record SpeedArgument(double Speed) : IVideoFilterArgument
    {
        public string Key   => "setpts";
        public string Value => $"{(1 / Speed).Format()}*PTS";
    }

    public record FpsArgument(double FPS) : IVideoFilterArgument
    {
        public string Key   => "fps";
        public string Value => FPS.Format();
    }

    public record CropArgument(Rectangle Crop) : IVideoFilterArgument
    {
        public string Key   => "crop";
        public string Value => $"{Crop.Width}:{Crop.Height}:{Crop.X}:{Crop.Y}";
    }

    public record CropArgumentXD(string[] Crop) : IVideoFilterArgument
    {
        public string Key   => "crop";
        public string Value => string.Join(':', Crop);
    }

    public record ScaleArgumentXD(string[] Scale) : IVideoFilterArgument
    {
        public string Key   => "scale";
        public string Value => string.Join(':', Scale);
    }

    public record SampleRatioArgument(double Ratio) : IVideoFilterArgument
    {
        public string Key   => "setsar";
        public string Value => Ratio.Format();
    }

    public class ReverseArgument : IVideoFilterArgument
    {
        public string Key   => null!;
        public string Value => "reverse";
    }

    #endregion


    #region AUDIO

    public record AtempoArgument(double Atempo) : IAudioFilterArgument
    {
        public string Key   => "atempo";
        public string Value => Atempo.Format();
    }

    public class AreverseArgument : IAudioFilterArgument
    {
        public string Key   => null!;
        public string Value => "areverse";
    }

    public record VolumeArgument(string Volume) : IAudioFilterArgument
    {
        public string Key   => "volume";
        public string Value => $"'{Volume}':eval=frame";
    }

    public record EqualizeArgument(double[] Args) : IAudioFilterArgument
    {
        public string Key   => "equalizer";
        public string Value => $"f={Args[0]}:g={Args[1]}:t=h:width={Args[2]}";
    }

    #endregion
}