using System;

namespace Witlesss.Backrooms;

public static class MathExtensions
{
    public static int RoundInt(this double x) => (int)Math.Round(x);
    public static int RoundInt(this float  x) => (int)Math.Round(x);

    public static byte ClampByte(this int x) => (byte)Math.Clamp(x, 0, 255);

    public static int ToEven(this int x) => x & ~1;
}