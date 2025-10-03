using System.Text;

namespace PF_Tools.ProcessRunning;

public static class GalleryDl
{
    public static async Task<List<Uri>> Run(string args, string directory = "")
    {
        var urls = new List<Uri>();
        var startedProcess = ProcessStarter.StartProcess_WithOutputHandler
            (GALLERY_DL, args, directory, (d, o) => Output_Save_Print_GetURL(d, o, urls));

        await startedProcess.Process.WaitForExitAsync();

        var result = new ProcessResult(args, startedProcess);
        if (result.Failure)
            throw new ProcessException(GALLERY_DL, result);

        return urls;
    }

    private static void Output_Save_Print_GetURL
        (string? data, StringBuilder output, List<Uri> urls)
    {
        ProcessStarter.Output_Save(data, output);

        if (data.IsNotNull_NorWhiteSpace())
        {
            Console.WriteLine(data);

            if (Uri.TryCreate(data, UriKind.Absolute, out var url))
                urls.Add(url);
        }
    }
}