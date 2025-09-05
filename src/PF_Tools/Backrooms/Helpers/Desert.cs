using System.Security.Cryptography;

namespace PF_Tools.Backrooms.Helpers;

/// Random string generator.
public static class Desert
{
    // 32 chars long (excluded: IOMW).
    private static readonly string _chars =
        "0123456789ABCDEFGHJKLNPQRSTUVXYZ";

    /// Get a random char sequence of given length. <br/>
    /// Number of possible values = 32 ^ length.
    public static string GetSand(int length = 4)
    {
        Span<char> chars = stackalloc char[length];
        Span<byte> bytes = stackalloc byte[length];

        RandomNumberGenerator.Fill(bytes);

        for (var i = 0; i < length; i++)
        {
            var index = bytes[i] & 0b00011111; // same as % 32
            chars[i] = _chars[index];
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