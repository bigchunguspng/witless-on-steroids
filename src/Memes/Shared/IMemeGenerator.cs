using System.Threading.Tasks;
using Witlesss.Commands.Meme;

namespace Witlesss.Memes.Shared;

public interface IMemeGenerator<in T>
{
    string GenerateMeme(MemeFileRequest request, T text);
    Task<string> GenerateVideoMeme(MemeFileRequest request, T text);
}