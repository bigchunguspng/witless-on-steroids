using System;

namespace Witlesss.Generation.Pack;

public readonly struct Transition(int wordID, float chance)
{
    public int   WordID { get; }               = wordID;
    public float Chance { get; private init; } = chance;

    public Transition WithChanceIncreasedBy(float value)
    {
        return this with { Chance = Chance.CombineRound(value) };
    }

    public Transition WithMaxChance(float value, out float difference)
    {
        var max = Math.Max(Chance, value);
        difference = max - Chance;
        return this with { Chance = max };
    }
}