using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using Witlesss.Memes.Shared;

namespace Witlesss.Memes;

public partial class MemeGenerator
{
    public static readonly FontWizard FontWizard = new("meme");

    private static Font _font = default!;
    private static FontFamily _fontFamily;
    private static FontStyle  _fontStyle;

    private static float FontSize => _font.Size;

    private void SetUpFonts()
    {
        _fontFamily = FontWizard.GetFontFamily("im");
        _fontStyle = FontWizard.GetFontStyle(_fontFamily);

        ResizeFont(GetStartingFontSize());
    }

    private void ResizeFont(float size)
    {
        _font = _fontFamily.CreateFont(size, _fontStyle);
        _fontOffset = FontSize * FontWizard.GetFontDependentOffset();
    }

    private float GetDefaultFontFize () => Math.Min(_w, 1.5F * _h) / 10F;

    private float GetStartingFontSize()
    {
        var multiplier = FontMultiplier / 100F;
        return Math.Max(GetDefaultFontFize() * multiplier, 15) * FontWizard.GetSizeMultiplier();
    }


    private string MakeTextFitCard(string text)
    {
        var options = GetDefaultTextOptions(PointF.Empty);
        var textChunks = TextMeasuring.MeasureTextSuperCool(text, options, GetEmojiSize());

        var lineHeight = FontSize * GetLineSpacing();

        var textWidthLimit = (float)_captionSize.Width;
        var textHeightLimit = (float)_captionSize.Height;

        var k = 1F;

        var textWidth = textChunks.GetMaxLineWidth();
        var textHeight = lineHeight * text.GetLineCount();
        if (textWidth < textWidthLimit && textHeight < textHeightLimit)
        {
            return text; // OK - don't change anything!
        }

        if (text.Contains('\n') || !WrapText) // ww
        {
            var textRatio = textWidth / textHeight;
            var areaRatio = textWidthLimit / textHeightLimit;
            k = textRatio > areaRatio ? textWidthLimit / textWidth : textHeightLimit / textHeight;
            
            // [if you wanna add code to prevent text being too small, here it goes]
        }
        else // generated / custom text, most cases
        {
            var maxWordWidth = textChunks.GetMaxWordWidth();
            if (maxWordWidth > textWidthLimit) k = textWidthLimit / maxWordWidth;

            if (textWidth * k > textWidthLimit)
            {
                // this fixes small text for wide pictures
                var areaRatio = textWidthLimit / textHeightLimit;
                var ratioFix = areaRatio <= 3 ? 0F : areaRatio >= 5 ? 1F : 0.5F * (areaRatio - 3);

                var lineCount = 1;
                while (true) // calculate line count
                {
                    var textRatio = (textWidth / lineCount) / (lineHeight * lineCount);
                    var targetRatio = textWidthLimit / (textHeightLimit * (Math.Min(lineCount, 4) + ratioFix) / 5F);
                    if (textRatio < targetRatio) break;

                    lineCount++;
                }

                textChunks.RedistributeText(lineCount); // lineCount: 2+
                text = textChunks.FillWith(text);

                var maxLineWidth = textChunks.GetMaxLineWidth();
                if (maxLineWidth * k > textWidthLimit)
                {
                    k = textWidthLimit / maxLineWidth; // fit longest line
                }
            }
        }

        ResizeFont(FontSize * k);

        return text;
    }

    private RichTextOptions GetDefaultTextOptions(PointF origin, bool top = true) => new(_font)
    {
        TextAlignment = TextAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = top ? VerticalAlignment.Top : VerticalAlignment.Bottom,
        Origin = origin,
        WrappingLength = _w,
        LineSpacing = GetLineSpacing(),
        FallbackFontFamilies = FontWizard.GetFallbackFamilies()
    };

    private int GetEmojiSize() => (int)(FontSize * GetLineSpacing());

    private float GetLineSpacing() => FontWizard.GetLineSpacing();

    private PointF GetTextOrigin(string text, bool top, out float caseOffset)
    {
        caseOffset = FontSize * FontWizard.GetCaseDependentOffset(text);
        var marginY = top ? _marginY : _h - _marginY;
        return new PointF(_w / 2F, marginY + _fontOffset - caseOffset);
    }

    private Point GetFunnyOrigin(Size size, RichTextOptions options, bool top, float caseOffset)
    {
        var space = 0.25F * options.Font.Size * options.LineSpacing;
        var marginY = top ? _marginY - space : _h - _marginY - size.Height + space;
        var x = _w.Gap(size.Width);
        var y = marginY - caseOffset;
        return new Point(x.RoundInt(), y.RoundInt());
    }
}