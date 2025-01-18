using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Witlesss.Commands.Meme.Core;
using Witlesss.Memes.Shared;

namespace Witlesss.Memes;

public partial class SnapChat : MemeGeneratorBase, IMemeGenerator<string>
{
    // OPTIONS

    public static bool WrapText = true, RandomOffset;
    public static int CardOpacity = 62, CardOffset = 50;
    public static float MinSizeMultiplier = 10, FontSizeMultiplier = 100;
    public static CustomColorOption CustomColorBack = new("_"), CustomColorText = new("#");

    // SIZE

    private int _w, _h, _marginX, _offsetY, _cardHeight;

    // DATA

    private SolidBrush _textBrush = default!;

    private readonly SolidBrush _white = new(Color.White);

    public string GenerateMeme(MemeFileRequest request, string text)
    {
        FetchImageSize(request);
        SetUp();

        using var image = GetImage(request);

        SetCaptionColor(image);
        using var card = DrawText(text);
        using var meme = Combine(image, card);

        meme.ApplyPressure(request.Press);

        return request.ExportAsSticker
            ? ImageSaver.SaveImageWebp(meme, request.TargetPath, request.Quality)
            : ImageSaver.SaveImage    (meme, request.TargetPath, request.Quality);
    }

    public Task<string> GenerateVideoMeme(MemeFileRequest request, string text)
    {
        FetchVideoSize(request);
        SetUp();
        SetCaptionColor(CustomColorText.ByCoords ? request.GetVideoSnapshot() : null);

        using var caption = DrawText(text);
        return request.UseFFMpeg()
            .Meme(VideoMemeRequest.From(request, caption), _sourceSizeAdjusted)
            .OutAs(request.TargetPath);
    }

    private void SetUp()
    {
        _w = _sourceSizeAdjusted.Width;
        _h = _sourceSizeAdjusted.Height;

        var offsetBase = RandomOffset ? RandomInt(25, 75) : CardOffset;
        _offsetY = _h * offsetBase / 100;
        _marginX = Math.Max(_w / 20, 10);

        SetUpFonts();
    }

    private void SetCardHeight(int x)
    {
        _cardHeight = x.ToEven();
    }

    private Image<Rgba32> GetImage(MemeFileRequest request)
    {
        if (request is { IsSticker: true, ExportAsSticker: false })
        {
            using var image = GetImage(request.SourcePath);

            var color = CustomColorBack.GetColor(image) ?? Color.Black;
            var background = new Image<Rgba32>(_w, _h, color);

            background.Mutate(x => x.DrawImage(image));
            return background;
        }
        else
            return GetImage(request.SourcePath);
    }

    private Image<Rgba32> DrawText(string text)
    {
        var emoji = EmojiRegex.Matches(text);
        var plain = emoji.Count == 0;

        var pngs = plain ? null : EmojiTool.GetEmojiPngs(emoji);
        text = MakeTextFitCard(plain ? text : EmojiTool.ReplaceEmoji(text, "👌", emoji, pngs));

        AdjustTextOffset(text);

        var image = new Image<Rgba32>(_w, _h);
        var options = GetDefaultTextOptions();

        image.Mutate(x =>
        {
            var y = (_offsetY - 0.5 * _cardHeight).RoundInt();
            var rect = new Rectangle(0, y, _w, _cardHeight);
            var color = new Rgba32(0, 0, 0, CardOpacity / 100F);
            x.Fill(color, rect);
        });

        if (plain)
        {
            options.WrappingLength = -1;
            image.Mutate(x => x.DrawText(options, text, _textBrush, pen: null));
        }
        else
        {
            var optionsE = new EmojiTool.Options(_textBrush, _w, GetEmojiSize(), _fontOffset);
            var textLayer = EmojiTool.DrawEmojiText(text, options, optionsE, pngs!.AsQueue());
            image.Mutate(ctx => ctx.DrawImage(textLayer, GetOriginFunny(textLayer.Size)));
        }

        return image;
    }

    private Image<Rgba32> Combine(Image? source, Image caption)
    {
        var meme = new Image<Rgba32>(_w, _h);

        if (source != null)
            meme.Mutate(x => x.DrawImage(source));

        meme.Mutate(x => x.DrawImage(caption));

        return meme;
    }

    private void SetCaptionColor(Image<Rgba32>? image)
    {
        var color = CustomColorText.GetColor(image);
        _textBrush = color is null
            ? _white
            : new SolidBrush(color.Value);
    }
}