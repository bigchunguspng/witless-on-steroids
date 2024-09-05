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
            path = request.UseFFMpeg()
                .Nuke(request.GetQscale())
                .OutAs(UniquePath(request.TargetPath)).Result;
        }

        return path;
    }

    public async Task<string> GenerateVideoMeme(MemeFileRequest request, int text)
    {
        var size = FFMpegXD.GetPictureSize(request.SourcePath).GrowSize().ValidMp4Size();

        var path = request.SourcePath;

        for (var i = 0; i < Depth.Clamp(3); i++)
        {
            path = await request.UseFFMpeg()
                .NukeVideo(size.Ok(), request.GetCRF())
                .OutAs(UniquePath(request.TargetPath));
        }

        return path;
    }
}