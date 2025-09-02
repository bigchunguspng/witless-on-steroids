using System.Security.Cryptography;

namespace PF_Tools.Backrooms.Helpers;

public static class RandomStringGenerator
{
    // Should be 64 chars long!
    private static readonly string _chars =
        "0123456789"
      + "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
      + "abcdefghijklmnopqrstuvwxyz_-";

    public static string GenerateRandomString(int length = 11)
    {
        Span<char> chars = stackalloc char[length];
        Span<byte> bytes = stackalloc byte[length];

        RandomNumberGenerator.Fill(bytes);

        for (var i = 0; i < length; i++)
        {
            var index = bytes[i] & 0b00111111; // same as % 64
            chars[i] = _chars[index];
        }

        return new string(chars);
    }
}