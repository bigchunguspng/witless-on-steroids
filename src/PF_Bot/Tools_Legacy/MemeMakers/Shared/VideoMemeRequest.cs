using PF_Bot.Features.Generate.Memes.Core;
using PF_Bot.Tools_Legacy.Technical;
using SixLabors.ImageSharp;

namespace PF_Bot.Tools_Legacy.MemeMakers.Shared;

public record VideoMemeRequest(Quality Quality, float Press, string Caption)
{
    public static VideoMemeRequest From
        (MemeFileRequest request, Image caption)
        => new(request.Quality, request.Press, ImageSaver.SaveImageTemp(caption));

    public static VideoMemeRequest From
        (MemeFileRequest request, string captionAsFile)
        => new(request.Quality, request.Press, captionAsFile);
}