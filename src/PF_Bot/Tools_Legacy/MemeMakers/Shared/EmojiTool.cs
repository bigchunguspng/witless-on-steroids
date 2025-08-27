using System.Globalization;
using PF_Bot.Backrooms.Helpers;
using PF_Bot.Backrooms.Types;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PF_Bot.Tools_Legacy.MemeMakers.Shared
{
    public static class EmojiTool
    {
        public record Options
        (
            SolidBrush Color,
            int Width,
            float EmojiSize,
            float FontOffset = 0,
            int MaxLines = -1,
            bool Pixelate = false
        );

        public static Image<Rgba32> DrawEmojiText
        (
            string text, RichTextOptions rto, Options options, Queue<string> pngs
        )
        {
            // RENDER EACH PARAGRAPH

            var maxLines = options.MaxLines < 1 ? int.MaxValue : options.MaxLines;
            var paragraphs = text.Split('\n');
            var lines = paragraphs
                .Select(paragraph => DrawEmojiTextParagraph(paragraph, rto, options, pngs))
                .Take(maxLines).ToList();

            // COMBINE

            var width  = options.Width;
            var height = rto.Font.Size * rto.LineSpacing;

            var canvas = new Image<Rgba32>(width, ((lines.Count + 0.5F) * height).RoundInt());

            var offsetY = 0.25F;
            foreach (var line in lines)
            {
                var x = rto.HorizontalAlignment switch
                {
                    HorizontalAlignment.Center => (width - line.Width) / 2,
                    HorizontalAlignment.Right  =>  width - line.Width,
                    _ => 0
                };
                var point = new Point(x, offsetY.RoundInt());
                canvas.Mutate(ctx => ctx.DrawImage(line, point));
                offsetY += height;
                line.Dispose();
            }

            return canvas;
        }

        private static Image<Rgba32> DrawEmojiTextParagraph
        (
            string paragraph, RichTextOptions rto, Options options, Queue<string> pngs
        )
        {
            var  textChunks = EmojiRegex.Replace(paragraph, "\t").Split('\t');
            var emojiChunks = GetEmojiPngs(EmojiRegex.Matches(paragraph));

            var width  = options.Width;
            var height = rto.Font.Size * rto.LineSpacing;
            var safeHeight = (1.5F * height).CeilingInt();
            var margin = (0.25F * height).RoundInt();

            var canvas = GetEmptyCanvas();
            var x = 0;

            for (var i = 0; i < emojiChunks.Count; i++)
            {
                DrawText ( textChunks[i]);
                DrawEmoji(emojiChunks[i]);
            }
            DrawText(textChunks[^1]);

            if (x < canvas.Width)
                canvas.Mutate(ctx => ctx.Crop(Math.Max(x, 1), canvas.Height));

            return canvas;


            // == FUN ==

            void DrawEmoji(List<string> sequence)
            {
                var side = options.EmojiSize.RoundInt();
                var size = new Size(side, side);
                var decoder = new DecoderOptions
                {
                    TargetSize = size,
                    Sampler = KnownResamplers.Lanczos2
                };

                foreach (var _ in sequence)
                {
                    var emoji = pngs.Dequeue();
                    if (emoji.EndsWith(".png"))
                    {
                        var image = Image.Load<Rgba32>(decoder, emoji);
                        if (options.Pixelate) image.Mutate(ctx => ctx.Pixelate(Math.Max(side / 16, 2)));
#if DEBUG
                        canvas.Mutate(ctx => ctx.Fill(Color.Gold, new Rectangle(GetDrawingOffsetEmo(), size)));
#endif
                        canvas.Mutate(ctx => ctx.DrawImage(image, GetDrawingOffsetEmo(), new GraphicsOptions()));
                        MoveX(side);
                    }
                    else DrawText(emoji);
                }
            }

            void DrawText(string text) // todo make emoji drawer a transient class with methods ?
            {
                var optionsW = new RichTextOptions(rto)
                {
                    WrappingLength = -1,
                    Origin = GetDrawingOffsetTxt()
                }.WithDefaultAlignment();

                var textSize = TextMeasuring.MeasureTextSize(text, optionsW, out _);
                canvas.Mutate(ctx => ctx.DrawText(optionsW, text, options.Color));
                MoveX((int)textSize.Width);
            }

            void MoveX(int offset)
            {
                x += offset;
            }

            Point GetDrawingOffsetEmo() => new(x, margin + 0);
            Point GetDrawingOffsetTxt() => new(x, margin + options.FontOffset.RoundInt());

            Image<Rgba32> GetEmptyCanvas() => new(width, safeHeight);
        }


        // NOT DRAWING

        public static string ReplaceEmoji
        (
            string text, string ok, MatchCollection? matches = null, List<List<string>>? pngs = null
        )
        {
            matches ??= EmojiRegex.Matches(text);
            if (matches.Count == 0) return text;

            pngs ??= GetEmojiPngs(matches);
            var m = 0;
            foreach (var cluster in pngs)
            {
                var replaced = cluster.Select(xd => xd.EndsWith(".png") ? ok : xd);
                text = text.Replace(matches[m++].Value, string.Join("", replaced));
            }

            return text;
        }

        public static EmojiPngList GetEmojiPngs(IList<Match> matches)
        {
            var pngs = new EmojiPngList(matches.Count);

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

                pngs.Add(new List<string>(cluster.Count));

                for (var i = 0; i < cluster.Count; i++)
                {
                    var j = i;
                    var name = cluster[i];
                    string? file = null;
                    bool repeat;
                    do
                    {
                        repeat = false;

                        var files = GetEmojiFilesCached(name);
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
                        pngs[n].Add(file);
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
                        if (Regex.IsMatch(character, @"[\u231a-\u303d]")) pngs[n].Add(character);
                    }
                }
            }

            return pngs;
        }

        private static readonly LimitedCache<string, string[]> _emojiCache = new(128);

        private static string[] GetEmojiFilesCached(string name)
        {
            if (!_emojiCache.Contains(name, out var files))
            {
                files = Directory.GetFiles(Dir_Emoji, name + "*.png");
                _emojiCache.Add(name, files);
            }

            return files;
        }

        public static int EmojisCached => _emojiCache.Count;
    }

    public class EmojiPngList(int count) : List<List<string>>(count)
    {
        public Queue<string> AsQueue() => new(this.SelectMany(x => x));
    }
}