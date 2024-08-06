using System;

namespace Witlesss.Backrooms;

public static class MathExtensions
{
    public static int RoundInt(this double x) => (int)Math.Round(x);
    public static int RoundInt(this float  x) => (int)Math.Round(x);

    public static int CeilingInt(this double x) => (int)Math.Ceiling(x);
    public static int CeilingInt(this float  x) => (int)Math.Ceiling(x);

    public static byte ClampByte(this int x) => (byte)Math.Clamp(x, 0, 255);
    public static byte Clamp100(this byte x) => Math.Min(x, (byte)100);
    public static byte Clamp(this byte x, byte max) => Math.Min(x, max);
    public static int  Clamp(this int  x, int  max) => Math.Min(x, max);

    public static int ToEven(this int x) => x & ~1;

    public static float CombineRound(this float a, float b) => MathF.Round(a + b, 1);

    public static float Gap(this int   outer, int   inner) => (outer - inner) / 2F;
    public static float Gap(this float outer, float inner) => (outer - inner) / 2F;

    public static int Clamp(this int x, int min, int max) => Math.Clamp(x, min, max);
}