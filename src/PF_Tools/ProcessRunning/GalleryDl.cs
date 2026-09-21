namespace PF_Tools.ProcessRunning;

public static class GalleryDl
{
    public static async Task<List<Uri>> Run(string args, string directory = "")
    {
        var startedProcess = ProcessStarter.StartProcess_WithOutputHandler
            (GALLERY_DL, args, directory, echo: true);

        var urls = new List<Uri>();
        startedProcess.Output
            .Where(x => x.Line.IsNotNull_NorWhiteSpace())
            .ForEach(x =>
            {
                if (Uri.TryCreate(x.Line, UriKind.Absolute, out var url)) urls.Add(url);
            });

        await startedProcess.Process.WaitForExitAsync();

        var result = new ProcessResult(args, startedProcess);
        if (result.Failure)
            throw new ProcessException(GALLERY_DL, result);

        return urls;
    }
}