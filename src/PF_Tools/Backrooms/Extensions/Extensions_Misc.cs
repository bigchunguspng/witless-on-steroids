namespace PF_Tools.Backrooms.Extensions;

public static class Extensions_Misc
{
    public static async Task DownloadFileAsync
        (this HttpClient client, string url, string path)
    {
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        await using var fs = File.Create(path);
        await response.Content.CopyToAsync(fs);
    }
}