using Witlesss.Generation.Pack;

namespace Witlesss.Generation.Tokens;

/// <summary>
/// Double word token (2 words, 1 space).
/// </summary>
public readonly struct DoubleToken(int id1, int id2, int idC) : IConsumableToken
{
    public int ID1 { get; } = id1; // 1st word
    public int ID2 { get; } = id2; // 2nd word
    public int IDC { get; } = idC; // combination

    public void RememberTransition(GenerationPack db, IConsumableToken next)
    {
        db.GetOrAddTable(ID1).Put(ID2, 1F);

        if (next is SingleToken simple)
        {
            db.GetOrAddTable(ID2).Put(simple.ID, 1F);
            db.GetOrAddTable(IDC).Put(simple.ID, 1F);
        }
        else if (next is DoubleToken combined)
        {
            db.GetOrAddTable(ID2).Put(combined.ID1, 0.1F);
            db.GetOrAddTable(ID2).Put(combined.IDC, 2.9F);
            db.GetOrAddTable(IDC).Put(combined.ID1, 0.1F);
            db.GetOrAddTable(IDC).Put(combined.IDC, 2.9F);
        }
    }
}