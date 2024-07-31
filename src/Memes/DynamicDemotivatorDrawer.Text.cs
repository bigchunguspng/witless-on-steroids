using System;
using System.Linq;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using Witlesss.Backrooms.Helpers;
using Witlesss.Memes.Shared;

namespace Witlesss.Memes;

public partial class DynamicDemotivatorDrawer
{
    public static readonly ExtraFonts ExtraFonts = new("dp");

    private static Font _font = default!;
    private static FontFamily _fontFamily;
    private static FontStyle  _fontStyle;

    public static float FontSize => _font.Size;

    private void SetUpFonts()
    {
        _fontFamily = ExtraFonts.GetFontFamily("sg");
        _fontStyle = ExtraFonts.GetFontStyle(_fontFamily);

        ResizeFont(GetStartingFontSize());
    }

    private void ResizeFont(float size) => _font = _fontFamily.CreateFont(size, _fontStyle);

    private float GetStartingFontSize()
    {
        var defaultFontSize = imageW * 0.1F;
        //var multiplier = FontSizeMultiplier / 10F;
        return Math.Max(defaultFontSize /* * multiplier*/, MinFontSize) * ExtraFonts.GetSizeMultiplier();
    }


    private float _textHeight;

    // LOGIC

    private string MakeTextFitCard(string text)
    {
        var textChunks = TextMeasuring.MeasureTextSuperCool(text, GetDefaultTextOptions(), GetEmojiSize());

        var lineHeight = FontSize * ExtraFonts.GetRelativeSize() * GetLineSpacing();
        var textWidthLimit = 1.1F * imageW;

        var k = 1F;

        if (text.Contains('\n') || !WrapText) // ww OR custom breaks
        {
            EnsureLongestLineFits();

            if (FontSize * k < MinFontSize)
            {
                k = MinFontSize / FontSize;

                var widthLimit = textWidthLimit / k;
                textChunks.RedistributeText(widthLimit);
                text = textChunks.FillWith(text);
            }

            _textHeight = lineHeight * text.GetLineCount();
        }
        else
        {
            var textWidth = textChunks.Sum(x => x.Width);
            if (textWidth * 2F < imageW)
            {
                _textHeight = lineHeight * 2F;
                ResizeFont(FontSize * 2F);
                return text; // Make it bigger!
            }

            if (textWidth < textWidthLimit)
            {
                _textHeight = lineHeight;
                return text; // OK - don't change anything!
            }

            var maxWordWidth = textChunks.GetMaxWordWidth() * k;
            if (maxWordWidth > textWidthLimit) k = textWidthLimit / maxWordWidth;
            if (maxWordWidth / textWidth > 0.75F) // if the biggest word makes > 75% of the caption
            {
                k = textWidthLimit / textWidth;
            }

            var lineCount = 1;

            if (textWidth * k > textWidthLimit)
            {
                var minRatio = GetMinTextRatio(textWidth);
                while (true) // calculate line count
                {
                    var textRatio = (textWidth / lineCount) / (lineHeight * lineCount);
                    var targetRatio = Math.Min(minRatio, textWidthLimit / (imageH * Math.Min(lineCount, 8) / 8F));
                    Log($"lineCount: {lineCount} min: {minRatio:F2} text: {textRatio:F2} target: {targetRatio:F2}", ConsoleColor.DarkCyan);
                    if (textRatio < targetRatio) break;

                    lineCount++;
                }

                if (lineCount > 1)
                {
                    textChunks.RedistributeText(lineCount);
                    text = textChunks.FillWith(text);
                }

                EnsureLongestLineFits();
            }

            _textHeight = k * lineHeight * lineCount;
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

        float GetMinTextRatio(float textWidth)
        {
            var fontRatio = MinFontSize / FontSize;
            var lineHeightK = fontRatio * lineHeight;
            var textWidthK = fontRatio * textWidth;
            var lineCountK = textWidthK / textWidthLimit;
            return textWidthLimit / (lineHeightK * lineCountK);
        }
    }

    private RichTextOptions GetDefaultTextOptions(/*float width, float height*/) => new(_font)
    {
        TextAlignment = TextAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        Origin = GetTextOrigin(),
        //WrappingLength = width,
        LineSpacing = GetLineSpacing(),
        FallbackFontFamilies = ExtraFonts.FallbackFamilies,
    };

    private PointF GetTextOrigin()
    {
        var occupied = imageH + marginTop + FM;
        var x = fullW / 2F;
        var y = occupied + (fullH - occupied) / 2F + _textOffset;
        return new PointF(x, y);
    }

    private float GetLineSpacing() => ExtraFonts.GetLineSpacing() * 1.2F;

    private int GetEmojiSize() => (int)(FontSize * GetLineSpacing());


    // TEXT OFFSET

    private float _fontOffset, _caseOffset, _textOffset;

    private void AdjustTextOffset(string text)
    {
        _fontOffset = FontSize * ExtraFonts.GetFontDependentOffset();
        _caseOffset = FontSize * ExtraFonts.GetCaseDependentOffset(text);
        _textOffset = _fontOffset - _caseOffset;

        Log($"/dp >> font size: {FontSize:F2}", ConsoleColor.DarkYellow);
    }
}