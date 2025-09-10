namespace PF_Tools.Backrooms.Types;

/// Struct salesman: *slaps docstring* <br/>
/// This bad boy can fit up to 8 bool flags in a single byte.
/// Usage example:
/// <code> public bool Real { get => _flags[3]; set => _flags[3] = value; } </code>
public struct FlagSet_8
{
    private byte _data;

    /// Don't pass anything >= 8 here.
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