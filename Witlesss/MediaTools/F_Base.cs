using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using MediaToolkit.Core;
using MediaToolkit.Tasks;
using MediaToolkit.Util;

namespace Witlesss.MediaTools
{
    public abstract class F_Base : FfMpegTaskBase<string>
    {
        private static readonly List<string> Command = new(12);

        protected string Output;

        protected F_Base(string output)
        {
            Command.Clear();
            Output = output;
        }

        public override IList<string> CreateArguments()
        {
            Command.Add(Output);
            return Command;
        }

        public override async Task<string> ExecuteCommandAsync(IFfProcess ffProcess)
        {
            await ffProcess.Task;
            return Output;
        }

        protected void AddOptions (params string[] args) => args.ForEach(arg => Command.Add(arg));
        protected void AddInput   (string input) => AddOptions("-i", input);
        protected void AddSize    (Size s)       => AddOptions("-s", $"{s.Width}x{s.Height}");
        protected void AddWhen    (bool b, params string[] args) { if (b) AddOptions(args); }
        protected void AddSizeFix (string input, MediaType type)
        {
            if (type == MediaType.Video && Memes.ToMP4(input, ref Output, out var s)) AddSize(s);
        }
    }
}