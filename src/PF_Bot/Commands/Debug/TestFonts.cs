using PF_Bot.Features_Main.Edit.Helpers;
using PF_Bot.Features_Main.Memes.Core.Options;
using PF_Bot.Routing.Messages.Commands;
using PF_Tools.Graphics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PF_Bot.Commands.Debug;

public class TestFonts : CommandHandlerAsync
{
    private const int
        W = 800,
        W_CODE = 80,
        H_LINE = 32,
        MARGIN_X = 10,
        MARGIN_Y = 5,
        FONT_SIZE = 24;

    protected override async Task Run()
    {
        var text = Args?.Split('\n', StringSplitOptions.RemoveEmptyEntries)[0].Trim()
                ?? "Вы слушаете: ... Certified H0OD classic!";

        var task_loadBackground = Image.LoadAsync(File_TestFontsBackground);

        var lines = new List<Image>();

        foreach (var (key, suffix) in GetAllFontCodes()) // draw lines
        {
            _op = new FontOption(key, suffix);
            var fontFamily = _op.GetFontFamily();
            var fontStyle  = _op.GetFontStyle(fontFamily);
            var fontSize   = _op.GetSizeMultiplier() * FONT_SIZE;
            _font = fontFamily.CreateFont(fontSize, fontStyle);

            var code = suffix is null ? key : $"{key}-{suffix}";
            var image = new Image<Rgba32>(W, H_LINE);
            DrawText(image, code, offsetX: MARGIN_X);
            DrawText(image, text, offsetX: MARGIN_X + W_CODE);
            lines.Add(image);
        }

        var canvas = await task_loadBackground; // compose
        var y = MARGIN_Y;
        foreach (var line in lines)
        {
            canvas.Mutate(ctx => ctx.DrawImage(line, new Point(0, y)));
            y += line.Height;
        }

        var path = GetTempFileName("jpg");
        await ImageSaver.SaveImageJpeg(canvas, path, 85);

        SendFile(path, MediaType.Photo);
        Log($"{Title} >> FONTS | {text}");
    }

    private static IEnumerable<(string key, string? suffix)> GetAllFontCodes() => FontStorage.Families
        .OrderBy(pair => pair.Key) // aa-zz
        .SelectMany(pair =>
        {
            var (key, family) = pair;
            var styles = family.GetAvailableStyles().ToHashSet();
            return styles
                .OrderBy(style => (int)style % 2 * 2 + (int)style) // _, i, b, bi | 0, 2, 1, 3
                .Select(style =>
                {
                    var suffix = (string?)null;
                    if (styles.Count > 1) // some fonts has only Bold style (treat as Regular).
                    {
                        var b = style.HasFlag(FontStyle.Bold);
                        var i = style.HasFlag(FontStyle.Italic);
                        if (b && styles.Count > 2) suffix = $"{suffix}b";
                        if (i)                     suffix = $"{suffix}i";
                    }

                    return (key, suffix);
                });
        });

    // DRAW

    private static readonly SolidBrush _brush = new(Color.Black);

    private Font     _font = null!;
    private FontOption _op = null!;

    private float _fontOffset, _caseOffset, _textOffset;

    private void DrawText(Image<Rgba32> image, string text, int offsetX)
    {
        var emoji = EmojiTool.FindEmoji(text);
        var plain = emoji.Count == 0;

        _fontOffset = _font.Size * _op.GetFontDependentOffset();
        _caseOffset = _font.Size * _op.GetCaseDependentOffset(text);
        _textOffset = _fontOffset - _caseOffset;

        var options = GetDefaultTextOptions(offsetX);

        if (plain)
        {
            image.Mutate(x => x.DrawText(options, text, _brush, pen: null));
        }
        else
        {
            var pngs = EmojiTool.GetEmojiPngs(emoji).AsQueue();
            var optionsE = new EmojiTool.Options(_brush, W, GetEmojiSize(), _fontOffset);
            var textLayer = EmojiTool.DrawEmojiText(text, options, optionsE, pngs);
            image.Mutate(ctx => ctx.DrawImage(textLayer, GetOriginFunny(offsetX, textLayer.Size)));
        }
    }

    private RichTextOptions GetDefaultTextOptions(int offsetX) => new(_font)
    {
        TextAlignment = TextAlignment.Start,
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Center,
        Origin = GetTextOrigin(offsetX),
        WrappingLength = -1,
        LineSpacing = GetLineSpacing(),
        FallbackFontFamilies = _op.GetFallbackFamilies(),
    };

    private PointF GetTextOrigin(int x)
    {
        var y = H_LINE / 2F + _textOffset;
        return new PointF(x, y);
    }

    private Point GetOriginFunny(int x, Size textLayer)
    {
        var y = H_LINE.Gap(textLayer.Height) - _caseOffset;
        return new Point(x, y.RoundInt());
    }

    private int GetEmojiSize() => (int)(_font.Size * GetLineSpacing());

    private float GetLineSpacing() => _op.GetLineSpacing() * 1.2F;
}