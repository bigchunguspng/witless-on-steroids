using PF_Bot.Features_Main.Edit.Core;
using PF_Bot.Features_Main.Edit.Helpers;
using PF_Bot.Features_Main.Memes.Core.Options;
using PF_Bot.Features_Main.Memes.Core.Shared;
using PF_Tools.FFMpeg;
using PF_Tools.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PF_Bot.Features_Main.Memes.Core.Generators; // ReSharper disable InconsistentNaming

public struct MemeOptions_Dp()
{
    public FontOption FontOption;

    /// Color of frame and text.
    public ColorOption CustomColor;

    public int MinFontSizeMultiplier =  10;
    public int    FontSizeMultiplier = 100;

    public bool WrapText = true;
    public bool Minimalist;

    public float MinFontSizeMultiplier_Float => MinFontSizeMultiplier / 100F;
    public float    FontSizeMultiplier_Float =>    FontSizeMultiplier / 100F;
}

public partial class Demotivators2077(MemeOptions_Dp options) : MemeGeneratorBase, IMemeGenerator<string>
{
    private MemeOptions_Dp op = options;

    // SIZE

    private const int FRAME_MARGIN = 5;

    private int _frameMargin, _frameWidth;

    private int imageW, imageH, fullW, fullH, marginTop;
    private double _ratio;

    private Point _imageOrigin;

    // DATA

    private Rgb24 FrameColor;
    private SolidBrush TextBrush = null!;

    private readonly SolidBrush WhiteBrush = new(Color.White);

    // LOGIC

    public async Task GenerateMeme(MemeRequest request, string text)
    {
        await FetchImageSize(request);
        SetUp();

        text = ArrangeText(text, out var emojiPngs);

        SetUpFrameSize(request);
        using var image = await GetImage(request.SourcePath);

        SetColor(image);
        using var frame = DrawFrame(text, emojiPngs);

        InsertImage(frame, image);

        frame.ApplyPressure(request.Press);

        await ImageSaver.SaveImageJpeg(frame, request.TargetPath, request.Quality);
    }

    public async Task GenerateVideoMeme(MemeRequest request, string text)
    {
        await FetchVideoSize(request);
        SetUp();

        text = ArrangeText(text, out var emojiPngs);

        SetUpFrameSize(request);
        SetColor(op.CustomColor.ByCoords ? await request.GetVideoSnapshot() : null);

        using var frame = DrawFrame(text, emojiPngs);
        var frameAsFile = await ImageSaver.SaveImageTemp(frame);

        var probe = await request.ProbeSource();
        await new FFMpeg_Meme(probe, request, frameAsFile)
            .Demotivator(_sourceSizeAdjusted, _imageOrigin)
            .FFMpeg_Run();
    }

    private void SetUp()
    {
        imageW = _sourceSizeAdjusted.Width;
        imageH = _sourceSizeAdjusted.Height;

        _ratio = _sourceSizeAdjusted.AspectRatio();

        if (_ratio > 3) op.Minimalist = true;

        SetUpFonts();
    }

    // CALCULATE

    private string ArrangeText(string text, out EmojiPngList? pngs)
    {
        var emoji = EmojiTool.FindEmoji(text);
        var plain = emoji.Count == 0;
        pngs = plain ? null : EmojiTool.GetEmojiPngs(emoji);
        text = plain ? text : EmojiTool.ReplaceEmoji(text, "👌", emoji, pngs!);
        return MakeTextFitCard(text);
    }

    private void SetUpFrameSize(MemeRequest request)
    {
        var space = Math.Max(imageH / 30F, 4);
        var lineHeight = FontSize * GetLineSpacing();
        var textHeight = _textHeight + 0.5F * lineHeight;
        var n = op.Minimalist ? 2 : 3;
        fullH = (imageH + textHeight + n * space).RoundInt().ToEven();
        fullW = op.Minimalist ? imageW : (fullH * _ratio).RoundInt().ToEven();

        marginTop = op.Minimalist ? 0 : (2 * space).RoundInt();

        var targetSize = request.IsVideo ? new Size(1280, 720) : new Size(1280, 800);
        var size = new Size(fullW, fullH).FitSize(targetSize);
        if (size.Width != fullW)
        {
            var k = size.Width / (float) fullW;
            fullW = (fullW * k).RoundInt().ToEven();
            fullH = (fullH * k).RoundInt().ToEven();
            imageW = (imageW * k).RoundInt();
            imageH = (imageH * k).RoundInt();
            _sourceSizeAdjusted = new Size(imageW, imageH);
            _textHeight *= k;
            marginTop = (marginTop * k).RoundInt();

            ResizeFont(FontSize * k);
        }

        _imageOrigin = op.Minimalist ? Point.Empty : new Point((fullW - imageW) / 2, marginTop);

        _frameMargin = imageW + imageH > 800 ? 5 : 3;
        _frameWidth  = imageW + imageH > 800 ? 3 : 2;
    }

    // DRAW

    private Image DrawFrame(string text, EmojiPngList? emojiPngs)
    {
        var background = new Image<Rgb24>(fullW, fullH, Color.Black);

        AdjustTextOffset(text);

        var options = GetDefaultTextOptions();

        if (emojiPngs is null)
        {
#if DEBUG
            Debug_Text(background, options);
#endif
            options.WrappingLength = -1;
            background.Mutate(x => x.DrawText(options, text, TextBrush, pen: null));
        }
        else
        {
            var optionsE = new EmojiTool.Options(TextBrush, fullW, GetEmojiSize(), _fontOffset);
            var textLayer = EmojiTool.DrawEmojiText(text, options, optionsE, emojiPngs.AsQueue());

            background.Mutate(ctx => ctx.DrawImage(textLayer, GetOriginFunny(textLayer.Size)));
        }

        background.DrawFrame(new Rectangle(_imageOrigin, _sourceSizeAdjusted), _frameWidth, _frameMargin, FrameColor);

        return background;
    }

    private void InsertImage(Image background, Image image)
    {
        background.Mutate(x => x.DrawImage(image, _imageOrigin));

        var size  = background.Size.FitSize(1280);
        if (size != background.Size)
        {
            background.Mutate(x => x.Resize(size));
        }
    }


    // OTHER STUFF I GUESS

    private void SetColor(Image<Rgba32>? image)
    {
        var color = op.CustomColor.GetColor(image);
        FrameColor = color?.Rgb ?? Color.White;
        TextBrush = color is null ? WhiteBrush : new SolidBrush(color.Value);
    }

    private void Debug_Text(Image image, RichTextOptions options)
    {
        var y = options.Origin.Y - _textHeight / 2F;
        image.Mutate(x => x.Fill(Color.Indigo, new RectangleF(0, y, fullW, _textHeight)));
    }
}