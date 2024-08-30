using Witlesss.Commands.Meme.Core;

namespace Witlesss.Memes.Shared;

public interface IMemeGenerator<in T>
{
    string
        GenerateMeme
        (MemeFileRequest request, T text);

    Task<string>
        GenerateVideoMeme
        (MemeFileRequest request, T text);
}

public record TextPair(string A, string B);