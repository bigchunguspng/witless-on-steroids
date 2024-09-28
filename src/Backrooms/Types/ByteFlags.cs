namespace Witlesss.Backrooms.Types;

/// <summary>
/// Use this struct to store up to 8 bool values in a single byte.
/// </summary>
public struct ByteFlags
{
    private byte _data;

    /// <summary>
    /// Don't pass anything >= 8 here.
    /// </summary>
    public bool this[byte index]
    {
        get => (_data & (1 << index)) != 0;
        set
        {
            if (value) _data |= (byte) (1 << index);
            else       _data &= (byte)~(1 << index);
        }
    }
}

/*

USAGE EXAMPLE:

private ByteFlags _flags;

[JsonProperty] public bool Stickers { get => _flags[1]; set => _flags[1] = value; }

*/