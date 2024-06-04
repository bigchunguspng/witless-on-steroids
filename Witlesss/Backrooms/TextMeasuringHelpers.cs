using System;
using System.Collections.Generic;
using System.Linq;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;

namespace Witlesss.Backrooms;

public static class TextMeasuringHelpers
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

    public static Size MeasureTextSize(string text, TextOptions options, out int linesFilled)
    {
        var ops = new RichTextOptions(options.Font)
        {
            WrappingLength = options.WrappingLength,
            LineSpacing = options.LineSpacing
        };
        TextMeasurer.TryMeasureCharacterBounds(text, ops, out var bounds);
        TextMeasurer.TryMeasureCharacterAdvances(text, ops, out var advances);

        linesFilled = 0;
        var maxWidth = 0F;
        var currentWidth = 0F;

        for (var i = 0; i < bounds.Length; i++)
        {
            if (i + 1 == bounds.Length || bounds[i].Bounds.X > bounds[i + 1].Bounds.X)
            {
                linesFilled++;
                maxWidth = Math.Max(maxWidth, currentWidth);
                currentWidth = 0F;
            }
            else
            {
                currentWidth += advances[i].Bounds.Width;
            }
        }

        return new Size(maxWidth.RoundInt(), (linesFilled * advances[0].Bounds.Height).RoundInt());
    }

    // it's here for the future...
    public static List<TextMeasurement> GetTextGlyphBounds(string text, TextOptions options)
    {
        var lines = text.Split('\n');
        return lines.Select(line => GetLineGlyphBounds(line, options)).ToList();
    }

    public static TextMeasurement GetLineGlyphBounds(string text, TextOptions options)
    {
        var ops = new RichTextOptions(options.Font);
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

    public record WordMeasurement(string Word, float Width);
}