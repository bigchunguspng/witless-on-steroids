using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using Witlesss.Memes.Shared;

namespace Witlesss.Memes;

public partial class SnapChat
{
    public static readonly FontWizard FontWizard = new("snap");

    private static Font _font = default!;
    private static FontFamily _fontFamily;
    private static FontStyle  _fontStyle;

    private static float _minFontSize;
    private static float FontSize => _font.Size;

    private void SetUpFonts()
    {
        _fontFamily = FontWizard.GetFontFamily("rg");
        _fontStyle = FontWizard.GetFontStyle(_fontFamily);

        ResizeFont(GetStartingFontSize());
    }

    private void ResizeFont(float size) => _font = _fontFamily.CreateFont(size, _fontStyle);

    private float GetStartingFontSize()
    {
        var defaultFontSize = Math.Min(_w, 1.5F * _h) / 10F;
        var multiplier = FontSizeMultiplier / 100F;
        var multiplierM = MinSizeMultiplier / 100F;
        _minFontSize = defaultFontSize * multiplierM;
        return Math.Max(defaultFontSize * multiplier, _minFontSize) * FontWizard.GetSizeMultiplier();
    }

    private string MakeTextFitCard(string text)
    {
        var textChunks = TextMeasuring.MeasureTextSuperCool(text, GetDefaultTextOptions(), GetEmojiSize());

        var lineHeight = FontSize * GetLineSpacing();
        var textWidthLimit = _w - 2 * _marginX;

        var k = 1F;
        float textHeight;

        SetCardHeight(GetHeightWithPadding(lineHeight, 1F));

        if (text.Contains('\n') || !WrapText) // ww OR custom breaks
        {
            EnsureLongestLineFits();

            if (FontSize * k < _minFontSize)
            {
                k = _minFontSize / FontSize;

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
                SetCardHeight(GetHeightWithPadding(lineHeight, 1F));
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

        var height = GetHeightWithPadding(textHeight, k);

        SetCardHeight(height);
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
            var fontRatio = _minFontSize / FontSize;
            var lineHeightK = fontRatio * lineHeight;
            var textWidthK = fontRatio * textWidth;
            var lineCountK = textWidthK / textWidthLimit;
            return textWidthLimit / (lineHeightK * lineCountK);
        }
    }

    private int GetHeightWithPadding(float textHeight, float k)
    {
        var extra = Math.Max(FontSize * k, 16) * FontWizard.GetRelativeSize();
        return (textHeight * k + extra).CeilingInt();
    }

    private RichTextOptions GetDefaultTextOptions() => new(_font)
    {
        TextAlignment = TextAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        Origin = GetTextOrigin(),
        WrappingLength = _w - 2 * _marginX,
        LineSpacing = GetLineSpacing(),
        FallbackFontFamilies = FontWizard.GetFallbackFamilies(),
    };

    private int GetEmojiSize() => (int)(FontSize * GetLineSpacing());

    private float GetLineSpacing() => FontWizard.GetLineSpacing() * 1.2F;

    private PointF GetTextOrigin()
    {
        var x = _w / 2F;
        var y = _offsetY + _textOffset;
        return new PointF(x, y);
    }

    private Point GetOriginFunny(Size textLayer)
    {
        var x = _w.Gap(textLayer.Width);
        var y = _offsetY - 0.5F * textLayer.Height - _caseOffset;
        return new Point(x.RoundInt(), y.RoundInt());
    }


    // TEXT OFFSET

    private float _fontOffset, _caseOffset, _textOffset;

    private void AdjustTextOffset(string text)
    {
        _fontOffset = FontSize * FontWizard.GetFontDependentOffset();
        _caseOffset = FontSize * FontWizard.GetCaseDependentOffset(text);
        _textOffset = _fontOffset - _caseOffset;

        LogDebug($"/snap >> font size: {FontSize:F2}, min: {_minFontSize:F2}");
    }
}