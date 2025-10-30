using ColorHelper;
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

namespace PF_Bot.Features_Main.Memes.Core.Generators;

public struct MemeOptions_Meme()
{
    public FontOption FontOption;

    /// Color to fill sticker background with.
    public ColorOption CustomColorBack;
    public ColorOption CustomColorText;
    public ColorOption CustomColorShad;

    /// Text vertical offset, 0-100%
    /// If negative, standart 2-text meme is generated.
    public int TextOffset         =  -1;
    public int FontSizeMultiplier = 100;
    public int ShadowThickness    = 100;

    public byte ShadowOpacity = 100;

    public bool WrapText = true;
    public bool RandomTextColor;
    /// Text is placed at random vertical offset of 15-85%.
    public bool RandomTextOffset;
    /// Top and bottom texts are placed close to vertical borders.
    public bool           NoMargin;
    /// Top and bottom texts cross vertical borders XD.
    public bool AbsolutelyNoMargin;

    public float    FontSizeMultiplier_Float =>    FontSizeMultiplier / 100F;

    /// Generate meme with 1 offsetable text, insead of traditional 2-text one.
    public bool FloatingCaptionMode => RandomTextOffset || TextOffset >= 0;
}

public partial class MemeGenerator(MemeOptions_Meme op) : MemeGeneratorBase, IMemeGenerator<TextPair>
{
    // SIZE

    private int _w, _h, _marginY, _marginX;
    private float _fontOffset;
    private Size _captionSize;

    // DATA

    private Rgba32 _shadowColor = Color.Black;

    private SolidBrush _textBrush = null!;

    private readonly SolidBrush _white = new(Color.White);

    private readonly DrawingOptions _textDrawingOptions = new()
    {
        GraphicsOptions = new GraphicsOptions { Antialias = true, AntialiasSubpixelDepth = 16 }
    };


    // LOGIC

    public async Task GenerateMeme(MemeRequest request, TextPair text)
    {
        await FetchImageSize(request);
        SetUp();

        using var image = await GetImage(request);

        SetCaptionColors(image);
        using var caption = DrawCaption(text);
        using var meme = Combine(image, caption);

        meme.ApplyPressure(request.Press);

        var saveImageTask = request.ExportAsSticker
            ? ImageSaver.SaveImageWebp(meme, request.TargetPath, request.Quality)
            : ImageSaver.SaveImageJpeg(meme, request.TargetPath, request.Quality);

        await saveImageTask;
    }

    public async Task GenerateVideoMeme(MemeRequest request, TextPair text)
    {
        await FetchVideoSize(request);
        SetUp();
        SetCaptionColors(op.CustomColorText.ByCoords ? await request.GetVideoSnapshot() : null);

        using var caption = DrawCaption(text);
        var captionAsFile = await ImageSaver.SaveImageTemp(caption);
        var probe = await request.ProbeSource();
        await new FFMpeg_Meme(probe, request, captionAsFile)
            .Meme(_sourceSizeAdjusted)
            .FFMpeg_Run();
    }

    private void SetUp()
    {
        _w = _sourceSizeAdjusted.Width;
        _h = _sourceSizeAdjusted.Height;

        _marginX = op.NoMargin ? 0 : op.AbsolutelyNoMargin ? 0 - _w / 20 : Math.Max(_w / 20, 10);
        _marginY = op.NoMargin ? 0 : op.AbsolutelyNoMargin ? 0 - _h / 30 : Math.Max(_h / 30, 10);

        if (op.FloatingCaptionMode)
        {
            var offset = op.RandomTextOffset ? Fortune.RandomInt(15, 85) : op.TextOffset;
            _marginY = _h * offset / 100;
        }

        SetUpFonts();
    }

    private async Task<Image<Rgba32>> GetImage(MemeRequest request)
    {
        if (request is { IsSticker: true, ExportAsSticker: false })
        {
            using var image = await GetImage(request.SourcePath);

            var color = op.CustomColorBack.GetColor(image);
            var background = color.HasValue
                ?     new Image<Rgba32>(_w, _h, color.Value)
                : image.HasTransparentAreas(200)
                    ? await GetFunnyBackground()
                    : new Image<Rgba32>(_w, _h, Color.White);

            background.Mutate(x => x.DrawImage(image));
            return background;
        }
        else
            return await GetImage(request.SourcePath);
    }

    private async Task<Image<Rgba32>> GetFunnyBackground()
    {
        var sw = Stopwatch.StartNew();
        var file = Dir_Backs.GetFiles().PickAny();
        var back = await Image.LoadAsync<Rgba32>(file);
        var size = back.Size.AdjustBackgroundSize(new Size(_w, _h));
        var x = (size.Width  - _w) / 2;
        var y = (size.Height - _h) / 2;
        var crop = new Rectangle(x, y, _w, _h);
        back.Mutate(ctx =>
        {
            if (size != back.Size)
                ctx.Resize(size);

            if (size != crop.Size && crop.Location != Point.Empty)
                ctx.Crop(crop);
        });
        sw.Log("/meme -> GetFunnyBackground");
        return back;
    }

    public static void ScaleBackgrounds()
    {
        var stickerSize = new Size(512, 512);
        foreach (var file in Dir_Backs.Combine("_").GetFiles())
        {
            var image = Image.Load<Rgba32>(file);
            var size  = image.Size.AdjustBackgroundSize(stickerSize);
            image.Mutate(ctx => ctx.Resize(size));
            var name = Path.GetFileNameWithoutExtension(file);
            var path = Dir_Backs.Combine(name + ".png");
            image.SaveAsPng(path);
        }
    }

    private Image<Rgba32> Combine(Image<Rgba32> image, Image<Rgba32> caption)
    {
        image.Mutate(x => x.DrawImage(caption));
        return image;
    }

    private Image<Rgba32> DrawCaption(TextPair text)
    {
        var canvas = new Image<Rgba32>(_w, _h);

        var height = op.FloatingCaptionMode
            ? _h / 3
            : _h / 3 - _marginY;

        _captionSize = new Size(_w - 2 * _marginX, height);

        var tuple1 = AddText(canvas, text.A, top: true);
        var tuple2 = AddText(canvas, text.B, top: false);

        return op.ShadowOpacity > 0 ? DrawShadow(canvas, tuple1, tuple2) : canvas;
    }

    private (float height, float fontSize) AddText(Image<Rgba32> background, string text, bool top)
    {
        if (text.IsNull_OrEmpty()) return (0, 0);

        text = text.Trim('\n');

        var emoji = EmojiTool.FindEmoji(text);
        var plain = emoji.Count == 0;
            
        var pngs = plain ? null : EmojiTool.GetEmojiPngs(emoji);
        text = MakeTextFitCard(plain ? text : EmojiTool.ReplaceEmoji(text, "👌", emoji, pngs!));

        var origin = GetTextOrigin(text, top, out var caseOffset);
        var options = GetDefaultTextOptions(origin, top);

        if (plain)
        {
            options.WrappingLength = -1;
            background.Mutate(x => x.DrawText(_textDrawingOptions, options, text, _textBrush, pen: null));
        }
        else
        {
            var pixelate = op.FontOption.FontIsPixelated();
            var optionsE = new EmojiTool.Options(_textBrush, _w, GetEmojiSize(), _fontOffset, Pixelate: pixelate);
            var textLayer = EmojiTool.DrawEmojiText(text, options, optionsE, pngs!.AsQueue());
            var point = GetFunnyOrigin(textLayer.Size, options, top, caseOffset);
            background.Mutate(ctx => ctx.DrawImage(textLayer, point));
        }

        LogDebug($"/meme >> font size: {FontSize:F2}");

        return (FontSize * GetLineSpacing() * text.GetLineCount(), FontSize);
    }

    private void SetCaptionColors(Image<Rgba32>? image)
    {
        var colorText = op.CustomColorText.GetColor(image);
        _textBrush = colorText.HasValue ? new SolidBrush(colorText.Value)
            : op.RandomTextColor        ? new SolidBrush(RandomTextColor())
            : _white;
        _shadowColor = op.CustomColorShad.GetColor(image) ?? Color.Black;
    }

    private Color RandomTextColor()
    {
        var h =       Random.Shared.Next(360);
        var s = (byte)Random.Shared.Next(50, 100);
        var l = (byte)Random.Shared.Next(50,  95);

        var rgb = ColorConverter.HslToRgb(new HSL(h, s, l));
        return rgb.ToRgb24();
    }
}