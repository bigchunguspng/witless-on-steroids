using System;
using System.Drawing;

namespace Witlesss.MediaTools
{
    public static class ComplexFilterArgs
    {
        private static string _a;

        /// <summary> Call this before appending videofilter nodes </summary> <param name="v"> file has video stream</param>
        public static bool VF(bool v) { _a =  ""; return v; }
        
        /// <summary> Call this before appending audiofilter nodes </summary> <param name="a"> file has audio stream</param>
        public static bool AF(bool a) { _a = "a"; return a; }
        
        private const string PTS = "setpts=PTS-STARTPTS"; // fixes dissync when doing /sus with duration < 1s on video with audio channel

        private static string Overlay (Point p) => $"overlay={p.X}:{p.Y}:format=rgb";
        private static string Scale    (Size s) => $"scale={s.Width}:{s.Height}";
        private static string Crop(Rectangle r) => $"crop={r.Width}:{r.Height}:{r.X}:{r.Y}";
        private static string Blur   (double b) => $"boxblur=1:{b}";
        private static string FPS  (double fps) => $"fps={FormatDouble(fps)}";

        private static string Trim  (CutSpan s) => $"{_a}trim=start='{TsFormat(s.Start)}':duration='{TsFormat(s.Length)}',{_a}{PTS}";
        private static string Split          () => $"{_a}split=2";
        private static string Reverse        () => $"{_a}reverse,{_a}{PTS}";
        private static string Concat         () => $"concat=n=2:v={(_a == "" ? "1" : "0:a=1")}";

        private static string TsFormat(TimeSpan s) => s.ToString("c").Replace(":", "\\:");
        private static string DefaultInput(CoFi p) => p.Output is SingleLabel l ? l.Tag : null;

        public static CoFi Split   (this CoFi node, string a, string b) => CoFi.S(DefaultInput(node), Split(), a, b, node);

        public static CoFi Scale   (this CoFi node, string input, Size    s,     string output = null) => new(input,  Scale(s), output, node);
        public static CoFi Trim    (this CoFi node, string input, CutSpan s,     string output = null) => new(input,   Trim(s), output, node);
        public static CoFi Reverse (this CoFi node, string input,                string output = null) => new(input, Reverse(), output, node);
        public static CoFi Crop    (this CoFi node, string input, Rectangle r,   string output = null) => new(input,   Crop(r), output, node);
        public static CoFi Blur    (this CoFi node, string input, double b,      string output = null) => new(input,   Blur(b), output, node);
        public static CoFi Fps     (this CoFi node, string input, double fps,    string output = null) => new(input,  FPS(fps), output, node);

        public static CoFi Overlay (this CoFi node, string a, string b, Point p, string output = null) => CoFi.J(a, b, Overlay(p), output, node);
        public static CoFi Concat  (this CoFi node, string a, string b,          string output = null) => CoFi.J(a, b,   Concat(), output, node);
    }

    /// <summary> Represents a node of "-filter_complex" option </summary>
    public class CoFi
    {
        private readonly CoFi Previous;

        private readonly Label  Input;
        private readonly string Things;
        public  readonly Label Output;

        private string PriorText => Previous is null ? "" : $"{Previous.Text};";
        private string Out       => Output   is null ? "" :      Output.Text;

        public static CoFi Null => null;
        public static CoFi J(string a, string b, string things, string output, CoFi last = null) => new (new DoubleLabel(a, b), things, new SingleLabel(output), last);
        public static CoFi S(string input,  string things, string a, string b, CoFi last = null) => new (new SingleLabel(input), things, new DoubleLabel(a,  b), last);

        public  CoFi(string input, string things, string output, CoFi last = null) : this(new SingleLabel(input), things, new SingleLabel(output), last) { }
        private CoFi(Label  input, string things, Label  output, CoFi last = null)
        {
            Input  =  input;
            Output = output;
            Previous = last;
            Things = things;
        }

        public string Text => $"{PriorText}{Input.Text}{Things}{Out}";
    }

    /// <summary> Input / Output label of a "-filter_complex" node </summary>
    public interface Label { public string Text { get; } }

    public class SingleLabel : Label
    {
        public SingleLabel(string tag) => Tag = tag;

        public readonly string Tag;
        public          string Text => Tag is null ? "" : $"[{Tag}]";
    }

    public class DoubleLabel : Label
    {
        public DoubleLabel(string a, string b)
        {
            A = a;
            B = b;
        }
        private readonly string A, B;
        public           string Text => $"[{A}][{B}]";
    }
}