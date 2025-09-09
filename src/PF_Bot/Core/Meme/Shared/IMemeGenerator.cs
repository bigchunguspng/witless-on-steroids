namespace PF_Bot.Core.Meme.Shared;

public interface IMemeGenerator<in T>
{
    string
        GenerateMeme
        (MemeFileRequest request, T text);

    Task
        GenerateVideoMeme
        (MemeFileRequest request, T text);
}

public record TextPair(string A, string B);