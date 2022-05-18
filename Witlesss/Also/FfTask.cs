using System.Threading.Tasks;
using MediaToolkit.Core;
using MediaToolkit.Tasks;

namespace Witlesss.Also
{
    public abstract class FfTask : FfMpegTaskBase<int>
    {
        public override async Task<int> ExecuteCommandAsync(IFfProcess ffProcess)
        {
            await ffProcess.Task;
            return 0;
        }
    }
}