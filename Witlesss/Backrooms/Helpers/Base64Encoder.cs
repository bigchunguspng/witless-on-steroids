using System;

namespace Witlesss.Backrooms.Helpers;

/// <summary>
/// Converts <see cref="int"/> into <see cref="string"/> and back using base 64 number system.
/// <code>
/// -5928-> -1Se -> -5928
///    -3->   -3 ->    -3
///    69->   15 ->    69
///   420->   6a ->   420
///  1337->   Kv ->  1337
/// </code>
/// </summary>
public static class Base64Encoder
{
    private const string BASE_64 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_#";

    public static string ToString(int value)
    {
        var negative = value < 0;
        if (negative)  value = -value;

        var chars = new[]
        {
            // using only 30 bits will lower max value to ~1 billion
            BASE_64[(value & 0b00111111000000000000000000000000) >> 24],
            BASE_64[(value & 0b00000000111111000000000000000000) >> 18],
            BASE_64[(value & 0b00000000000000111111000000000000) >> 12],
            BASE_64[(value & 0b00000000000000000000111111000000) >>  6],
            BASE_64[ value & 0b00000000000000000000000000111111],
        };

        var start = 0;
        for (var i = 0; i < 5; i++)
        {
            if (chars[i] != '0' || i == 4)
            {
                start = i;
                break;
            }
        }

        var digits = new ReadOnlySpan<char>(chars).Slice(start);
        if (negative)
        {
            Span<char> result = stackalloc char[6 - start];
            result[0] = '-';
            digits.CopyTo(result.Slice(1));

            return new string(result);
        }
        else
        {
            Span<char> result = stackalloc char[5 - start];
            digits.CopyTo(result);

            return new string(result);
        }
    }

    public static int ToInt(string value) // ASSUMING THE VALUE IS CORRECT!
    {
        var negative = value[0] == '-';

        var span = value.AsSpan(negative ? 1 : 0);

        var result = 0;
        var offset = (span.Length - 1) * 6;

        foreach (var c in span)
        {
            result = result | (BASE_64.IndexOf(c) << offset);
            offset -= 6;
        }

        return negative ? -result : result;
    }
}