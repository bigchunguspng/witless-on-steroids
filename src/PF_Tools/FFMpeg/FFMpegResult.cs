using System.Text;

namespace PF_Tools.FFMpeg;

public class FFMpegResult(FFMpegArgs args)
{
    public string        Arguments     { get; } = args.Build();
    public StringBuilder ProcessOutput { get; } = new();

    public int  ExitCode  { get; set; }
    public bool WasKilled { get; set; }

    public bool Success => ExitCode == 0;
    public bool Failure => ExitCode != 0;
}