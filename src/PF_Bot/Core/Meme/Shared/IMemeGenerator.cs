namespace PF_Bot.Core.Meme.Shared;

public interface IMemeGenerator<in T>
{
    Task GenerateMeme
        (MemeFileRequest request, FilePath output, T text);

    Task GenerateVideoMeme
        (MemeFileRequest request, FilePath output, T text);
}

public record TextPair(string A, string B);