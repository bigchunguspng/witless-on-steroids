using System.Diagnostics;
using System.Text;

namespace PF_Tools.FFMpeg;

public static class FFMpeg
{
	public const string FFMPEG = "ffmpeg";

	public static FFMpegArgs Args() => new();

	/// Builds arguments and starts FFMpeg process.
	/// All output is redirected to result's <see cref="FFMpegResult.ProcessOutput"/>.
	/// <br/>You still need to wait for the process exit!
	/// <see cref="FFMpegResult"/> result is returned without <see cref="FFMpegResult.ExitCode"/>!
	public static (Process process, FFMpegResult result) StartProcess(FFMpegArgs args)
	{
		var result = new FFMpegResult(args);
		var process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = FFMPEG, Arguments = result.Arguments,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError  = true,
				StandardOutputEncoding = Encoding.UTF8,
				StandardErrorEncoding  = Encoding.UTF8,
			},
		};

		process.OutputDataReceived += (_, e) => result.ProcessOutput.Append(e.Data).Append('\n');
		process. ErrorDataReceived += (_, e) => result.ProcessOutput.Append(e.Data).Append('\n');

		process.Start();

		process.BeginOutputReadLine();
		process.BeginErrorReadLine();

		return (process, result);
	}
}