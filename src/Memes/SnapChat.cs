using Witlesss.Commands.Meme.Core;
using Witlesss.Memes.Shared;

namespace Witlesss.Memes;

public class SnapChat : MemeGeneratorBase, IMemeGenerator<string>
{
    public string GenerateMeme(MemeFileRequest request, string text)
    {
        throw new NotImplementedException();
    }

    public Task<string> GenerateVideoMeme(MemeFileRequest request, string text)
    {
        throw new NotImplementedException();
    }
}