using System.Security.Cryptography;

namespace PF_Tools.Backrooms.Helpers;

/// Random string generator.
public static class Desert
{
    private const string
        _chars32 =
            "0123456789"
          + "ABCDEFGHJKLNPQRSTUVXYZ", // IMOW out
        _chars64 =
            "0123456789"
          + "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
          + "abcdefghijklmnopqrstuvwxyz_-";

    /// Get a random char sequence of given length. <br/>
    /// Style: numbers, uppercase. <br/>
    /// Number of possible values = 32 ^ length.
    public static string GetSand
        (int length = 4) => GetSoil(length, 0b00011111, _chars32);

    /// Get a random char sequence of given length. <br/>
    /// Style: numbers, uppercase, lowercase, _, -. <br/>
    /// Number of possible values = 64 ^ length.
    public static string GetSilt
        (int length = 4) => GetSoil(length, 0b00111111, _chars64);

    private static string GetSoil(int length, byte mask, string alphabet)
    {
        Span<char> chars = stackalloc char[length];
        Span<byte> bytes = stackalloc byte[length];

        RandomNumberGenerator.Fill(bytes);

        for (var i = 0; i < length; i++)
        {
            var index = bytes[i] & mask;
            chars[i] = alphabet[index];
        }

        return new string(chars);
    }

}

/*  .    _    +     .  ______   .          .
 (      /|\      .    |      \      .   +
     . |||||     _    | |   | | ||         .
.      |||||    | |  _| | | | |_||    .
   /\  ||||| .  | | |   | |      |       .
__||||_|||||____| |_|_____________\__________
. |||| |||||  /\   _____      _____  .   .
  |||| ||||| ||||   .   .  .         ________
 . \|`-'|||| ||||    __________       .    .
    \__ |||| ||||      .          .     .
 __    ||||`-'|||  .       .    __________
.    . |||| ___/  ___________             .
   . _ ||||| . _               .   _________
_   ___|||||__  _ \\--//    .          _
     _ `---'    .)=\oo|=(.   _   .   .    .
_  ^      .  -    . \.|                    */