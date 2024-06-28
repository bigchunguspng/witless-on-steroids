using System.Threading.Tasks;
using SixLabors.ImageSharp;
using Witlesss.Commands.Meme;
using Witlesss.MediaTools;

namespace Witlesss.Services.Memes;

public interface IMemeGenerator<in T>
{
    string GenerateMeme(MemeFileRequest request, T text);
    Task<string> GenerateVideoMeme(MemeFileRequest request, T text);
}

public abstract class DynamicSizeMemeGenerator<T> : IMemeGenerator<T>
{
    public string GenerateMeme(MemeFileRequest request, T text)
    {
        var size = GetImageSize(request.SourcePath);

        SetUp(size);

        var textLayer = DrawTextLayer(text);
        var captionLayer = DrawCaptionLayer(textLayer);
        var result = Combine(request.SourcePath, captionLayer);

        return ImageSaver.SaveImage(result, request.TargetPath, request.Quality);
    }

    public Task<string> GenerateVideoMeme(MemeFileRequest request, T text)
    {
        var size = SizeHelpers.GetImageSize_FFmpeg(request.SourcePath);

        SetUp(size);

        var textLayer = DrawTextLayer(text);
        var captionLayer = DrawCaptionLayer(textLayer);
        var caption = ImageSaver.SaveImageTemp(captionLayer);

        return MakeVideoMeme(request, caption).OutputAs(request.TargetPath);
    }

    /*private Image SetUpAndDrawCaptionLayer(Size size, T text)
    {
        SetUp(size);

        /*if (TextLayerDependsOnColor)
        {
            Image.Load(Memes.Snapshot(request.SourcePath))
        }#1#
        return DrawCaptionLayer(DrawTextLayer(text));
    }*/

    public abstract bool TextLayerDependsOnColor { get; }

    /// <summary>
    /// Calibrates the source image size,
    /// calulates all dependent sizes and margins.
    /// Full size and image size should NOT be changed after!
    /// </summary>
    protected abstract void SetUp(Size size);

    /// <summary>
    /// Creates an image with text (and emoji if any)
    /// </summary>
    protected abstract Image DrawTextLayer(T text);

    /// <summary>
    /// Creates the frame, card or shadowed text image from the text layer.
    /// </summary>
    protected abstract Image DrawCaptionLayer(Image textLayer);

    /// <summary>
    /// Creates the completed meme by combining the caption layer and the source image.
    /// </summary>
    protected abstract Image Combine(string sourcePath, Image captionLayer);

    protected abstract F_Action MakeVideoMeme(MemeFileRequest request, string captionPath);
    // new F_Combine(request.SourcePath, captionPath)
    // .%MemeType%(request.GetCRF(), ImageSize (0 if don't resize), ImageLocation, ...)

    private Size GetImageSize(string path)
    {
        return Image.Identify(path).Size;
    }
}