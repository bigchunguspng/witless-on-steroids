using System;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using Witlesss.Backrooms.Helpers;
using Witlesss.Memes.Shared;

namespace Witlesss.Memes;

public partial class MemeGenerator
{
    public static readonly ExtraFonts ExtraFonts = new("meme");


    private float _startingFontSize;

    private static Font _font = default!;
    private static FontFamily _fontFamily;
    private static FontStyle  _fontStyle;

    private static float FontSize => _font.Size;

    private void SetUpFonts()
    {
        _fontFamily = ExtraFonts.GetFontFamily("im");
        _fontStyle = ExtraFonts.GetFontStyle(_fontFamily);

        _startingFontSize = GetStartingFontSize();
        ResizeFont(_startingFontSize);

        _offsetY = FontSize * ExtraFonts.GetVerticalOffset();
    }

    private void ResizeFont(float size) => _font = _fontFamily.CreateFont(size, _fontStyle);

    private float GetStartingFontSize()
    {
        var defaultFontSize = Math.Min(_w, 1.5F * _h) / 10F;
        var multiplier = FontMultiplier / 10F;
        return Math.Max(defaultFontSize * multiplier, 15) * ExtraFonts.GetSizeMultiplier();
    }


    private string MakeTextFitCard(string text)
    {
        var textChunks = TextMeasuring.MeasureTextSuperCool(text, GetDefaultTextOptions(_marginY), GetEmojiSize());

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
                var areaRatio = textWidthLimit / textHeightLimit;
                var ratioFix = areaRatio <= 3 ? 0F : areaRatio <= 4 ? 0.5F : 1F; // fixes small text for wide pics
                var lineCount = 2;
                while (true) // calculate line count
                {
                    var textRatio = (textWidth / lineCount) / (lineHeight * lineCount);
                    var targetRatio = textWidthLimit / (textHeightLimit * (Math.Min(lineCount, 4) + ratioFix) / 6F);
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

    private RichTextOptions GetDefaultTextOptions(int y) => new(_font)
    {
        TextAlignment = TextAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = y == _marginY ? VerticalAlignment.Top : VerticalAlignment.Bottom,
        Origin = new PointF(_w / 2F, (y == _marginY ? _marginY * 1.5F : y) + _offsetY),
        WrappingLength = _w,
        LineSpacing = GetLineSpacing(),
        FallbackFontFamilies = ExtraFonts.FallbackFamilies
    };

    private float GetLineSpacing() => ExtraFonts.GetLineSpacing();
}