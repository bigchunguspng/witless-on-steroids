using PF_Bot.Backrooms.Helpers;
using PF_Bot.Core.Editing;
using PF_Bot.Core.Meme.Options;
using PF_Bot.Core.Meme.Shared;
using PF_Tools.FFMpeg;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PF_Bot.Core.Meme.Generators;

public struct MemeOptions_Snap()
{
    public FontOption FontOption;

    public ColorOption CustomColorBack;
    public ColorOption CustomColorText;

    public int CardOffset  = 50;
    public int CardOpacity = 62;

    public float  MinSizeMultiplier =  10;
    public float FontSizeMultiplier = 100;

    public bool WrapText     = true;
    public bool RandomOffset;
}

public partial class SnapChat(MemeOptions_Snap op) : MemeGeneratorBase, IMemeGenerator<string>
{
    // SIZE

    private int _w, _h, _marginX, _offsetY, _cardHeight;

    // DATA

    private SolidBrush _textBrush = null!;

    private readonly SolidBrush _white = new(Color.White);

    public async Task GenerateMeme(MemeFileRequest request, FilePath output, string text)
    {
        await FetchImageSize(request);
        SetUp();

        using var image = await GetImage(request);

        SetCaptionColor(image);
        using var card = DrawText(text);
        using var meme = Combine(image, card);

        meme.ApplyPressure(request.Press);

        var saveImageTask = request.ExportAsSticker
            ? ImageSaver.SaveImageWebp(meme, output, request.Quality)
            : ImageSaver.SaveImageJpeg(meme, output, request.Quality);

        await saveImageTask;
    }

    public async Task GenerateVideoMeme(MemeFileRequest request, FilePath output, string text)
    {
        await FetchVideoSize(request);
        SetUp();
        SetCaptionColor(op.CustomColorText.ByCoords ? await request.GetVideoSnapshot() : null);

        using var caption = DrawText(text);
        var captionAsFile = await ImageSaver.SaveImageTemp(caption);
        var probe = await request.ProbeSource();
        await new FFMpeg_Meme(probe, request, output, captionAsFile)
            .Meme(_sourceSizeAdjusted)
            .FFMpeg_Run();
    }

    private void SetUp()
    {
        _w = _sourceSizeAdjusted.Width;
        _h = _sourceSizeAdjusted.Height;

        var offsetBase = op.RandomOffset ? Fortune.RandomInt(25, 75) : op.CardOffset;
        _offsetY = _h * offsetBase / 100;
        _marginX = Math.Max(_w / 20, 10);

        SetUpFonts();
    }

    private void SetCardHeight(int x)
    {
        _cardHeight = x.ToEven();
    }

    private async Task<Image<Rgba32>> GetImage(MemeFileRequest request)
    {
        if (request is { IsSticker: true, ExportAsSticker: false })
        {
            using var image = await GetImage(request.SourcePath);

            var color = op.CustomColorBack.GetColor(image) ?? Color.Black;
            var background = new Image<Rgba32>(_w, _h, color);

            background.Mutate(x => x.DrawImage(image));
            return background;
        }
        else
            return await GetImage(request.SourcePath);
    }

    private Image<Rgba32> DrawText(string text)
    {
        var emoji = EmojiTool.FindEmoji(text);
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
            var color = new Rgba32(0, 0, 0, op.CardOpacity / 100F);
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
        var color = op.CustomColorText.GetColor(image);
        _textBrush = color is null
            ? _white
            : new SolidBrush(color.Value);
    }
}