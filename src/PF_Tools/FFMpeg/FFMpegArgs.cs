using System.Text;

namespace PF_Tools.FFMpeg;

public delegate FFMpegInputOptions  FFMpegInputPipeline (FFMpegInputOptions  opts);
public delegate FFMpegOutputOptions FFMpegOutputPipeline(FFMpegOutputOptions opts);

public class FFMpegArgs
{
    private readonly       List<string>                      _globals = [];
    private readonly Dictionary<string, FFMpegInputOptions>   _inputs = new();
    private readonly       List<string>                      _filters = [];
    private readonly Dictionary<string, FFMpegOutputOptions> _outputs = new();

    //

    public FFMpegArgs Globals
        (string options)
        => this.Fluent(() => _globals.Add(options));

    public FFMpegArgs Input
        (string path)
        => this.Fluent(() => _inputs.Add(path, new FFMpegInputOptions()));

    public FFMpegArgs Input
        (string path, FFMpegInputOptions options)
        => this.Fluent(() => _inputs.Add(path, options));

    public FFMpegArgs Input
        (string path, FFMpegInputPipeline options)
        => this.Fluent(() => _inputs.Add(path, options(new FFMpegInputOptions())));

    public FFMpegArgs Filter
        (string filter)
        => this.Fluent(() => _filters.Add(filter));

    public FFMpegArgs Out
        (string path)
        => this.Fluent(() => _outputs.Add(path, new FFMpegOutputOptions()));

    public FFMpegArgs Out
        (string path, FFMpegOutputOptions options)
        => this.Fluent(() => _outputs.Add(path, options));

    public FFMpegArgs Out
        (string path, FFMpegOutputPipeline options)
        => this.Fluent(() => _outputs.Add(path, options(new FFMpegOutputOptions())));

    public string Build()
    {
        var sb = new StringBuilder();

        foreach (var input in _inputs)
        {
            input.Deconstruct(out var path, out var options);

            sb.AppendSpaceSeparator();
            var optionsSb = options.Build();
            if (optionsSb.Length > 0) sb.Append(optionsSb).Append(' ');
            sb.Append("-i ").AppendInQuotes(path);
        }

        if (_filters.Count > 0)
        {
            sb.AppendSpaceSeparator();
            sb.Append("-filter_complex ").AppendInQuotes(_filters, ';');
        }

        foreach (var output in _outputs)
        {
            output.Deconstruct(out var path, out var options);

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