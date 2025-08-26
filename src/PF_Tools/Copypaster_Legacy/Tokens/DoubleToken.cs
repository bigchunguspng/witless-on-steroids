using PF_Tools.Copypaster_Legacy.Pack;

namespace PF_Tools.Copypaster_Legacy.Tokens;

/// <summary>
/// Double word token (2 words, 1 space).
/// </summary>
public readonly struct DoubleToken(int id1, int id2, int idC) : IConsumableToken
{
    public int ID1 { get; } = id1; // 1st word
    public int ID2 { get; } = id2; // 2nd word
    public int IDC { get; } = idC; // combination

    public void RememberTransition(GenerationPack db, IConsumableToken next, float chance)
    {
        db.GetOrAddTable(ID1).Put(ID2, chance);

        if (next is SingleToken simple)
        {
            db.GetOrAddTable(ID2).Put(simple.ID, chance);
            db.GetOrAddTable(IDC).Put(simple.ID, chance);
        }
        else if (next is DoubleToken combined)
        {
            var (low, high) = IConsumableToken.SplitChance(chance);

            db.GetOrAddTable(ID2).Put(combined.ID1, low);
            db.GetOrAddTable(ID2).Put(combined.IDC, high);
            db.GetOrAddTable(IDC).Put(combined.ID1, low);
            db.GetOrAddTable(IDC).Put(combined.IDC, high);
        }
    }
}