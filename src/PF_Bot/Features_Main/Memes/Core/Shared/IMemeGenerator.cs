namespace PF_Bot.Features_Main.Memes.Core.Shared;

public interface IMemeGenerator<in T>
{
    Task GenerateMeme
        (MemeRequest request, T text);

    Task GenerateVideoMeme
        (MemeRequest request, T text);
}

public record TextPair(string A, string B);