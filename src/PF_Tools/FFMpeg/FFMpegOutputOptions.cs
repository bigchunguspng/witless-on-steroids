using System.Text;

namespace PF_Tools.FFMpeg;

public class FFMpegOutputOptions
{
    private readonly List<string> _vf = [];
    private readonly List<string> _af = [];
    private readonly List<string> _options = [];

    public FFMpegOutputOptions Options
        (string options)
        => this.Fluent(() => _options.Add(options));

    public FFMpegOutputOptions VF
        (string filter)
        => this.Fluent(() => _vf.Add(filter));

    public FFMpegOutputOptions AF
        (string filter)
        => this.Fluent(() => _af.Add(filter));

    public FFMpegOutputOptions Map
        (string label)
        => Options($"-map {label}");

    public StringBuilder Build()
    {
        var sb = new StringBuilder();
        if (_vf     .Count > 0) sb                       .Append("-vf ").AppendInQuotes(_vf, ',');
        if (_af     .Count > 0) sb.AppendSpaceSeparator().Append("-af ").AppendInQuotes(_af, ',');
        if (_options.Count > 0) sb.AppendSpaceSeparator().AppendJoin(' ', _options);
        return sb;
    }

    public static implicit operator FFMpegOutputOptions
        (string options) => new FFMpegOutputOptions().Options(options);

    public static implicit operator FFMpegOutputOptions
        (FFMpegOutputPipeline options) => options(new FFMpegOutputOptions());
}