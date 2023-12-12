using System;
using System.IO;
using FFMpegCore;

namespace Witlesss.MediaTools
{
    public abstract class F_Action
    {
        protected Action<FFMpegArgumentOptions> Action;

        protected F_Action ApplyEffects(Action<FFMpegArgumentOptions> action)
        {
            Action = action;
            return this;
        }


        public string OutputAs(string path) => Cook(path);

        public string Output(string suffix, string extension = ".mp4")
        {
            return Cook(NameSource.RemoveExtension() + suffix + extension);
        }

        public string Output_WEBM_safe(string suffix)
        {
            var extension = Path.GetExtension(NameSource);
            if (extension == ".webm") extension = ".mp4";
            return Output(suffix, extension);
        }


        protected abstract string NameSource { get; }
        protected abstract string Cook(string output);


        protected static void Run(FFMpegArgumentProcessor processor)
        {
            LogArguments(processor);
            processor.ProcessSynchronously();
        }

        private static void LogArguments(FFMpegArgumentProcessor a)
        {
            _args = a.Arguments;
#if DEBUG
            Log("[FFMPEG] >> " + FFMpegCommand, ConsoleColor.DarkYellow);
#endif
        }

        private static string _args;
        public  static string FFMpegCommand => _args is not null ? $"ffmpeg {_args}" : "Live FFMpeg reaction:";


        public static VideoStream GetVideoStream(string path) => FFProbe.Analyse(path).PrimaryVideoStream;
    }
}