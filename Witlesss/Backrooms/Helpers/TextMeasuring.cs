using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace Witlesss.Backrooms.Helpers;

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

    // bounds   -    position, glyph size
    // advances - no position,  full size

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
        charsFitted = 0;
        var ops = new TextOptions(options).WithDefaultAlignment();

        var advances = MeasureCharacterAdvances(text, options);
        if (advances.Length == 0) return SizeF.Empty;

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

    private static ReadOnlySpan<GlyphBounds> MeasureCharacterAdvances(string text, TextOptions options)
    {
        try
        {
            TextMeasurer.TryMeasureCharacterAdvances(text, options, out var advances);
            return advances;
        }
        catch
        {
            return [];
        }
    }

    public static List<WordMeasurement> MeasureTextSuperCool(string text, TextOptions options, bool cloneOptions = true)
    {
        if (string.IsNullOrEmpty(text))
            return [];


        var ops = cloneOptions 
            ? new TextOptions(options).WithDefaultAlignment().WithoutWrapping() 
            : options;

        if (text.Contains('\n'))
            return MeasureTextSuperCoolMultiline(text, ops);


        TextMeasurer.TryMeasureCharacterAdvances(text, ops, out var advances);

        var list = new List<WordMeasurement>();

        var type = CharType.Text;
        var start = 0;
        var length = 0;
        var width = 0F;

        foreach (var advance in advances)
        {
            Append(advance, advance.Codepoint.Value == 0x20 ? CharType.Spaces : CharType.Text);
        }

        list.Add(new WordMeasurement(start, length, width, type));

        return list;

        // ==

        void Append(GlyphBounds advance, CharType ofType)
        {
            if (type != ofType)
            {
                list.Add(new WordMeasurement(start, length, width, type));
                type = ofType;
                start += length;
                length = 0;
                width = 0F;
            }

            length++;
            width += advance.Bounds.Width;
        }
    }

    private static List<WordMeasurement> MeasureTextSuperCoolMultiline(string text, TextOptions options)
    {
        var list = new List<WordMeasurement>();
        var lines = text.Split('\n');
        foreach (var line in lines)
        {
            list.AddRange(MeasureTextSuperCool(line, options, cloneOptions: false));
            var last = list.Last();
            list.Add(new WordMeasurement(last.Start + last.Length, 1, 0F, CharType.LineBreak));
        }

        list.RemoveAt(list.Count - 1);
        return list;
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

[DebuggerDisplay("{Start} + {Length} : {Width} of {Type}")]
public readonly struct WordMeasurement(int start, int length, float width, CharType type)
{
    public int    Start  { get; } = start;
    public int    Length { get; } = length;
    public float  Width  { get; } = width;
    public CharType Type { get; } = type;
}

public enum CharType
{
    Text,       // can't be broken
    Emoji,      // can   be broken
    Spaces,     // can be used as a line break
    LineBreak   // mandatory        line break
}