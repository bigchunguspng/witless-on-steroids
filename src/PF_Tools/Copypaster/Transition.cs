using PF_Tools.Backrooms;

namespace PF_Tools.Copypaster;

/// Transition chance [from one word] to another word.
public readonly struct Transition(int wordId, float chance)
{
    // DATA

    public int   WordId { get; }               = wordId;
    public float Chance { get; private init; } = chance;

    // LOGIC

    public Transition WithChanceIncreasedBy(float value)
    {
        return this with { Chance = Chance.CombineRound(value, 1) };
    }

    public bool CanBeUsedFor(int wordId) => WordId == wordId || IsEmpty();

    public bool    IsEmpty() => WordId == GenerationPack.NO_WORD;
    public bool IsNotEmpty() => WordId != GenerationPack.NO_WORD;

    public static Transition Empty => new(GenerationPack.NO_WORD, 0);
}