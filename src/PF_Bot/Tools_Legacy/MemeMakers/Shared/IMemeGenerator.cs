using PF_Bot.Features.Generate.Memes.Core;

namespace PF_Bot.Tools_Legacy.MemeMakers.Shared;

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