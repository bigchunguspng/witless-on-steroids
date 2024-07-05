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

        var textWidthLimit = _captionSize.Width;
        var textHeightLimit = _captionSize.Height;

        var k = 1F;

        if (WrapText)
        {
            if (text.Contains('\n'))
            {
                // get closest to target text ratio by changing width in binary search way starting with 0.5*max.w
                throw new NotImplementedException();
            }
            else
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