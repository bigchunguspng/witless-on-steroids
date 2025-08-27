using System.Text;

namespace PF_Tools.FFMpeg;

public class FFMpegInputOptions
{
    private readonly List<string> _options = [];

    public FFMpegInputOptions Options(string options)
    {
        _options.Add(options);
        return this;
    }

    public StringBuilder Build()
    {
        var sb = new StringBuilder();

        if (_options.Count > 0)
        {
            sb.AppendJoin(' ', _options);
        }

        return sb;
    }
}