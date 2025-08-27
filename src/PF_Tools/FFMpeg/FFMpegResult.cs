using System.Text;

namespace PF_Tools.FFMpeg;

public class FFMpegResult
{
    public required string        Command       { get; init; }
    public required StringBuilder ProcessOutput { get; init; }
    public required int           ExitCode      { get; init; }

    public          bool          Success => ExitCode == 0;
    public          bool          Failure => ExitCode != 0;

}