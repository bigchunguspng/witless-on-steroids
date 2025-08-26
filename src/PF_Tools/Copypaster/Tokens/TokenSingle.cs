namespace PF_Tools.Copypaster.Tokens;

/// Single word (no spaces).
public readonly struct TokenSingle(int id) : IConsumableToken
{
    public int ID { get; } = id;

    public void RememberTransition(GenerationPack db, IConsumableToken next, float chance)
    {
        if (next is TokenSingle simple)
        {
            db.PutTransition(ID, simple.ID, chance);
        }
        else if (next is TokenDouble combined)
        {
            var (low, high) = IConsumableToken.SplitChance(chance);

            db.PutTransition(ID, combined.ID1, low);
            db.PutTransition(ID, combined.IDC, high);
        }
    }
}