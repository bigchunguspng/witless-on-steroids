namespace PF_Bot.Core.Memes.Shared;

public interface IMemeGenerator<in T>
{
    Task GenerateMeme
        (MemeRequest request, T text);

    Task GenerateVideoMeme
        (MemeRequest request, T text);
}

public record TextPair(string A, string B);