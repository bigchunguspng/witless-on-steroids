using Witlesss.Generation.Pack;

namespace Witlesss.Generation.Tokens;

/// <summary>
/// Single word token (no spaces).
/// </summary>
public readonly struct SingleToken(int id) : IConsumableToken
{
    public int ID { get; } = id;

    public void RememberTransition(GenerationPack db, IConsumableToken next)
    {
        if (next is SingleToken simple)
        {
            db.GetOrAddTable(ID).Put(simple.ID, 1F);
        }
        else if (next is DoubleToken combined)
        {
            db.GetOrAddTable(ID).Put(combined.ID1, 0.1F);
            db.GetOrAddTable(ID).Put(combined.IDC, 2.9F);
        }
    }
}

public interface IConsumableToken
{
    void RememberTransition(GenerationPack db, IConsumableToken next);
}