using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Witlesss.Services
{
    public class EmojiTool
    {
        public MemeType MemeType { get; init; }

        private bool Dg  => MemeType == MemeType.Dg;
        private bool Top => MemeType == MemeType.Top;

        /*private static readonly StringFormat[] Formats = new[]
        {
            new StringFormat(NoWrap) { Alignment = Near, Trimming = None },
            new StringFormat(NoWrap) { Alignment = Near, Trimming = Character },
            new StringFormat(      ) { Alignment = Near, Trimming = Character },
            new StringFormat(NoWrap) { Alignment = Near, Trimming = EllipsisCharacter }
        };*/

        public record Options(SolidBrush Color, float EmojiSize, int MaxLines = -1);

        public Image<Rgba32> DrawEmojiText(string text, RichTextOptions options, Options parameters, out int linesFilled)
        {
            // RENDER EACH PARAGRAPH

            var paragraphs = text.Split('\n');
            var lines = paragraphs
                .Select(paragraph => DrawEmojiTextParagraph(paragraph, options, parameters))
                .SelectMany(x => x).ToList();
            linesFilled = lines.Count;

            // COMBINE

            var width = options.WrappingLength.CeilingInt();
            var height = options.Font.Size * options.LineSpacing;

            var canvas = new Image<Rgba32>(width, (lines.Count * height).RoundInt());

            var offsetY = 0F;
            foreach (var line in lines)
            {
                var x = options.HorizontalAlignment switch
                {
                    HorizontalAlignment.Center => (width - line.Width) / 2,
                    HorizontalAlignment.Right  =>  width - line.Width,
                    _ => 0
                };
                var point = new Point(x, offsetY.RoundInt());
                canvas.Mutate(ctx => ctx.DrawImage(line, point, opacity: 1));
                offsetY += height;
                line.Dispose();
            }

            return canvas;
        }

        private List<Image<Rgba32>> DrawEmojiTextParagraph(string paragraph, RichTextOptions options, Options parameters)
        {
            var  textChunks = EmojiRegex.Replace(paragraph, "\t").Split('\t');
            var emojiChunks = GetEmojiPngs(EmojiRegex.Matches(paragraph));

            var lines = new List<Image<Rgba32>>();

            var width = options.WrappingLength.CeilingInt();
            var height = (options.Font.Size * options.LineSpacing).CeilingInt();

            var canvas = GetEmptyCanvas();

            int x = 0, max = 0;

            for (var i = 0; i < emojiChunks.Count; i++)
            {
                DrawText ( textChunks[i]);
                DrawEmoji(emojiChunks[i]);
            }
            DrawText(textChunks[^1]);

            RenderLine();

            return lines;


            // == FUN ==

            void DrawEmoji(List<string> sequence)
            {
                var side = parameters.EmojiSize.RoundInt();
                var size = new Size(side, side);
                var decoder = new DecoderOptions() { TargetSize = size }; // todo quality?

                foreach (var emoji in sequence)
                {
                    if (side + x > width)
                    {
                        if (Dg) break;
                        else     NewLine();
                    }

                    if (emoji.EndsWith(".png"))
                    {
                        var image = Image.Load<Rgba32>(decoder, emoji);
#if DEBUG
                        canvas.Mutate(ctx => ctx.Fill(Color.Gold, new Rectangle(GetDrawingOffset(), size)));
#endif
                        canvas.Mutate(ctx => ctx.DrawImage(image, GetDrawingOffset(), new GraphicsOptions()));
                        MoveX(side);
                    }
                    else DrawText(emoji);
                }
            }

            void DrawText(string text) // todo make emoji drawer a transient class with methods ?
            {
                var rest = width - x;
                if (rest == 0)
                {
                    NewLine();
                    DrawText(text);
                }
                else
                {
                    //text = text.TrimEnd();
                    
                    var optionsW = new RichTextOptions(options)
                    {
                        WrappingLength = width
                    }.WithDefaultAlignment();
                    var ms = TextMeasuring.MeasureTextSize(text, optionsW, out var linesFilled);
                    var w = linesFilled > 1 ? rest : (int) Math.Min(ms.Width, rest);

                    if (w < rest) DrawSingleLineText();
                    else if  (Dg) DrawSingleLineText();
                    else
                    {
                        var optionsR = new RichTextOptions(options)
                        {
                            Origin = GetDrawingOffset(), WrappingLength = rest
                        }.WithDefaultAlignment();
                        _ = TextMeasuring.MeasureTextSizeSingleLine(text, optionsR, out var chars); // w - x
                        _ = TextMeasuring.MeasureTextSizeSingleLine(text, optionsW, out var cw); // w
                        var start = (int)(Math.Max(0.66f - x / (float)width, 0) * cw);
                        var space = text[start..cw].Contains(' ');
                        var index = text[..chars].LastIndexOf(' ');
                        var cr = index < 0;
                        var trim = space ? cr ? "" : text[..index] : text[..chars];
                        ms = TextMeasuring.MeasureTextSize(trim, optionsR, out _);
                        optionsR.WrappingLength = ms.Width + 0.05F; // safe space, fixes text being not rendered.
#if DEBUG
                        canvas.Mutate(ctx => ctx.Fill(Color.Crimson, new Rectangle(x, 0, rest, height)));
#endif
                        canvas.Mutate(ctx => ctx.DrawText(optionsR, trim, parameters.Color));
                        MoveX((int)TextMeasuring.MeasureTextSize(trim, optionsR, out _).Width);
                        var next = space ? cr ? text : text[(index + 1)..] : text[chars..];
                        NewLine();
                        DrawText(next);
                    }

                    void DrawSingleLineText()
                    {
#if DEBUG
                        canvas.Mutate(ctx => ctx.Fill(Color.Chocolate, new Rectangle(x, 0, w, height)));
#endif
                        optionsW.Origin = GetDrawingOffset();
                        canvas.Mutate(ctx => ctx.DrawText(optionsW, text, parameters.Color));
                        MoveX(w);
                    }
                }
            }

            void MoveX(int offset)
            {
                x += offset;
                max = Math.Max(x, max);
            }

            void NewLine()
            {
                RenderLine();

                x = 0;
                max = 0;

                canvas = GetEmptyCanvas();
            }

            void RenderLine()
            {
                canvas.Mutate(ctx => ctx.Crop(max, canvas.Height));
                lines.Add(canvas);
            }

            Point GetDrawingOffset() => new(x, 0);
            Image<Rgba32> GetEmptyCanvas() => new(width, height);
        }

        public static string RemoveEmoji (string text) => ReplaceEmoji(text, "");
        public static string ReplaceEmoji(string text, string nn)
        {
            var matches = EmojiRegex.Matches(text);
            if (matches.Count == 0) return text;

            var emoji = GetEmojiPngs(matches);
            var m = 0;
            foreach (var cluster in emoji)
            {
                var replaced = cluster.Select(xd => xd.EndsWith(".png") ? nn : xd);
                text = text.Replace(matches[m++].Value, string.Join("", replaced));
            }

            return text;
        }

        private static List<List<string>> GetEmojiPngs(IList<Match> matches)
        {
            var emoji = new List<List<string>>(matches.Count);

            for (var n = 0; n < matches.Count; n++)
            {
                var match = matches[n];
                var xd = match.Value;
                var cluster = new List<string>(xd.Length / 2);
                for (var i = 0; i < xd.Length; i += char.IsSurrogatePair(xd, i) ? 2 : 1)
                {
                    var c = char.ConvertToUtf32(xd, i).ToString("x4");
                    cluster.Add(c);
                }

                emoji.Add(new List<string>(cluster.Count));

                for (var i = 0; i < cluster.Count; i++)
                {
                    var j = i;
                    var name = cluster[i];
                    string file = null;
                    bool repeat;
                    do
                    {
                        repeat = false;

                        var files = Directory.GetFiles(EMOJI_FOLDER, name + "*.png");
                        if (files.Length == 1) file = files[0];
                        else if (files.Length > 1)
                        {
                            file = files[^1];
                            if (cluster.Count > j + 1)
                            {
                                repeat = true;
                                j++;
                                name = name + "-" + cluster[j];
                            }
                        }
                    } while (repeat);

                    if (file != null)
                    {
                        emoji[n].Add(file);
                        var s = Path.GetFileNameWithoutExtension(file);
                        var split = s.Split('-');
                        for (var k = 1; k < split.Length && i + 1 < cluster.Count; k++)
                        {
                            if (split[k] == cluster[i + 1]) i++;
                        }
                    }
                    else
                    {
                        var character = char.ConvertFromUtf32(int.Parse(name, NumberStyles.HexNumber));
                        if (Regex.IsMatch(character, @"[\u231a-\u303d]")) emoji[n].Add(character);
                    }
                }
            }

            return emoji;
        }
    }
}