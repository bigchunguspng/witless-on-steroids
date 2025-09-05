using System.Text;

namespace PF_Tools.Backrooms.Extensions;

public static class Extensions_StringBuilder
{
    public static StringBuilder AppendSpaceSeparator
        (this StringBuilder sb)
        => sb.AppendSeparator(' ');

    public static StringBuilder AppendSeparator
        (this StringBuilder sb, char separator)
    {
        if (sb.Length > 0) sb.Append(separator);
        return sb;
    }

    public static StringBuilder AppendInQuotes
        (this StringBuilder sb, IEnumerable<string> texts, char separator)
        => sb.Append('"').AppendJoin(separator, texts).Append('"');

    public static StringBuilder AppendInQuotes
        (this StringBuilder sb, string text)
        => sb.Append('"').Append(text).Append('"');

    public static StringBuilder AppendInQuotes
        (this StringBuilder sb, StringBuilder text)
        => sb.Append('"').Append(text).Append('"');
}