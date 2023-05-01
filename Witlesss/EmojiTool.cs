using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static System.Drawing.StringAlignment;
using static System.Drawing.StringFormatFlags;
using static System.Drawing.StringTrimming;

namespace Witlesss
{
    public class EmojiTool
    {
        public MemeType MemeType { get; init; }

        private bool Dg => MemeType == MemeType.Dg;

        private static readonly Regex Emoji = new(REGEX_EMOJI);
        private static readonly StringFormat[] Formats = new[]
        {
            new StringFormat(NoWrap) { Alignment = Near, Trimming = None },
            new StringFormat(NoWrap) { Alignment = Near, Trimming = Word },
            new StringFormat(NoWrap) { Alignment = Near, Trimming = EllipsisCharacter }
        };

        public int DrawTextAndEmoji(Graphics g, string text, IList<Match> matches, TextParameters p, int m = 0, int m2 = 34, int off = 0)
        {
            var lines = 0;
            if (p.Lines > 1 && text.Contains('\n'))
            {
                var s = Dg ? text.Split('\n') : text.Split('\n', 2);
                var index1 = s[0].Length;
                var index2 = s[0].Length + 1 + s[1].Length;
                var matchesA = matches.Where(u => u.Index < off + index1).ToArray();
                var matchesB = matches.Where(u => u.Index > off + index1 && u.Index <  off + index2).ToArray();
                lines += DrawTextAndEmoji(g, s[0], matchesA, p, m,              m2, off);
                lines += DrawTextAndEmoji(g, s[1], matchesB, p, m + m2 * lines, m2, off + index1);

                return lines;
            }

            var texts = Emoji.Replace(text, "\t").Split('\t');
            var emoji = GetEmojiPngs(matches);
            var w = (int)p.Layout.Width;
            var h = (int)p.Layout.Height;

            using var textArea = new Bitmap(w, h);
            using var graphics = Graphics.FromImage(textArea);

            graphics.CompositingMode    = CompositingMode.SourceOver;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.PixelOffsetMode    = PixelOffsetMode.HighQuality;
            graphics.TextRenderingHint  = TextRenderingHint.AntiAlias;

            int x = 0, y = 0, max = 0;

            for (var i = 0; i < emoji.Count; i++)
            {
                DoText(texts[i]);

                for (var j = 0; j < emoji[i].Count; j++)
                {
                    var xd = emoji[i][j];
                    if (p.EmojiS + x > w)
                    {
                        if (Dg) break;
                        else     CR();
                    }

                    if (xd.EndsWith(".png"))
                    {
                        var image = new Bitmap(Image.FromFile(xd), p.EmojiSize);
                        graphics.DrawImage(image, x, y);
                        MoveX(p.EmojiS);
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
                var width = (int) Math.Min(graphics.MeasureString(s, p.Font).Width, rest);

                if (width < rest) DrawSingleLineText(Formats[0]);
                else if      (Dg) DrawSingleLineText(Formats[2]);
                else
                {
                    var format = Formats[1];
                    var layout = new RectangleF(x, y, width, h);
                    var ms = graphics.MeasureString(s, p.Font, layout.Size, format, out var chars, out _);
                    var space = s.Contains(' ');
                    var index = s[..chars].LastIndexOf(' ');
                    var cr = index < 0;
                    var trim = space ? cr ? "" : s[..index] : s[..chars];
                    layout.Width = ms.Width;
                    graphics.DrawString(trim, p.Font, p.Color, layout, format);
                    MoveX((int)ms.Width);
                    var next = space ? cr ? s : s[(index + 1)..] : s[chars..];
                    CR();
                    DoText(next);
                }

                void DrawSingleLineText(StringFormat format)
                {
                    var layout = new RectangleF(x, y, width, h);
                    graphics.DrawString(s, p.Font, p.Color, layout, format);
                    MoveX(width);
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
                var offset = IFunnyApp.UseLeftAlignment ? (int)Math.Min(p.Font.Size / 3, 5) : (w - max) / 2;
                g.DrawImage(textArea, new Point(offset, (int)p.Layout.Y + m));
                graphics.Clear(Color.Transparent);
            }
        }

        public static string RemoveEmoji (string text) => ReplaceEmoji(text, "");
        public static string ReplaceEmoji(string text, string nn)
        {
            var matches = Regex.Matches(text, REGEX_EMOJI);
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