using System.Text;
using FFMpegInput  = (string Path, PF_Tools.FFMpeg.FFMpegInputOptions  Options);
using FFMpegOutput = (string Path, PF_Tools.FFMpeg.FFMpegOutputOptions Options);

namespace PF_Tools.FFMpeg;

public delegate FFMpegInputOptions  FFMpegInputPipeline (FFMpegInputOptions  opts);
public delegate FFMpegOutputOptions FFMpegOutputPipeline(FFMpegOutputOptions opts);

public class FFMpegArgs
{
    private readonly List<string>       _globals = [];
    private readonly List<FFMpegInput>   _inputs = [];
    private readonly StringBuilder       _filter = new();
    private readonly List<FFMpegOutput> _outputs = [];

    //

    public FFMpegArgs Globals
        (string options)
        => this.Fluent(() => _globals.Add(options));

    public FFMpegArgs Input
        (string path)
        => this.Fluent(() => _inputs.Add((path, new FFMpegInputOptions())));

    public FFMpegArgs Input
        (string path, FFMpegInputOptions options)
        => this.Fluent(() => _inputs.Add((path, options)));

    public FFMpegArgs Input
        (string path, FFMpegInputPipeline options)
        => this.Fluent(() => _inputs.Add((path, options)));

    /// Arg is auto-prepended by ';' if filtergraph is not empty.
    public FFMpegArgs Filter
        (string filter)
        => this.Fluent(() => _filter.AppendSeparator(';').Append(filter));

    /// Arg is auto-prepended by ',' if filtergraph is not empty,
    /// except when filtergraph ends with ']' or arg starts with ':', '=' or '+'.
    /// Don't pass empty strings!
    public FFMpegArgs FilterAppend
        (string filter)
        => this.Fluent(() =>
        {
            if (_filter.Length is not 0)
            {
                var ending = _filter[^1];
                var start  =  filter[0];
                var insertComma = false == (ending is ']' || start is ':' or '=' or '+');
                if (insertComma)
                    _filter.Append(',');
            }
            _filter.Append(filter);
        });

    public FFMpegArgs Out
        (string path)
        => this.Fluent(() => _outputs.Add((path, new FFMpegOutputOptions())));

    public FFMpegArgs Out
        (string path, FFMpegOutputOptions options)
        => this.Fluent(() => _outputs.Add((path, options)));

    public FFMpegArgs Out
        (string path, FFMpegOutputPipeline options)
        => this.Fluent(() => _outputs.Add((path, options)));

    public string Build()
    {
        var sb = new StringBuilder();

        foreach (var input in _inputs)
        {
            var (path, options) = input;

            sb.AppendSpaceSeparator();
            var optionsSb = options.Build();
            if (optionsSb.Length > 0) sb.Append(optionsSb).Append(' ');
            sb.Append("-i ").AppendInQuotes(path);
        }

        if (_filter.Length > 0)
        {
            sb.AppendSpaceSeparator();
            sb.Append("-filter_complex ").AppendInQuotes(_filter);
        }

        foreach (var output in _outputs)
        {
            var (path, options) = output;

            sb.AppendSpaceSeparator();
            var optionsSb = options.Build();
            if (optionsSb.Length > 0) sb.Append(optionsSb).Append(' ');
            sb.AppendInQuotes(path);
        }

        if (_globals.Count > 0)
        {
            sb.AppendSpaceSeparator();
            sb.AppendJoin(' ', _globals);
        }

        return sb.ToString();
    }
}