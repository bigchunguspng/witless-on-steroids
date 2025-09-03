using System.Text;

namespace PF_Tools.FFMpeg;

public class FFMpegInputOptions
{
    private readonly List<string> _options = [];

    public FFMpegInputOptions Options
        (string options)
        => this.Fluent(() => _options.Add(options));

    public StringBuilder Build()
    {
        var sb = new StringBuilder();
        return _options.Count > 0 ? sb.AppendJoin(' ', _options) : sb;
    }

    public static implicit operator FFMpegInputOptions
        (string options) => new FFMpegInputOptions().Options(options);

    public static implicit operator FFMpegInputOptions
        (FFMpegInputPipeline options) => options(new FFMpegInputOptions());
}