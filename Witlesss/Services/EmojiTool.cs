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

        public int DrawTextAndEmoji
        (
            Image img,
            string text,
            IList<Match> matches,
            TextOptions ops,
            SolidBrush color,
            RectangleF layout,
            int emojiS,
            int maxLines,
            int m = 0, int m2 = 34, int off = 0
        )
        {
            var lines = 0;
            if (maxLines > 1 && text.Contains('\n'))
            {
                var s = Dg ? text.Split('\n') : text.Split('\n', 2);
                var index1 = off + s[0].Length;
                var index2 = off + s[0].Length + 1 + s[1].Length;
                var matchesA = matches.Where(u => u.Index < index1).ToList();
                var matchesB = matches.Where(u => u.Index > index1 && u.Index < index2).ToList();
                lines += DrawTextAndEmoji(img, s[0], matchesA, ops, color, layout, emojiS, maxLines, m,              m2, off);
                lines += DrawTextAndEmoji(img, s[1], matchesB, ops, color, layout, emojiS, maxLines, m + m2 * lines, m2, index1 + 1);

                return lines;
            }

            var texts = EmojiRegex.Replace(text, "\t").Split('\t');
            var emoji = GetEmojiPngs(matches);
            var w = (int)layout.Width;
            var h = (int)layout.Height;

            using var textArea = new Image<Rgba32>(w, h); // graphics

            //textArea.CompositingMode    = CompositingMode.SourceOver;
            //textArea.CompositingQuality = CompositingQuality.HighQuality;
            //textArea.PixelOffsetMode    = PixelOffsetMode.HighQuality;
            //textArea.TextRenderingHint  = TextRenderingHint.AntiAlias;

            int x = 0, y = 0, max = 0;

            for (var i = 0; i < emoji.Count; i++)
            {
                DoText(texts[i]);

                for (var j = 0; j < emoji[i].Count; j++)
                {
                    var xd = emoji[i][j];
                    if (emojiS + x > w)
                    {
                        if (Dg) break;
                        else     CR();
                    }

                    if (xd.EndsWith(".png"))
                    {
                        var image = Image.Load<Rgba32>(new DecoderOptions() { TargetSize = new Size(emojiS, emojiS) }, xd);
#if DEBUG
                        graphics.FillRectangle(new SolidBrush(Color.Gold), new Rectangle(new Point(x, y), emojiSize));
#endif
                        textArea.Mutate(ctx => ctx.DrawImage(image, new Point(x, y), new GraphicsOptions()));
                        MoveX(emojiS);
                    }
                    else DoText(xd);
                }
            }
            DoText(texts[^1]);

            RenderLine();

            return lines + 1;

            void DoText(string s)
            {
                var rest = w - x;
                if (rest == 0)
                {
                    CR();
                    DoText(s);
                }
                else
                {
                    s = s.TrimEnd();
                    
                    var optionsW = new RichTextOptions(ops.Font)
                    {
                        WrappingLength = w,
                        LineSpacing = ops.LineSpacing,
                    };
                    var ms = TextMeasuringHelpers.MeasureTextSize(s, optionsW, out var l);
                    //var ms = graphics.MeasureString(s, p.Font, p.Layout.Size, Formats[2], out _,  out var l);
                    var width = l > 1 ? rest : (int) Math.Min(ms.Width, rest);

                    if (width < rest) DrawSingleLineText(/*Formats[0]*/);
                    else if      (Dg) DrawSingleLineText(/*Formats[3]*/);
                    else
                    {
                        //var format = Formats[1];
                        var layoutR = new RectangleF(x, y, rest, h);
                        var optionsR = new RichTextOptions(ops.Font)
                        {
                            Origin = new Point(x, y),
                            WrappingLength = rest,
                            LineSpacing = ops.LineSpacing,
                        };
                        _ = TextMeasuringHelpers.MeasureTextSizeSingleLine(s, optionsR, out var chars); // w - x
                        _ = TextMeasuringHelpers.MeasureTextSizeSingleLine(s, optionsW, out var cw); // w
                        var start = (int)(Math.Max(0.66f - x / (float)w, 0) * cw);
                        var space = s[start..cw].Contains(' ');
                        var index = s[..chars].LastIndexOf(' ');
                        var cr = index < 0;
                        var trim = space ? cr ? "" : s[..index] : s[..chars];
                        ms = TextMeasuringHelpers.MeasureTextSize(trim, optionsR, out _);
                        //layoutR.Width = ms.Width;
                        optionsR.WrappingLength = ms.Width;
#if DEBUG
                        graphics.FillRectangle(new SolidBrush(Color.Crimson), layoutR);
#endif
                        textArea.Mutate(ctx => ctx.DrawText(optionsR, trim, color));
                        MoveX((int)TextMeasuringHelpers.MeasureTextSize(trim, optionsR, out _).Width);
                        var next = space ? cr ? s : s[(index + 1)..] : s[chars..];
                        CR();
                        DoText(next);
                    }

                    void DrawSingleLineText(/*StringFormat format*/)
                    {
                        //var layout = new RectangleF(x, y, width, h);
#if DEBUG
                        graphics.FillRectangle(new SolidBrush(Color.Chocolate), layout);
#endif
                        optionsW.Origin = new Point(x, y);
                        textArea.Mutate(ctx => ctx.DrawText(optionsW, s, color));
                        //graphics.DrawString(s, p.Font, p.Color, layout, format);
                        MoveX(width);
                    }
                }
            }
            void MoveX(int o)
            {
                x += o;
                max = Math.Max(x, max);
            }
            void CR()
            {
                RenderLine();

                x = 0;
                max = 0;
                y += m2;
                lines++;
            }

            void RenderLine()
            {
                var offset = /*Top && IFunnyApp.UseLeftAlignment ? (int)p.Layout.X :*/ (w - max) / 2;
                img.Mutate(ctx => ctx.DrawImage(textArea, new Point(offset, (int)layout.Y + m), new GraphicsOptions()));
                textArea.Mutate(ctx => ctx.Fill(Color.Transparent));
            }
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