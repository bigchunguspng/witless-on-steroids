using System.Threading.Tasks;
using MediaToolkit.Core;
using MediaToolkit.Tasks;

namespace Witlesss.MediaTools
{
    public abstract class F_Base : FfMpegTaskBase<string>
    {
        protected readonly string Output;

        protected F_Base(string output) => Output = output;
        
        public override async Task<string> ExecuteCommandAsync(IFfProcess ffProcess)
        {
            await ffProcess.Task;
            return Output;
        }
    }

    public abstract class F_SimpleTask : F_Base
    {
        protected readonly string Input;

        protected F_SimpleTask(string input, string output) : base(output) => Input = input;
    }
}