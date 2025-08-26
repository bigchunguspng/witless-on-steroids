namespace PF_Tools.Copypaster_Legacy.Pack;

public readonly struct Transition(int wordID, float chance)
{
    public int   WordID { get; }               = wordID;
    public float Chance { get; private init; } = chance;

    public Transition WithChanceIncreasedBy(float value)
    {
        return this with { Chance = Chance.CombineRound(value, 1) };
    }

    public Transition WithMaxChance(float value, out float difference)
    {
        var max = Math.Max(Chance, value);
        difference = max - Chance;
        return this with { Chance = max };
    }

    public bool IsPossible() => WordID != GenerationPack.NO_WORD;

    public static Transition Impossible => new(GenerationPack.NO_WORD, -1);
}