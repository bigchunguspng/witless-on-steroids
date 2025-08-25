using PF_Bot.Generation.Pack;

namespace PF_Bot.Generation.Tokens;

/// <summary>
/// Single word token (no spaces).
/// </summary>
public readonly struct SingleToken(int id) : IConsumableToken
{
    public int ID { get; } = id;

    public void RememberTransition(GenerationPack db, IConsumableToken next, float chance)
    {
        if (next is SingleToken simple)
        {
            db.GetOrAddTable(ID).Put(simple.ID, chance);
        }
        else if (next is DoubleToken combined)
        {
            var (low, high) = IConsumableToken.SplitChance(chance);

            db.GetOrAddTable(ID).Put(combined.ID1, low);
            db.GetOrAddTable(ID).Put(combined.IDC, high);
        }
    }
}

public interface IConsumableToken
{
    void RememberTransition(GenerationPack db, IConsumableToken next, float chance);

    public static (float low, float high) SplitChance(float chance)
    {
        var l = MathF.Round(chance * 0.2F, 1);
        var h = MathF.Round(chance - l,    1);
        return (l, h);
    }
}