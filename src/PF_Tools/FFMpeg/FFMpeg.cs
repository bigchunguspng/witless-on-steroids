using System.Diagnostics;
using System.Text;

namespace PF_Tools.FFMpeg;

public static class FFMpeg
{
	private const string FFMPEG = "ffmpeg";

	public static FFMpegArgs Args() => new();

	public static async Task<FFMpegResult> Run(FFMpegArgs args, string? directory = "")
	{
		var arguments = args.Build();

		using var process = new Process();
		process.StartInfo = new ProcessStartInfo
		{
			FileName = FFMPEG, Arguments = arguments,
			WorkingDirectory = directory,
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError  = true,
			StandardOutputEncoding = Encoding.UTF8,
			StandardErrorEncoding  = Encoding.UTF8,
		};

		var output = new StringBuilder();
		process.OutputDataReceived += (_, e) => output.Append(e.Data).Append('\n');
		process. ErrorDataReceived += (_, e) => output.Append(e.Data).Append('\n');

		process.Start();

		process.BeginOutputReadLine();
		process.BeginErrorReadLine();

		await process.WaitForExitAsync();

		return new FFMpegResult
		{
			ExitCode = process.ExitCode,
			Command = $"{FFMPEG} {arguments}",
			ProcessOutput = output,
		};
	}
}