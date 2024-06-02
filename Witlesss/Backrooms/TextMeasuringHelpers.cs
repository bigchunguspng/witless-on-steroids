using System.Collections.Generic;
using System.Linq;
using SixLabors.Fonts;
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

    public static int MeasureTextHeight(string text, TextOptions options, out int linesFilled)
    {
        var ops = new RichTextOptions(options.Font)
        {
            WrappingLength = options.WrappingLength
        };
        TextMeasurer.TryMeasureCharacterBounds(text, ops, out var bounds);
        TextMeasurer.TryMeasureCharacterAdvances(text[0].ToString(), ops, out var advances);

        linesFilled = 0;
        for (var i = 0; i < bounds.Length - 1; i++)
        {
            if (bounds[i].Bounds.X > bounds[i + 1].Bounds.X) // line break
            {
                linesFilled++;
            }
        }

        return (linesFilled * advances[0].Bounds.Height).RoundInt();
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