namespace PF_Bot.Core.Memes.Shared;

public interface IMemeGenerator<in T>
{
    Task GenerateMeme
        (MemeFileRequest request, T text);

    Task GenerateVideoMeme
        (MemeFileRequest request, T text);
}

public record TextPair(string A, string B);