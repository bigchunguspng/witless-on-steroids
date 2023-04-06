﻿using System;
using System.IO;
using FFMpegCore;
using FFO = FFMpegCore.FFMpegArgumentOptions;
using FAP = FFMpegCore.FFMpegArgumentProcessor;

namespace Witlesss.MediaTools // ReSharper disable RedundantAssignment InconsistentNaming
{
    public abstract class F_SingleInput_Base
    {
        protected readonly string _input;

        protected F_SingleInput_Base(string input) => _input = input;

        public static string SetOutName_WEBM_safe(string path, string suffix)
        {
            string extension = Path.GetExtension(path);
            if (extension == ".webm") extension = ".mp4";
            return SetOutName(path, suffix, extension);
        }
        public static string SetOutName(string path, string suffix, string extension)
        {
            return RemoveExtension(path) + suffix + extension;
        }

        public static VideoStream GetVideoStream(string path) => FFProbe.Analyse(path).PrimaryVideoStream;

        /// <summary> Gets media info + adds fixes to the options </summary>
        protected MediaInfo MediaInfoWithFixing(ref FFO o)
        {
            var i = MediaInfo();
            o = AddFixes(o, i);
            return i;
        }
        protected MediaInfo MediaInfo()
        {
            var info = FFProbe.Analyse(_input);
            var v = info.PrimaryVideoStream;
            var a = info.PrimaryAudioStream;
            var audio = a is { };
            var video = v is { AvgFrameRate: not double.NaN };

            return new MediaInfo(info, audio, video, v);
        }
        protected FFO AddFixes(FFO o, MediaInfo i)
        {
            if (i.video) o = o.FixWebmSize(i.v);
            if (i.audio) o = o.FixSongArt(i.info);

            return o;
        }

        protected string Cook(string output, Action<FFO> args) // waltuh
        {
            var processor = GetFFMpegAP(_input, output, args);
            Run(processor);
            
            return output;
        }

        public static void Run(FAP processor)
        {
            LogArguments(processor);
            processor.ProcessSynchronously();
        }

        private static FAP GetFFMpegAP(string input, string output, Action<FFO> action)
        {
            return FFMpegArguments.FromFileInput(input).OutputToFile(output, addArguments: action);
        }

        private static void LogArguments(FAP a) => _args = a.Arguments;

        private static string _args;
        public  static string FFMpegCommand => _args is { } ? $"ffmpeg {_args}" : "*А НЕТУ!!!*";
    }

    public record MediaInfo(IMediaAnalysis info, bool audio, bool video, VideoStream v);
}