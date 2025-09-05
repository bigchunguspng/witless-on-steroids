using PF_Bot.Features.Generate.Memes.Core;
using PF_Bot.Tools_Legacy.Technical;
using SixLabors.ImageSharp;

namespace PF_Bot.Tools_Legacy.MemeMakers.Shared;

public record VideoMemeRequest(int Quality, float Press, string Caption)
{
    public static VideoMemeRequest From
        (MemeFileRequest request, Image caption)
        => new(request.GetCRF(), request.Press, ImageSaver.SaveImageTemp(caption));

    public static VideoMemeRequest From
        (MemeFileRequest request, string captionAsFile)
        => new(request.GetCRF(), request.Press, captionAsFile);
}