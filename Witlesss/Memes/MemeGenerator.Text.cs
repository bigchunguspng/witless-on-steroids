using System;
using System.Linq;
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

    public static float FontSize => _font.Size;

    private void SetUpFonts()
    {
        _fontFamily = ExtraFonts.GetFontFamily("im");
        _fontStyle = ExtraFonts.GetFontStyle(_fontFamily);

        _startingFontSize = GetStartingFontSize();
        ResizeFont(_startingFontSize);
    }

    private void ResizeFont(float size) => _font = _fontFamily.CreateFont(size, _fontStyle);

    private float GetStartingFontSize() // todo change maybe
    {
        var minSide = (int)Math.Min(_w, 1.5 * _h);
        var multiplier = FontMultiplier / 10F;
        return Math.Max(minSide * multiplier * ExtraFonts.GetSizeMultiplier() / 10, 15);
    }


    private string MakeTextFitCard(string text)
    {
        var textChunks = TextMeasuring.MeasureTextSuperCool(text, GetDefaultTextOptions(_marginY), GetEmojiSize());

        var lineHeight = FontSize * GetLineSpacing();

        var textWidthLimit = (float)_captionSize.Width;
        var textHeightLimit = (float)_captionSize.Height;

        var k = 1F;

        if (WrapText)
        {
            if (text.Contains('\n')) // custom text only, sometimes
            {
                var textLineCount = text.GetLineCount();
                var textWidth = textChunks.GetMaxLineWidth();
                var textHeight = lineHeight * textLineCount;
                if (textWidth < textWidthLimit && textHeight < textHeightLimit)
                {
                    return text; // OK - don't change anything!
                }

                var textRatio = textWidth / textHeight;
                var areaRatio = textWidthLimit / textHeightLimit;
                k = textRatio > areaRatio ? textWidthLimit / textWidth : textHeightLimit / textHeight;

                if (textRatio > areaRatio * 2.5F)
                {
                    var widths = textChunks.GetLineWidths();

                    float min = 0F, max = 1F, widthLimit = textWidth;

                    while (textRatio / areaRatio is < 0.95F or > 1.15F && max - min > 0.01F)
                    {
                        var avg = (min + max) / 2F;
                        widthLimit = textWidth * avg;
                        var lineCount = widths.Sum(x => Math.Max(1, (x / widthLimit).CeilingInt()));

                        textHeight = lineHeight * lineCount;
                        textRatio = widthLimit / textHeight;

                        var tooWide = textRatio > areaRatio;
                        if (tooWide) max = avg;
                        else         min = avg;
                    }

                    TextMeasuring.RedistributeText(textChunks, widthLimit);
                    text = textChunks.FillWith(text);

                    var maxLineWidth = textChunks.GetMaxLineWidth();
                    textHeight = lineHeight * (textChunks.Count(x => x.Type == CharType.LineBreak) + 1);
                    textRatio = maxLineWidth / textHeight;
                    k = textRatio > areaRatio ? textWidthLimit / maxLineWidth : textHeightLimit / textHeight;
                }
            }
            else // generated / custom text, most cases
            {
                var textWidth = textChunks.Sum(x => x.Width);
                var textHeight = lineHeight * text.GetLineCount();
                if (textWidth < textWidthLimit && textHeight < textHeightLimit)
                {
                    return text; // OK - don't change anything!
                }

                var maxWordWidth = textChunks.GetMaxWordWidth();
                if (maxWordWidth > textWidthLimit) k = textWidthLimit / maxWordWidth;

                if (textWidth * k > textWidthLimit)
                {
                    var lineCount = 2;
                    while (true) // calculate line count
                    {
                        var textRatio = (textWidth / lineCount) / (lineHeight * lineCount);
                        var targetRatio = textWidthLimit / (textHeightLimit * Math.Min(lineCount, 4) / 6F);
                        if (textRatio < targetRatio) break;

                        lineCount++;
                    }

                    TextMeasuring.RedistributeText(textChunks, lineCount); // lineCount: 2+
                    text = textChunks.FillWith(text);

                    EnsureLongestLineFits();
                }
            }
        }
        else // ww
        {
            var textWidth = textChunks.Sum(x => x.Width);
            var textHeight = lineHeight * text.GetLineCount();
            if (textWidth < textWidthLimit && textHeight < textHeightLimit)
            {
                return text; // OK - don't change anything!
            }

            var textRatio = textWidth / textHeight;
            var areaRatio = textWidthLimit / textHeightLimit;
            k = textRatio > areaRatio ? textWidthLimit / textWidth : textHeightLimit / textHeight;
        }

        ResizeFont(FontSize * k);

        return text;

        //

        void EnsureLongestLineFits()
        {
            var maxLineWidth = textChunks.GetMaxLineWidth();
            if (maxLineWidth * k > textWidthLimit)
            {
                k = textWidthLimit / maxLineWidth;
            }
        }
    }

    private RichTextOptions GetDefaultTextOptions(int y) => new(_font)
    {
        TextAlignment = TextAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = y == _marginY ? VerticalAlignment.Top : VerticalAlignment.Bottom,
        Origin = new Point(_w / 2, y),
        WrappingLength = _w,
        LineSpacing = GetLineSpacing(),
        FallbackFontFamilies = ExtraFonts.FallbackFamilies
    };

    private float GetLineSpacing() => ExtraFonts.GetLineSpacing();
}