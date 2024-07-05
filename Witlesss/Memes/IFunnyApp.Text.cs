﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using Witlesss.Backrooms.Helpers;
using Witlesss.Memes.Shared;

namespace Witlesss.Memes;

public partial class IFunnyApp
{
    private static readonly Regex CAPS = new(@"[A-ZА-Я0-9bdfhkltбф]");
    public  static readonly ExtraFonts ExtraFonts = new("top");


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

        if (text.Contains('\n') || !WrapText) // ww
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
                if (ThinCard) SetCardHeightLol(lineHeight, 1F);
                return text; // OK - don't change anything!
            }

            var maxWordWidth = textChunks.GetMaxWordWidth();
            if (maxWordWidth > textWidthLimit) k = textWidthLimit / maxWordWidth;

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
            SetCardHeightLol(textHeight, k);
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

    private void SetCardHeightLol(float textHeight, float k)
    {
        var k2 = UltraThinCard ? 0.1F : 1F;
        var min = UltraThinCard ? 8 : 16;
        var extra = Math.Max(FontSize * k * k2, min);
        SetCardHeight((textHeight * k + extra).CeilingInt());
    }


    private void AdjustTextPosition(string s)
    {
        var offset = FontSize * ExtraFonts.GetVerticalOffset();
        var caps = TextIsUppercaseEnough(s) ? FontSize * 0.103F : 0; // todo different for every font

        _textOffset = offset; // offset + caps;

        Log($"/top >> font size: {FontSize:F2}", ConsoleColor.DarkYellow);
    }

    private bool TextIsUppercaseEnough(string s)
    {
        var caps = CAPS.Matches(s).Count;
        var emoji = EmojiRegex.Matches(s).Sum(m => m.Length);
        return caps + 3 * emoji > s.Length / 5;
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

    private float GetLineSpacing() => ExtraFonts.GetLineSpacing() * 1.2F;

    private PointF GetTextOrigin()
    {
        var x = UseLeftAlignment ? _marginLeft : _w / 2F;
        var y = _cardHeight / 2F + _textOffset;

        return new PointF(x, y);
    }
}