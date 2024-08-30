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


        public Task<string> OutAs(string path) => Cook(path);

        public Task<string> Out(string suffix, string extension = ".mp4")
        {
            if (extension == ".webm") extension = ".mp4";
            return Cook(GetOutputName(suffix, extension));
        }

        public string GetOutputName(string suffix, string extension)
        {
            return NameSource.RemoveExtension() + suffix + extension;
        }


        protected abstract string NameSource { get; }
        protected abstract Task<string> Cook(string output);


        protected static Task Run(FFMpegArgumentProcessor processor)
        {
            LogArguments(processor);
            return processor.ProcessAsynchronously();
        }

        private static void LogArguments(FFMpegArgumentProcessor a)
        {
            _args = a.Arguments;
#if DEBUG
            Log("[FFMPEG] >> " + FFMpegCommand, ConsoleColor.DarkYellow);
#endif
        }

        private static string? _args;
        public  static string FFMpegCommand => _args is not null ? $"ffmpeg {_args}" : "Live FFMpeg reaction:";


        public static VideoStream? GetVideoStream(string path) => FFProbe.Analyse(path).PrimaryVideoStream;
    }
}