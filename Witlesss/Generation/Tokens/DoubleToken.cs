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
        db.GetTableByID(ID1).Put(ID2, 1F);

        var table2 = db.GetTableByID(ID2);
        var tableC = db.GetTableByID(IDC);

        if (next is SingleToken simple)
        {
            table2.Put(simple.ID, 1F);
            tableC.Put(simple.ID, 1F);
        }
        else if (next is DoubleToken combined)
        {
            table2.Put(combined.ID1, 0.1F);
            table2.Put(combined.IDC, 2.9F);
            tableC.Put(combined.ID1, 0.1F);
            tableC.Put(combined.IDC, 2.9F);
        }
    }
}