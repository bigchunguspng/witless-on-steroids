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

    public FFMpegArgs Globals(string options)
    {
        _globals.Add(options);
        return this;
    }

    public FFMpegArgs Input(string path)
    {
        _inputs.Add(path, new FFMpegInputOptions());
        return this;
    }

    public FFMpegArgs Input(string path, FFMpegInputOptions options)
    {
        _inputs.Add(path, options);
        return this;
    }

    public FFMpegArgs Input(string path, Func<FFMpegInputOptions, FFMpegInputOptions> options)
    {
        _inputs.Add(path, options(new FFMpegInputOptions()));
        return this;
    }

    public FFMpegArgs Filter(string filter)
    {
        _filters.Add(filter);
        return this;
    }

    public FFMpegArgs Out(string path)
    {
        _outputs.Add(path, new FFMpegOutputOptions());
        return this;
    }

    public FFMpegArgs Out(string path, FFMpegOutputOptions options)
    {
        _outputs.Add(path, options);
        return this;
    }

    public FFMpegArgs Out(string path, Func<FFMpegOutputOptions, FFMpegOutputOptions> options)
    {
        _outputs.Add(path, options(new FFMpegOutputOptions()));
        return this;
    }

    public string Build()
    {
        var sb = new StringBuilder();

        foreach (var input in _inputs)
        {
            input.Deconstruct(out var path, out var options);

            sb.AppendSpaceSeparator();
            var optionsText = options.Build();
            if (optionsText.Length > 0)
                sb.Append(optionsText).Append(' ');

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
            var optionsText = options.Build();
            if (optionsText.Length > 0)
                sb.Append(optionsText).Append(' ');

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