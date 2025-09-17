using System.Diagnostics;
using System.Text;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace PF_Tools.Backrooms.Helpers;

/// Use this to measure and distribute text.
public static class Ruler
{
    /// Returns index of last character before line break,
    /// or <b>-1</b> if text fits into one line.
    public static int DetectLineBreak(string text, TextOptions options, int lines)
    {
        TextMeasurer.TryMeasureCharacterBounds(text, options, out var bounds);
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

        if (text.IsNull_OrEmpty()) return Size.Empty;

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

        linesFilled += _rgx_emptyLine.Count(text);

        var fontHeight = advances[0].Bounds.Height;
        var textHeight = fontHeight * linesFilled;
        //var lineSpacings = fontHeight * (ops.LineSpacing - 1) * (linesFilled - 1);
        return new SizeF(maxWidth, textHeight /*+ lineSpacings*/);
    }

    private static readonly Regex
        _rgx_emptyLine = new(@"\n(?=[ \t]*\n)", RegexOptions.Compiled);

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

    /// Replace all emoji in the text with "👌" before calling this method!
    public static LinkedList<TextChunk> MeasureTextSuperCool
    (
        string text, TextOptions options, float emojiSize, int start = 0, bool cloneOptions = true
    )
    {
        if (text.IsNull_OrEmpty())
            return [];


        var ops = cloneOptions 
            ? new TextOptions(options).WithDefaultAlignment().WithoutWrapping() 
            : options;

        if (text.Contains('\n'))
            return MeasureTextSuperCoolMultiline(text, ops, emojiSize);


        TextMeasurer.TryMeasureCharacterAdvances(text, ops, out var advances);

        var list = new LinkedList<TextChunk>();

        var type = CharType.Text;
        var length = 0;
        var width = 0F;

        foreach (var advance in advances)
        {
            var chunkType = advance.Codepoint.Value switch
            {
                0x20    => CharType.Spaces,
                0x1F44C => CharType.Emoji,
                _       => CharType.Text,
            };
            MoveForward(advance, chunkType);
        }

        list.AddLast(new TextChunk(start, length, width, type));

        return list;

        // ==

        void MoveForward(GlyphBounds advance, CharType chunkType)
        {
            if (type != chunkType || type == CharType.Emoji)
            {
                list.AddLast(new TextChunk(start, length, width, type));
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

    private static LinkedList<TextChunk> MeasureTextSuperCoolMultiline
    (
        string text, TextOptions options, float emojiSize
    )
    {
        var list = new LinkedList<TextChunk>();
        var lines = text.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var chunks = MeasureTextSuperCool(lines[i], options, emojiSize, GetStart(), cloneOptions: false);
            foreach (var chunk in chunks) list.AddLast(chunk);

            if (i + 1 < lines.Length)
            {
                list.AddLast(new TextChunk(GetStart(), 1, 0F, CharType.LineBreak));
            }
        }

        return list;

        int GetStart() => list.LastOrDefault().GetNextChunkStart();
    }

    // GET WIDTH

    public static float GetMaxLineWidth(this LinkedList<TextChunk> chunks)
    {
        var max = 0F;
        var currentLineWidth = 0F;
        foreach (var chunk in chunks)
        {
            if (chunk.Type == CharType.LineBreak)
            {
                max = Math.Max(max, currentLineWidth);
                currentLineWidth = 0F;
            }
            else
                currentLineWidth += chunk.Width;
        }

        return Math.Max(max, currentLineWidth);
    }

    public static float GetMaxWordWidth(this LinkedList<TextChunk> chunks)
    {
        return chunks
            .Where(x => x is { Type: CharType.Text, Length: <= 25 })
            .OrderByDescending(x => x.Width).FirstOrDefault().Width;
    }

    // DISTRIBUTE

    /// Use this with MULTI-LINE text.
    public static void RedistributeText(this LinkedList<TextChunk> chunks, float widthLimit)
    {
        var currentLineWidth = 0F;

        var chunk = chunks.First!;
        while (chunk.Next is not null)
        {
            var limit = currentLineWidth + chunk.Value.Width;
            if (chunk.Value.Type == CharType.LineBreak)
            {
                currentLineWidth = 0F;
            }
            else if (limit >= widthLimit)
            {
                var lineBreak = new TextChunk(0, 0, 0F, CharType.LineBreak);

                if (chunk.Value.Type == CharType.Spaces)
                {
                    chunk.Value = lineBreak;
                    currentLineWidth = 0F;
                }
                else
                {
                    currentLineWidth = chunk.Value.Length > 25
                        ? BreakLongWord(chunks, chunk, lineBreak, widthLimit, currentLineWidth)
                        : BreakBefore(chunks, chunk, lineBreak);
                }
            }
            else
            {
                currentLineWidth = limit;
            }

            chunk = chunk.Next;
        }
    }

    /// Use this with SINGLE LINE text.
    public static void RedistributeText(this LinkedList<TextChunk> chunks, int lines)
    {
        var widthTotal = chunks.Sum(x => x.Width);
        var averageLineWidth = widthTotal / lines;
        var currentLineWidth = 0F;
        var linesFilled = 1;

        var chunk = chunks.First;
        while (linesFilled < lines && chunk is not null)
        {
            var limit = currentLineWidth + chunk.Value.Width;
            if (limit >= averageLineWidth)
            {
                var lineBreak = new TextChunk(0, 0, 0F, CharType.LineBreak);

                if (chunk.Value.Type == CharType.Spaces)
                {
                    chunk.Value = lineBreak;
                    currentLineWidth = 0F;
                }
                else
                {
                    var breakBefore = averageLineWidth - currentLineWidth < limit - averageLineWidth;
                    currentLineWidth = chunk.Value.Length > 25
                        ? BreakLongWord(chunks, chunk, lineBreak, averageLineWidth, currentLineWidth)
                        : breakBefore
                            ? BreakBefore(chunks, chunk, lineBreak)
                            : BreakAfter (chunks, chunk, lineBreak);

                    if (chunk.Next?.Value.Type == CharType.LineBreak) chunk = chunk.Next;
                }

                linesFilled++;

                // fix "too many text on the last line"
                if (lines > 2 && linesFilled == lines - 1)
                {
                    averageLineWidth = 0F;
                    var node = chunks.FindLast(lineBreak)!.Next;
                    while (node is not null)
                    {
                        averageLineWidth += node.Value.Width;
                        node = node.Next;
                    }

                    averageLineWidth /= 2;
                }
            }
            else
            {
                currentLineWidth = limit;
            }

            chunk = chunk.Next;
        }
    }

    // BREAKING

    private static float BreakLongWord
    (
        LinkedList<TextChunk> chunks, LinkedListNode<TextChunk> chunk,
        TextChunk lineBreak,
        float widthLimit, float currentLineWidth
    )
    {
        var c = chunk.Value;

        var  width1 = widthLimit - currentLineWidth;
        var length1 = (c.Length * width1 / c.Width).RoundInt(); // todo replace with more precice method
        var  start2 = c.Start + length1;
        var length2 = c.Length - length1;
        var  width2 = c.Width - width1;

        var part1 = new TextChunk(c.Start, length1, width1, c.Type);
        var part2 = new TextChunk( start2, length2, width2, c.Type);

        chunk.Value = lineBreak;
        chunks.AddBefore(chunk, part1);
        chunks.AddAfter (chunk, part2);

        return 0F;
    }

    private static float BreakBefore
    (
        LinkedList<TextChunk> chunks, LinkedListNode<TextChunk> chunk, TextChunk lineBreak
    )
    {
        if (chunk.Previous?.Value.Type == CharType.Spaces)
            chunk.Previous.Value = lineBreak;
        else
            chunks.AddBefore(chunk, lineBreak);
        return chunk.Value.Width;
    }

    private static float BreakAfter
    (
        LinkedList<TextChunk> chunks, LinkedListNode<TextChunk> chunk, TextChunk lineBreak
    )
    {
        if (chunk.Next?.Value.Type == CharType.Spaces)
            chunk.Next.Value = lineBreak;
        else
            chunks.AddAfter(chunk, lineBreak);
        return 0F;
    }

    //

    public static string FillWith(this LinkedList<TextChunk> chunks, string text)
    {
        var sb = new StringBuilder((text.Length * 1.05F).RoundInt());
        foreach (var chunk in chunks)
        {
            var n = chunk.Type == CharType.LineBreak;
            if (n) sb.Append('\n');
            else   sb.Append(text.Substring(chunk.Start, chunk.Length));
        }

        return sb.ToString();
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
    LineBreak,  // mandatory        line break
}