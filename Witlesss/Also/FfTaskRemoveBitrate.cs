using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaToolkit.Core;
using MediaToolkit.Tasks;
using static Witlesss.Also.Extension;

namespace Witlesss.Also
{
    public class FfTaskRemoveBitrate : FfMpegTaskBase<int>
    {
        private readonly string _inputFilePath;
        private readonly string _outputFilePath;
        private readonly string _bitrate;
        private readonly bool _video;

        public FfTaskRemoveBitrate(string inputFilePath, out string outputFilePath, int bitrate)
        {
            _inputFilePath = inputFilePath;
            string extension = GetFileExtension(inputFilePath);
            _video = extension == ".mp4";
            if (!_video) extension = ".mp3";
            _outputFilePath = inputFilePath.Remove(inputFilePath.LastIndexOf('.')) + "-L" + extension;
            _bitrate = bitrate.ToString();
            outputFilePath = _outputFilePath;
        }
        
        public override IList<string> CreateArguments()
        {
            var result = new List<string> {"-i", _inputFilePath, "-f", "mp3", "", "", "-b:a", "1k", _outputFilePath};
            if (_video)
            {
                result[3] = "mp4";
                result[4] = "-b:v";
                result[5] = $"{_bitrate}k";
            }
            return RemoveEmpties(result);
        }

        private IList<string> RemoveEmpties(IList<string> list) => list.Where(s => !string.IsNullOrEmpty(s)).ToList();

        public override async Task<int> ExecuteCommandAsync(IFfProcess ffProcess)
        {
            await ffProcess.Task;
            return 0;
        }
    }
}