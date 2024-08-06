using System;
using System.Linq;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using Witlesss.Backrooms.Helpers;
using Witlesss.Memes.Shared;

namespace Witlesss.Memes;

public partial class IFunnyApp
{
    public static readonly ExtraFonts ExtraFonts = new("top");

    private static Font _font = default!;
    private static FontFamily _fontFamily;
    private static FontStyle  _fontStyle;

    public static float FontSize => _font.Size;

    private void SetUpFonts()
    {
        _fontFamily = ExtraFonts.GetFontFamily(PreferSegoe ? "sg" : "ft");
        _fontStyle = ExtraFonts.GetFontStyle(_fontFamily);

        ResizeFont(GetStartingFontSize());
    }

    private void ResizeFont(float size) => _font = _fontFamily.CreateFont(size, _fontStyle);

    private float GetStartingFontSize()
    {
        var defaultFontSize = Math.Max(48, _cardHeight / 3.75F);
        var multiplier = FontSizeMultiplier / 10F;
        return Math.Max(defaultFontSize * multiplier, MinFontSize) * ExtraFonts.GetSizeMultiplier();
    }


    /// <summary>
    /// Does the following things if there is a need:
    /// <li>Changes font size and card height.</li>
    /// <li>Redistributes the text.</li>
    /// </summary>
    private string MakeTextFitCard(string text)
    {
        var textChunks = TextMeasuring.MeasureTextSuperCool(text, GetDefaultTextOptions(), GetEmojiSize());

        var lineHeight = FontSize * GetLineSpacing();
        var textWidthLimit = 0.9F * _w;

        var k = 1F;
        float textHeight;

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

            textHeight = lineHeight * text.GetLineCount();
        }
        else
        {
            var textWidth = textChunks.Sum(x => x.Width);
            if (textWidth < textWidthLimit)
            {
                if (ThinCard) SetCardHeightThin(lineHeight, 1F);
                return text; // OK - don't change anything!
            }

            var maxWordWidth = textChunks.GetMaxWordWidth();
            if (maxWordWidth > textWidthLimit) k = textWidthLimit / maxWordWidth;
            if (maxWordWidth / textWidth > 0.75F) // if biggest word makes > 75% of the caption
            {
                k = textWidthLimit / textWidth;
            }

            if (textWidth * k > textWidthLimit)
            {
                var minRatio = GetMinTextRatio(textWidth);
                var lineCount = 2;
                while (true) // calculate line count
                {
                    var textRatio = (textWidth / lineCount) / (lineHeight * lineCount);
                    var targetRatio = Math.Min(minRatio, textWidthLimit / (_cardHeight * Math.Min(lineCount, 4) / 6F));
                    if (textRatio < targetRatio) break;

                    lineCount++;
                }

                textChunks.RedistributeText(lineCount); // lineCount: 2+
                text = textChunks.FillWith(text);

                EnsureLongestLineFits();

                textHeight = lineHeight * lineCount;
            }
            else
                textHeight = lineHeight;
        }

        if (ThinCard || textHeight * k > _cardHeight)
        {
            SetCardHeightThin(textHeight, k);
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

    private void SetCardHeightThin(float textHeight, float k)
    {
        var k2 = UltraThinCard ? 0.1F : 1F;
        var min = UltraThinCard ? 8 : 16;
        var extra = Math.Max(FontSize * k * k2, min) * ExtraFonts.GetRelativeSize();
        SetCardHeight((textHeight * k + extra).CeilingInt());
    }

    private RichTextOptions GetDefaultTextOptions() => new(_font)
    {
        TextAlignment = UseLeftAlignment
            ? TextAlignment.Start
            : TextAlignment.Center,
        HorizontalAlignment = UseLeftAlignment
            ? HorizontalAlignment.Left
            : HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        Origin = GetTextOrigin(),
        WrappingLength = _w,
        LineSpacing = GetLineSpacing(),
        FallbackFontFamilies = ExtraFonts.FallbackFamilies,
    };

    private int GetEmojiSize() => (int)(FontSize * GetLineSpacing());

    private float GetLineSpacing() => ExtraFonts.GetLineSpacing() * 1.2F;

    private PointF GetTextOrigin()
    {
        var x = UseLeftAlignment ? _marginLeft : _w / 2F;
        var y = _cardHeight / 2F + _textOffset;
        return new PointF(x, y);
    }

    private Point GetOriginFunny(Size textLayer)
    {
        var x = UseLeftAlignment ? _marginLeft : _w.Gap(textLayer.Width);
        var y = _cardHeight.Gap(textLayer.Height) - _caseOffset;
        return new Point(x.RoundInt(), y.RoundInt());
    }


    // TEXT OFFSET

    private float _fontOffset, _caseOffset, _textOffset;

    private void AdjustTextOffset(string text)
    {
        _fontOffset = FontSize * ExtraFonts.GetFontDependentOffset();
        _caseOffset = FontSize * ExtraFonts.GetCaseDependentOffset(text);
        _textOffset = _fontOffset - _caseOffset;

        Log($"/top >> font size: {FontSize:F2}", ConsoleColor.DarkYellow);
    }
}