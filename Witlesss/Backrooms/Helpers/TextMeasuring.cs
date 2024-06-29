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

    /// <summary>
    /// Replace all emoji in the text with "👌" before calling this method!
    /// </summary>
    public static List<TextChunk> MeasureTextSuperCool
    (
        string text, TextOptions options, float emojiSize, int start = 0, bool cloneOptions = true
    )
    {
        if (string.IsNullOrEmpty(text))
            return [];


        var ops = cloneOptions 
            ? new TextOptions(options).WithDefaultAlignment().WithoutWrapping() 
            : options;

        if (text.Contains('\n'))
            return MeasureTextSuperCoolMultiline(text, ops, emojiSize);


        TextMeasurer.TryMeasureCharacterAdvances(text, ops, out var advances);

        var list = new List<TextChunk>();

        var type = CharType.Text;
        var length = 0;
        var width = 0F;

        foreach (var advance in advances)
        {
            var chunkType = advance.Codepoint.Value switch
            {
                0x20    => CharType.Spaces,
                0x1F44C => CharType.Emoji,
                _       => CharType.Text
            };
            Append(advance, chunkType);
        }

        list.Add(new TextChunk(start, length, width, type));

        return list;

        // ==

        void Append(GlyphBounds advance, CharType chunkType)
        {
            if (type != chunkType || type == CharType.Emoji)
            {
                list.Add(new TextChunk(start, length, width, type));
                type = chunkType;
                start += length;
                length = 0;
                width = 0F;
            }

            length += advance.Codepoint.Utf16SequenceLength;
            width += chunkType == CharType.Emoji
                ? emojiSize
                : advance.Bounds.Width;
        }
    }

    private static List<TextChunk> MeasureTextSuperCoolMultiline
    (
        string text, TextOptions options, float emojiSize
    )
    {
        var list = new List<TextChunk>();
        var lines = text.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            list.AddRange(MeasureTextSuperCool(lines[i], options, emojiSize, GetStart(), cloneOptions: false));
            if (i + 1 < lines.Length)
            {
                list.Add(new TextChunk(GetStart(), 1, 0F, CharType.LineBreak));
            }
        }

        return list;

        int GetStart() => list.LastOrDefault().GetNextChunkStart();
    }
}

[DebuggerDisplay("{Start} + {Length} : {Width} of {Type}")]
public readonly struct TextChunk(int start, int length, float width, CharType type)
{
    public int    Start  { get; } = start;
    public int    Length { get; } = length;
    public float  Width  { get; } = width;
    public CharType Type { get; } = type;

    public int GetNextChunkStart() => Start + Length;
}

public enum CharType
{
    Text,       // can't be broken (unless it's hella long)
    Emoji,      // can   be broken
    Spaces,     // can be used as a line break
    LineBreak   // mandatory        line break
}