using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaToolkit.Core;
using MediaToolkit.Tasks;

namespace Witlesss.Also
{
    public class FfTaskRemoveBitrate : FfMpegTaskBase<int>
    {
        private readonly string _inputFilePath;
        private readonly string _outputFilePath;
        private readonly string _extension;
        private readonly string _bitrate;
        private readonly bool _video;


        public FfTaskRemoveBitrate(string inputFilePath, out string outputFilePath, int bitrate)
        {
            _inputFilePath = inputFilePath;
            _extension = inputFilePath.Substring(inputFilePath.LastIndexOf('.')); // .mp4
            _outputFilePath = inputFilePath.Remove(inputFilePath.LastIndexOf('.')) + "-L" + _extension;
            _video = _extension == ".mp4";
            _bitrate = bitrate.ToString();
            outputFilePath = _outputFilePath;
        }
        
        public override IList<string> CreateArguments()
        {
            var result = RemoveEmpties(new[]
            {
                // ffmpeg -i input.mp4 -f mp4 -b:v 40k -b:a 1k output.mp4
                "-i",
                _inputFilePath ?? "",
                "-f",
                _extension.Substring(1),
                _video ? "-b:v" : "",
                _video ? $"{_bitrate}k" : "",
                "-b:a",
                "1k",
                _outputFilePath ?? ""
            });
            //Logger.Log(string.Join(" ", result));
            return result;
        }

        private IList<string> RemoveEmpties(IList<string> list) => list.Where(s => !string.IsNullOrEmpty(s)).ToList();

        public override async Task<int> ExecuteCommandAsync(IFfProcess ffProcess)
        {
            await ffProcess.Task;
            return 0;
        }
    }
}