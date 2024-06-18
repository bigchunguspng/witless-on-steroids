using System;

namespace Witlesss.Backrooms;

public static class MathExtensions
{
    public static int RoundInt(this double x) => (int)Math.Round(x);
    public static int RoundInt(this float  x) => (int)Math.Round(x);

    public static int CeilingInt(this double x) => (int)Math.Ceiling(x);
    public static int CeilingInt(this float  x) => (int)Math.Ceiling(x);

    public static byte ClampByte(this int x) => (byte)Math.Clamp(x, 0, 255);

    public static int ToEven(this int x) => x & ~1;

    public static float CombineRound(this float a, float b) => MathF.Round(a + b, 1);
}