using System;
using System.IO;
using FFMpegCore;

namespace Witlesss.MediaTools // ReSharper disable RedundantAssignment
{
    public abstract class F_SingleInput_Base
    {
        protected readonly string _input;

        protected F_SingleInput_Base(string input) => _input = input;

        protected static FFMpegArgumentProcessor GetFFMpegAP(string input, string output, Action<FFMpegArgumentOptions> action)
        {
            return FFMpegArguments.FromFileInput(input).OutputToFile(output, addArguments: action);
        }
        protected static string SetOutName_WEBM_safe(string path, string suffix)
        {
            string extension = Path.GetExtension(path);
            if (extension == ".webm") extension = ".mp4";
            return SetOutName(path, suffix, extension);
        }

        public static VideoStream GetVideoStream(string path) => FFProbe.Analyse(path).PrimaryVideoStream;
        protected (IMediaAnalysis info, bool audio, bool video, VideoStream v) MediaInfo()
        {
            var info = FFProbe.Analyse(_input);
            var v = info.PrimaryVideoStream;
            var a = info.PrimaryAudioStream;
            var audio = a is { };
            var video = v is { AvgFrameRate: not double.NaN };

            return (info, audio, video, v);
        }
        protected string Cook(string output, Action<FFMpegArgumentOptions> args) // waltuh
        {
            var processor = GetFFMpegAP(_input, output, args);
            Run(processor);
            
            return output;
        }
        public static void Run(FFMpegArgumentProcessor processor)
        {
            LogArguments(processor);
            processor.ProcessSynchronously();
        }

        public static string SetOutName(string path, string suffix, string extension)
        {
            return RemoveExtension(path) + suffix + extension;
        }
        
        protected static void LogArguments(FFMpegArgumentProcessor a) => Log(string.Join(' ', a.Arguments));
    }
}