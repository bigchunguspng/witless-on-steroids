using PF_Bot.Backrooms.Helpers;
using PF_Bot.Core.Memes.Shared;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;

namespace PF_Bot.Core.Memes.Generators;

public partial class DynamicDemotivatorDrawer
{
    public static readonly FontWizard FontWizard = new("dp");

    private static Font _font = default!;
    private static FontFamily _fontFamily;
    private static FontStyle  _fontStyle;

    private static float _minFontSize;
    private static float FontSize => _font.Size;

    private void SetUpFonts()
    {
        _fontFamily = FontWizard.GetFontFamily("sg");
        _fontStyle = FontWizard.GetFontStyle(_fontFamily);

        ResizeFont(GetStartingFontSize());
    }

    private void ResizeFont(float size) => _font = _fontFamily.CreateFont(size, _fontStyle);

    private float GetStartingFontSize()
    {
        var defaultFontSize = imageW * 0.1F;
        var multiplier = FontSizeMultiplier / 100F;
        var multiplierM = MinSizeMultiplier / 100F;
        _minFontSize = defaultFontSize * multiplierM;
        return Math.Max(defaultFontSize * multiplier, _minFontSize) * FontWizard.GetSizeMultiplier();
    }


    private float _textHeight;

    // LOGIC

    private string MakeTextFitCard(string text)
    {
        var textChunks = TextMeasuring.MeasureTextSuperCool(text, GetDefaultTextOptions(), GetEmojiSize());

        var lineHeight = FontSize * FontWizard.GetRelativeSize() * GetLineSpacing();
        var textWidthLimit = (Minimalist ? 0.8F : 1.1F) * imageW;

        var k = 1F;
        var lineCount = 1;

        try
        {
            if (text.Contains('\n') || !WrapText) // ww OR custom breaks
            {
                FitLongestLine();

                if (FontSize * k < _minFontSize)
                {
                    k = _minFontSize / FontSize;

                    var widthLimit = textWidthLimit / k;
                    textChunks.RedistributeText(widthLimit);
                    text = textChunks.FillWith(text);
                }

                lineCount = text.GetLineCount();
            }
            else // le "most cases" branch
            {
                var textWidth = textChunks.Sum(x => x.Width);
                if (textWidth * 2F < imageW && !Minimalist && text == text.ToUpper())
                {
                    k = 2;
                    return text; // Make it bigger!
                }

                if (textWidth < textWidthLimit)
                {
                    return text; // OK - don't change anything!
                }

                var maxWordWidth = textChunks.GetMaxWordWidth();
                if (maxWordWidth > textWidthLimit)
                {
                    k = maxWordWidth / textWidth > 0.75F
                        ? textWidthLimit / textWidth // fit all if the biggest word makes > 75% of the caption
                        : textWidthLimit / maxWordWidth; // fit the biggest word
                }

                if (textWidth * k > textWidthLimit) // find the best line count
                {
                    var minRatio = GetMinTextRatio(textWidth);
                    var side = (imageW + 2 * imageH) / 3;
                    var multiplier = FontSizeMultiplier / 100;
                    while (true)
                    {
                        var textRatio = (textWidth / lineCount) / (lineHeight * lineCount) * multiplier;
                        var targetRatio = Math.Min(minRatio, textWidthLimit / (side * Math.Min(lineCount, 10) / 10F));
                        if (textRatio < targetRatio) break;

                        lineCount++;
                    }

                    if (lineCount > 1)
                    {
                        textChunks.RedistributeText(lineCount);
                        text = textChunks.FillWith(text);
                    }

                    FitLongestLine();
                }
            }
        }
        finally
        {
            ResizeFont(FontSize * k);
            _textHeight = k * lineHeight * lineCount * FontWizard.GetSizeMultiplier();
        }

        return text;

        //

        void FitLongestLine()
        {
            var maxLineWidth = textChunks.GetMaxLineWidth();
            if (maxLineWidth * k > textWidthLimit)
            {
                k = textWidthLimit / maxLineWidth;
            }
        }

        float GetMinTextRatio(float textWidth)
        {
            var fontRatio = _minFontSize / FontSize;
            var lineHeightK = fontRatio * lineHeight;
            var textWidthK = fontRatio * textWidth;
            var lineCountK = textWidthK / textWidthLimit;
            return textWidthLimit / (lineHeightK * lineCountK);
        }
    }

    private RichTextOptions GetDefaultTextOptions() => new(_font)
    {
        TextAlignment = TextAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        Origin = GetTextOrigin(),
        WrappingLength = fullW,
        LineSpacing = GetLineSpacing(),
        FallbackFontFamilies = FontWizard.GetFallbackFamilies(),
    };

    private int GetEmojiSize() => (int)(FontSize * GetLineSpacing());

    private float GetLineSpacing() => FontWizard.GetLineSpacing() * 1.2F;

    private PointF GetTextOrigin()
    {
        var occupied = imageH + marginTop + FRAME_MARGIN;
        var x = fullW / 2F;
        var y = occupied + (fullH - occupied) / 2F + _textOffset;
        return new PointF(x, y);
    }

    private Point GetOriginFunny(Size size)
    {
        var occupied = imageH + marginTop + FRAME_MARGIN;
        var x = fullW.Gap(size.Width);
        var y = occupied + (fullH - occupied) / 2F - size.Height / 2F - _caseOffset;
        return new Point(x.RoundInt(), y.RoundInt());
    }


    // TEXT OFFSET

    private float _fontOffset, _caseOffset, _textOffset;

    private void AdjustTextOffset(string text)
    {
        _fontOffset = FontSize * FontWizard.GetFontDependentOffset();
        _caseOffset = FontSize * FontWizard.GetCaseDependentOffset(text);
        _textOffset = _fontOffset - _caseOffset;

        LogDebug($"/dp >> font size: {FontSize:F2}");
    }
}