namespace PF_Tools.Copypaster.Tokens;

/// Two words, separated by a space.
public readonly struct TokenDouble(int id1, int id2, int idC) : IConsumableToken
{
    public  int ID1 { get; } = id1; // 1st word
    private int ID2 { get; } = id2; // 2nd word
    public  int IDC { get; } = idC; // Combination

    public void RememberTransition(GenerationPack db, IConsumableToken next, int chance)
    {
        db.PutTransition(ID1, ID2, chance);

        if (next is TokenSingle simple)
        {
            db.PutTransition(ID2, simple.ID, chance);
            db.PutTransition(IDC, simple.ID, chance);
        }
        else if (next is TokenDouble combined)
        {
            var (low, high) = IConsumableToken.SplitChance(chance);

            db.PutTransition(ID2, combined.ID1, low);
            db.PutTransition(ID2, combined.IDC, high);
            db.PutTransition(IDC, combined.ID1, low);
            db.PutTransition(IDC, combined.IDC, high);
        }
    }
}