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
        private static string DefaultInput(Filter p) => p.Output is SingleLabel l ? l.Tag : null;

        public static Filter Split   (this Filter node, string a, string b) => Filter.Split(DefaultInput(node), Split(), a, b, node);

        public static Filter Scale   (this Filter node, string input, Size    s,     string output = null) => new(input,  Scale(s), output, node);
        public static Filter Trim    (this Filter node, string input, CutSpan s,     string output = null) => new(input,   Trim(s), output, node);
        public static Filter Reverse (this Filter node, string input,                string output = null) => new(input, Reverse(), output, node);
        public static Filter Crop    (this Filter node, string input, Rectangle r,   string output = null) => new(input,   Crop(r), output, node);
        public static Filter Blur    (this Filter node, string input, double b,      string output = null) => new(input,   Blur(b), output, node);
        public static Filter Fps     (this Filter node, string input, double fps,    string output = null) => new(input,  FPS(fps), output, node);

        public static Filter Overlay (this Filter node, string a, string b, Point p, string output = null) => Filter.Join(a, b, Overlay(p), output, node);
        public static Filter Concat  (this Filter node, string a, string b,          string output = null) => Filter.Join(a, b,   Concat(), output, node);
    }

    /// <summary> Represents a node of "-filter_complex" option. </summary>
    public class Filter
    {
        private readonly Filter Previous;

        private readonly Label Input;
        private readonly string Action;
        public  readonly Label Output;

        private string PriorText => Previous is null ? "" : $"{Previous.Text};";
        private string Out       => Output   is null ? "" :      Output.Text;

        public static Filter Null => null;
        public static Filter Join (string a, string b, string action, string output, Filter last = null) => new (new DoubleLabel(a, b), action, new SingleLabel(output), last);
        public static Filter Split(string input,  string action, string a, string b, Filter last = null) => new (new SingleLabel(input), action, new DoubleLabel(a,  b), last);

        public  Filter(string input, string action, string output, Filter last = null) : this(new SingleLabel(input), action, new SingleLabel(output), last) { }
        private Filter(Label  input, string action, Label  output, Filter last = null)
        {
            Input  =  input;
            Action = action;
            Output = output;
            Previous = last;
        }

        public string Text => $"{PriorText}{Input.Text}{Action}{Out}";
    }

    /// <summary> Input / Output label of a "-filter_complex" node. </summary>
    public interface Label
    {
        public string Text { get; }
    }

    public record SingleLabel(string Tag) : Label
    {
        public string Text => Tag is null ? "" : $"[{Tag}]";
    }

    public record DoubleLabel(string A, string B) : Label
    {
        public string Text => $"[{A}][{B}]";
    }
}