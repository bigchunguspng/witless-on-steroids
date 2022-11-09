using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using MediaToolkit.Core;
using MediaToolkit.Tasks;

namespace Witlesss.MediaTools
{
    public abstract class F_Base : FfMpegTaskBase<string>
    {
        private readonly List<string> _command;

        protected string Output;

        protected F_Base(string output)
        {
            _command = new List<string>(12);
            Output = output;
        }

        public override IList<string> CreateArguments()
        {
            AddOptions(Output);
            return _command;
        }

        public override async Task<string> ExecuteCommandAsync(IFfProcess ffProcess)
        {
            await ffProcess.Task;
            return Output;
        }

        protected void AddInput(string input) => AddOptions("-i", input);
        protected void AddOptions(params string[] args) { foreach (var arg in args) _command.Add(arg); }
        protected void AddWhen(bool b, params string[] args) { if (b) AddOptions(args); }
        protected void AddSize(Size s) => AddOptions("-s", $"{s.Width}x{s.Height}");
    }

    public abstract class F_SimpleTask : F_Base
    {
        protected readonly string Input;

        protected F_SimpleTask(string input, string output) : base(output) => Input = input;

        protected void AddSizeFix(MediaType type)
        {
            if (type == MediaType.Video && Memes.ToMP4(Input, ref Output, out var s)) AddSize(s);
        }
    }
}