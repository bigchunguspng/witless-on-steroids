using System.Text;

namespace PF_Tools.Backrooms.Extensions;

public static class Extensions_StringBuilder
{
    public static StringBuilder AppendSpaceSeparator
        (this StringBuilder sb)
    {
        if (sb.Length > 0) sb.Append(' ');
        return sb;
    }

    public static StringBuilder AppendInQuotes
        (this StringBuilder sb, IEnumerable<string> texts, char separator)
        => sb.Append('"').AppendJoin(separator, texts).Append('"');

    public static StringBuilder AppendInQuotes
        (this StringBuilder sb, string text)
        => sb.Append('"').Append(text).Append('"');
}