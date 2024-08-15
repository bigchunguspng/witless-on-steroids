using Witlesss.Commands.Meme.Core;
using Witlesss.Memes.Shared;

namespace Witlesss.Memes;

public class DukeNukem : IMemeGenerator<int>
{
    public static int Depth = 1;

    public string GenerateMeme(MemeFileRequest request, int text)
    {
        var path = request.SourcePath;

        for (var i = 0; i < Depth; i++)
        {
            path = new F_Process(path)
                .DeepFry(request.GetQscale())
                .OutputAs(UniquePath(request.TargetPath)).Result;
        }

        return path;
    }

    public async Task<string> GenerateVideoMeme(MemeFileRequest request, int text)
    {
        var size = FFMpegXD.GetPictureSize(request.SourcePath).GrowSize().ValidMp4Size();

        var path = request.SourcePath;

        for (var i = 0; i < Depth.Clamp(3); i++)
        {
            path = await new F_Process(path)
                .DeepFryVideo(size.Ok(), request.GetCRF())
                .OutputAs(UniquePath(request.TargetPath));
        }

        return path;
    }
}