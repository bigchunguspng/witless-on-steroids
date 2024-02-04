using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using static Witlesss.Memes;
using FFMpAO = FFMpegCore.FFMpegArgumentOptions;

namespace Witlesss.MediaTools
{
    public class F_Process : F_Action_SingleInput
    {
        public F_Process(string input) : base(input) { }


        #region MEDIA CONVERSION

        // -s WxH -an -vcodec libx264 -crf 30
        public F_Action ToAnimation() => ApplyEffects(o =>
        {
            var v = GetVideoStream(_input);
            var size = FitSize(new Size(v.Width, v.Height));
            o.Resize(size).DisableChannel(Channel.Audio).WithCompression(30);
        });

        // -c:a libopus -b:a 48k -vn
        public F_Action ToVoiceMessage() => ApplyEffects(o =>
        {
            o.WithAudioCodec("libopus").WithAudioBitrate(48).DisableChannel(Channel.Video);
        });

        // -filter:v "crop=W:H:X:Y" -s 384x384
        public F_Action ToVideoNote(Rectangle crop) => ApplyEffects(o =>
        {
            o.WithVideoFilters(v => v.Crop(crop)).Resize(VideoNoteSize);
        });

        public F_Action ToSticker(Size size) => ApplyEffects(o => o.Resize(size));

        public F_Action Edit(string options) => ApplyEffects(o => o.WithCustomArgument(options));

        #endregion


        #region COMPRESSION

        // [-s WxH] [-vn] [-vcodec libx264 -crf 45] [-b:a 1k] [-f mp3]
        public F_Action Compress(int factor) => ApplyEffects(o => AddCompression(o, factor));

        private void AddCompression(FFMpAO o, int factor)
        {
            // factor: 0 lossless - 51 lowest quality
            var i = MediaInfoWithFixing(o);
            if (i.video) o.WithCompression(factor);
            if (i.audio) o.WithAudioBitrate(154 - 3 * factor);
            if (i.audio && !i.video) o.ForceFormat("mp3");
        }

        public F_Action CompressImage (Size s) => ApplyEffects(o => o.Resize(s).WithQscale(5)); // -qscale:v 5
        public F_Action CompressAnimation   () => ApplyEffects(o =>
        {
            var v = GetVideoStream(_input);
            o.FixWebmSize(v).DisableChannel(Channel.Audio).WithCompression(30);
        });

        public F_Action DeepFry        (int qscale) => ApplyEffects(o => DeepFryArgs(o, qscale));
        public F_Action DeepFryVideo(Size s, int f) => ApplyEffects(o => DeepFryArgs(o.Resize(s), f, isVideo: true));

        private void DeepFryArgs(FFMpAO o, int compression = 0, bool isVideo = false)
        {
            var sb = new StringBuilder("-filter_complex \"[v:0]");

            // VIGNETTE
            if (isVideo && IsOneIn(4))
            {
                sb.Append("vignette=").Append(RandomDouble(0.1, 0.5)).Append(',');
            }
            // https://ffmpeg.org/ffmpeg-filters.html#vignette-1

            // PIXELIZE
            if (IsOneIn(isVideo ? 4 : 8))
            {
                var i = MediaInfo();
                var p = Math.Max(2, Math.Min(i.v.Width, i.v.Height) / RandomInt(60, 120));
                sb.Append("pixelize=").Append(p).Append(':').Append(p).Append(":p=3,");
            }
            // https://ffmpeg.org/ffmpeg-filters.html#pixelize
            
            // AMPLIFY
            if (isVideo && IsOneIn(4))
            {
                sb.Append("amplify=").Append(RandomInt(1, 5)); // radius
                sb.Append(":factor=").Append(RandomInt(1, 5));

                var zeroes = RandomInt(0, 3);
                sb.Append(":threshold=1").Append(new string('0', zeroes)); // 1-10-100-1000
                if (zeroes == 3) // if threshold = 1000
                {
                    var values = new[] { 1, 10, 25, 50 };
                    sb.Append(":tolerance=").Append(Pick(values));
                }
                sb.Append(',');
            }
            // https://ffmpeg.org/ffmpeg-filters.html#amplify

            // HUE SATURATION
            sb.Append("huesaturation=").Append(RandomInt(-25, 25)); // [-180 - 180]
            sb.Append  (":saturation=").Append(RandomDouble(-1, 1));
            sb.Append   (":intensity=").Append(RandomDouble(-1, 1)); // was 0, 0.5
            sb.Append    (":strength=").Append(RandomInt(1, 100)); // 1 - 100 // was 1, 14
            if (IsOneIn(4))
            {
                var colors = new[] { 'r', 'g', 'b', 'c', 'm', 'y' };
                var selectedColors = colors.Where(_ => IsOneIn(3)).ToArray();
                if (selectedColors.Length > 0)
                {
                    sb.Append(":colors=").Append(string.Join('+', selectedColors));
                }
            }
            if (IsOneIn(4))
            {
                var colors = new[] { "rw", "gw", "bw" };
                var selectedColors = colors.Where(_ => IsOneIn(RandomInt(2, 3))).ToArray();
                foreach (var color in selectedColors)
                {
                    sb.Append(":").Append(color).Append("=").Append(RandomDouble(0, 1));
                }
            }
            sb.Append(",");
            // https://ffmpeg.org/ffmpeg-filters.html#huesaturation

            // UNSHARP
            var lumaMatrixSize = RandomInt(1, 11) * 2 + 1; // [3 - 23], odd only, lx + ly <= 26
            var lumaAmount = RandomDouble(-1.5, 1.5);  // [-1.5 - 1.5]

            sb.Append("unsharp");
            var b = IsOneIn(2);
            var s = Math.Min(lumaMatrixSize, 26 - lumaMatrixSize);
            sb.Append("=lx=").Append(b ? s : lumaMatrixSize);
            sb.Append(":ly=").Append(b ? lumaMatrixSize : s);
            sb.Append(":la=").Append(lumaAmount).Append(",");
            // https://ffmpeg.org/ffmpeg-filters.html#unsharp-1

            // NOISE
            var n_min = isVideo ? 10 : 25;
            var n_max = isVideo ? 45 : 100;
            
            sb.Append("noise").Append("=c0s=").Append(RandomInt(n_min, n_max)); // [0 - 100]
            if (IsOneIn(4)) sb.Append(":c1s=").Append(RandomInt(n_min, n_max)); // yellow-blue
            if (IsOneIn(4)) sb.Append(":c2s=").Append(RandomInt(n_min, n_max)); // red-green

            sb.Append(":allf=t");
            var flags = new[] { 'u', 'p', 'a' };
            var selectedFlags = flags.Where(_ => IsOneIn(2)).ToArray();
            foreach (var flag in selectedFlags)
            {
                sb.Append('+').Append(flag);
            }
            sb.Append("\"");
            // https://ffmpeg.org/ffmpeg-filters.html#noise


            var factor = isVideo
                ? compression
                : compression > 26 ? compression : Math.Min(31, compression + RandomInt(0, 10));

            if (isVideo) AddCompression(o, factor);
            o.WithQscale(factor).WithCustomArgument(sb.ToString());


            string RandomDouble(double min, double max) => FormatDouble(Extension.RandomDouble(min, max));
        }

        #endregion


        #region SPEED

        // [-s WxH] [-vn] [-vf reverse] [-af areverse]
        public F_Action Reverse() => ApplyEffects(o =>
        {
            var i = MediaInfoWithFixing(o);
            if (i.video) o.WithVideoFilters(v => v.ReverseVideo());
            if (i.audio) o.WithAudioFilters(a => a.ReverseAudio());
        });
        
        // [-vf "setpts=0.5*PTS,fps=60"][-s WxH] [-af "atempo=2.0"][-vn]
        public F_Action ChangeSpeed(double speed) => ApplyEffects(o =>
        {
            var i = MediaInfoWithFixing(o);
            if (i.video) o.WithVideoFilters(v => v.ChangeVideoSpeed(speed).SetFPS(GetFPS()));
            if (i.audio) o.WithAudioFilters(a => a.ChangeAudioSpeed(speed));

            double GetFPS() => Math.Min(i.v.AvgFrameRate * speed, 90D);
        });

        #endregion


        #region AUDIO

        public F_Action ChangeVolume(string arg)
        {
            return ApplyEffects(o => o.WithAudioFilters(v => v.Volume(arg)).WithVideoCodec("copy"));
        }
        
        public F_Action EQ(double[] args)
        {
            return ApplyEffects(o => o.WithAudioFilters(v => v.Equalize(args)).WithVideoCodec("copy"));
        }

        #endregion


        #region CROP / SCALE

        // -filter:v "crop=272:272:56:56"
        public F_Action CropVideoNote       () => ApplyEffects(o => o.WithVideoFilters(v => v.Crop(VideoNoteCrop)));
        public F_Action CropVideo (string[] c) => ApplyEffects(o => o.WithVideoFilters(v => v.Crop(c)));

        // -vf "scale=W:H,setsar=1"
        public F_Action ScaleVideo(string[] s) => ApplyEffects(o => o.WithVideoFilters(v => v.Scale(s).SampleRatio(1)));

        #endregion


        #region YT MUSIC

        public F_Action ExportThumbnail (bool square) => ApplyEffects(o => ExportThumbnail(o, square));
        public F_Action ResizeThumbnail (bool square) => ApplyEffects(o => ResizeThumbnail(o, square));
        public F_Action CompressJpeg    (int  factor) => ApplyEffects(o => o.WithQscale(factor));

        // -ss 1 -frames:v 1 -vf
        private static void ExportThumbnail(FFMpAO o, bool square)
        {
            ResizeThumbnail(o.Seek(TimeSpan.FromSeconds(1)).WithFrameOutputCount(1), square);
        }

        // -vf [ "crop=w='min(iw,ih)':h='min(iw,ih)',scale=640:640" | "scale=640:-1" ] -qscale:v 2
        private static void ResizeThumbnail(FFMpAO o, bool square)
        {
            o.WithVideoFilters(v => ApplyResizing(v, square)).WithQscale(2);
        }

        private static void ApplyResizing(VideoFilterOptions v, bool square)
        {
            if (square) v.MakeSquare(640);
            else        v.Scale(640,  -1);
        }

        #endregion


        #region OTHER

        public F_Action SliceRandom() => ApplyEffects(SliceRandomArgs);

        private void SliceRandomArgs(FFMpAO o)
        {
            var info = MediaInfo();
            var soundOnly = info.audio && !info.video;
            var seconds = info.video ? info.v.Duration.TotalSeconds : info.a.Duration.TotalSeconds;
            var minutes = seconds / 60;

            var timecodes = new List<(double A, double B)>();
            var head = -seconds / 2;
            while (head < seconds)
            {
                var step = IsFirstOf(seconds < 5 ? 8 : seconds < 30 ? 6 : seconds < 60 ? 4 : 2, 10) ? -1d : 1d;
                step *=
                      seconds <  5 ? RandomDouble(seconds / 20, seconds / 10)
                    : seconds < 30 ? IsOneIn(3) ? RandomInt(2,  5) : seconds / 15
                    : seconds < 60 ? IsOneIn(5) ? RandomInt(2, 10) : 5
                    : minutes <  5 ? IsOneIn(2) ? IsOneIn(2) ? RandomInt(10, 30) : RandomInt(1, 5) :  5
                    :                IsOneIn(2) ? IsOneIn(2) ? BigLeap()         : RandomInt(1, 5) : 10;

                var length = seconds < 5
                    ? RandomDouble(0.15, 0.35)
                    : RandomDouble(0.25, Math.Min(0.35 + 0.01 * seconds, 1.25));

                if (soundOnly && minutes < 3) length *= RandomDouble(1.5, 3);

                var a = Math.Clamp(head + step, 0, seconds - 0.15);
                var b = Math.Min(a + length, seconds);

                timecodes.Add((a, b));

                head = b;

                double BigLeap()
                {
                    var avg = Math.Min(seconds + head, 2 * seconds - head);
                    return step * RandomInt(10, Math.Max(10, (int)(0.1 * avg)));
                }
            }

            if (timecodes.Count <= 32) ApplyTrims(o, info, timecodes);
            else                 TrimPieceByPiece(o, info, timecodes);
        }

        private void TrimPieceByPiece(FFMpAO o, MediaInfo info, List<(double A, double B)> timecodes)
        {
            var count = (int)Math.Ceiling(timecodes.Count / 32d);
            var take = timecodes.Count / count + 1;
            var parts = new string[count];

            Log($"SLICING LONG VIDEO!!! ({count} parts, {timecodes.Count} trims)", ConsoleColor.Yellow);

            for (var i = 0; i < count; i++)
            {
                var offset = i * take;
                parts[i] = new F_Process(_input)
                    .ApplyEffects(ops => ApplyTrims(ops, info, timecodes, offset, take))
                    .Output($"-part-{i}", Path.GetExtension(_input));

                Log($"Part {i + 1} done!", ConsoleColor.Yellow);
            }

            var sb = new StringBuilder();
            for (var i = 0; i < count; i++)
            {
                sb.Append("-i \"").Append(parts[i]).Append("\" ");
            }
            sb.Append("-filter_complex \"");
            for (var i = 1; i <= count; i++)
            {
                if (info.video) sb.Append("[").Append(i).Append(":v]");
                if (info.audio) sb.Append("[").Append(i).Append(":a]");
            }
            AppendConcatenation(sb, count, info).Append('"');

            o.WithCustomArgument(sb.ToString());
        }

        private void ApplyTrims(FFMpAO o, MediaInfo info, List<(double A, double B)> timecodes, int offset = 0, int take = 0)
        {
            var count = take == 0 ? timecodes.Count : Math.Min(take, timecodes.Count - offset);

            var sb = new StringBuilder("-filter_complex \"");
            for (var i = offset; i < offset + count; i++)
            {
                if (info.video) AppendTrimming('v', "");
                if (info.audio) AppendTrimming('a', "a");

                void AppendTrimming(char av, string a)
                {
                    sb.Append("[0:").Append(av).Append(']').Append(a).Append("trim=");
                    sb.Append(Format(timecodes[i].A)).Append(':');
                    sb.Append(Format(timecodes[i].B));
                    sb.Append(',').Append(a).Append("setpts=PTS-STARTPTS[").Append(av).Append(i).Append("];");
                }
                string Format(double x) => FormatDouble(Math.Round(x, 3));
            }
            for (var i = offset; i < offset + count; i++)
            {
                if (info.video) sb.Append("[v").Append(i).Append(']');
                if (info.audio) sb.Append("[a").Append(i).Append(']');
            }
            AppendConcatenation(sb, count, info).Append('"');

            o.WithCustomArgument(sb.ToString());
        }

        private StringBuilder AppendConcatenation(StringBuilder sb, int count, MediaInfo info)
        {
            sb.Append("concat=n=").Append(count);
            sb.Append(":v=").Append(info.video ? 1 : 0);
            sb.Append(":a=").Append(info.audio ? 1 : 0);
            return sb;
        }

        #endregion
    }
}