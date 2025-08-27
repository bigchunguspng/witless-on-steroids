using System.Text;
using PF_Tools.Backrooms.Extensions;

namespace PF_Tools.FFMpeg;

public class FFMpegOutputOptions
{
    private readonly List<string> _vf = [];
    private readonly List<string> _af = [];
    private readonly List<string> _options = [];

    public FFMpegOutputOptions Options(string options)
    {
        _options.Add(options);
        return this;
    }

    public FFMpegOutputOptions VF(string filter)
    {
        _vf.Add(filter);
        return this;
    }

    public FFMpegOutputOptions AF(string filter)
    {
        _af.Add(filter);
        return this;
    }

    public FFMpegOutputOptions Map(string label)
    {
        return Options($"-map {label}");
    }

    public StringBuilder Build()
    {
        var sb = new StringBuilder();

        if (_vf.Count > 0)
        {
            sb.Append("-vf ").AppendInQuotes(_vf, ',');
        }

        if (_af.Count > 0)
        {
            sb.AppendSpaceSeparator();
            sb.Append("-af ").AppendInQuotes(_af, ',');
        }

        if (_options.Count > 0)
        {
            sb.AppendSpaceSeparator();
            sb.AppendJoin(' ', _options);
        }

        return sb;
    }
}