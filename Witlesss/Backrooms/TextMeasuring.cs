using System;
using System.Text.RegularExpressions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace Witlesss.Backrooms;

public static class TextMeasuring
{
    /// <returns>
    /// Index of last character before line break,
    /// or <b>-1</b> if text fits into one line.
    /// </returns>
    public static int DetectLineBreak(string text, TextOptions options, int lines)
    {
        TextMeasurer.TryMeasureCharacterBounds(text, options, out var bounds);
        TextMeasurer.TryMeasureCharacterAdvances(text, options, out var advances);
        var line = 0;
        for (var i = 0; i < bounds.Length - 1; i++)
        {
            if (bounds[i].Bounds.X > bounds[i + 1].Bounds.X) // line break
            {
                if (++line == lines) return bounds[i + 1].StringIndex;
            }
        }

        return -1;
    }

    public static SizeF MeasureTextSize(string text, TextOptions options, out int linesFilled)
    {
        linesFilled = 0;

        if (string.IsNullOrEmpty(text)) return Size.Empty;

        var ops = new TextOptions(options).WithDefaultAlignment();

        TextMeasurer.TryMeasureCharacterBounds(text, ops, out var bounds);
        TextMeasurer.TryMeasureCharacterAdvances(text, ops, out var advances);

        var maxWidth = 0F;
        var currentWidth = 0F;

        for (var i = 0; i < bounds.Length; i++)
        {
            currentWidth += advances[i].Bounds.Width;

            var endOfText = i + 1 == bounds.Length;
            var endOfLine = !endOfText && bounds[i].Bounds.X > bounds[i + 1].Bounds.X;
            if (endOfText || endOfLine)
            {
                linesFilled++;
                maxWidth = Math.Max(maxWidth, currentWidth);
                currentWidth = 0F;
            }
        }

        linesFilled += Regex.Count(text, @"\n(?=[ \t]*\n)");

        var fontHeight = advances[0].Bounds.Height;
        var textHeight = fontHeight * linesFilled;
        //var lineSpacings = fontHeight * (ops.LineSpacing - 1) * (linesFilled - 1);
        return new SizeF(maxWidth, textHeight /*+ lineSpacings*/);
    }

    public static SizeF MeasureTextSizeSingleLine(string text, TextOptions options, out int charsFitted)
    {
        var ops = new TextOptions(options).WithDefaultAlignment();

        TextMeasurer.TryMeasureCharacterAdvances(text, ops, out var advances);

        var width = 0F;
        charsFitted = text.Length;

        foreach (var advance in advances)
        {
            var newWidth = width + advance.Bounds.Width;
            if (newWidth > ops.WrappingLength)
            {
                charsFitted = advance.StringIndex;
                break;
            }

            width = newWidth;
        }

        var textHeight = advances[0].Bounds.Height;
        return new SizeF(width, textHeight * ops.LineSpacing);
    }

    // it's here for the future...
    /*public static List<TextMeasurement> GetTextGlyphBounds(string text, TextOptions options)
    {
        var lines = text.Split('\n');
        return lines.Select(line => GetLineGlyphBounds(line, options)).ToList();
    }

    public static TextMeasurement GetLineGlyphBounds(string text, TextOptions options)
    {
        var ops = new TextOptions(options);
        TextMeasurer.TryMeasureCharacterAdvances(text, ops, out var advances);

        var result = new List<WordMeasurement>();
        var start = 0;
        var width = 0F;
        for (var i = 0; i <= advances.Length; i++)
        {
            if (i == advances.Length)
            {
                result.Add(new WordMeasurement(text.Substring(start), width));
                break;
            }

            var advance = advances[i];
            var index = advance.StringIndex;
            if (text[index] == ' ')
            {
                var length = index - start;
                result.Add(new WordMeasurement(text.Substring(start, length), width));
                start = index + 1;
                width = 0F;

                result.Add(new WordMeasurement(text[index].ToString(), advance.Bounds.Width));
            }
            else
            {
                width += advance.Bounds.Width;
            }
        }

        return new TextMeasurement(result, advances[0].Bounds.Height);
    }

    public class TextMeasurement(List<WordMeasurement> words, float height)
    {
        public float MaxHeight { get; } = height;
        public List<WordMeasurement> Words { get; } = words;
    }

    public record WordMeasurement(string Word, float Width);*/
}