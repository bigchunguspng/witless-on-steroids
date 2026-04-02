using System.Runtime.CompilerServices;

namespace PF_Tools.Backrooms.Extensions;

public static class Extensions_Bool
{
    /// Compares condition to false. <br/> JAPANESE 100
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Janai
        (this bool condition) => condition == false;

    /// Compares condition to false.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOff
        (this bool condition) => condition == false;

    /// Compares condition to false.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Failed
        (this bool condition) => condition == false;

    /// Compares <see cref="Match.Success"/> to false.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Failed
        (this Match match) => match.Success == false;
}