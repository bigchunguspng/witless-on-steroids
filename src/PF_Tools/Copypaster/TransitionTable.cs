using PF_Tools.Copypaster.TransitionTables;

namespace PF_Tools.Copypaster;

/// Transition chances [from one word] to many other.
public interface TransitionTable
{
    Transition this[int index] { get; }
    IEnumerable<Transition> AsIEnumerable();

    int   Count       { get; }
    float TotalChance { get; }

    bool ShouldBeUpgradedToPut(int wordId);

    /// Increases transition chance to given word.
    void Put(int wordId, float chance);

    /// Returns -1 if given word is not in the table.
    int IndexOfWordId(int wordId);

    /// Convenient wrapper for <see cref="IndexOfWordId"/>.
    bool ContainsWordId(int wordId, out int index)
    {
        index = IndexOfWordId(wordId);
        return index >= 0;
    }
}

public static class TransitionTableExtensions
{
    // […] -> [  C2  ] -> [       TransitionTableV8       ] -> [                      TransitionTableVU                       ]
    // [1] -> [1 -> 2] -> [2 -> 3 -> 4 -> 5 -> 6 -> 7 -> 8] -> [8 -> 10 -> 12 -> 15 -> 18 -> 22 -> 27 -> 33 -> 41 -> 51 -> ...]

    public static TransitionTable Upgrade(this TransitionTable table)
    {
        if /**/ (table is TransitionTableC1) return new TransitionTableC2(table[0]);
        else if (table is TransitionTableC2) return new TransitionTableV8(table.AsIEnumerable());
        else if (table is TransitionTableV8) return new TransitionTableVU(table.AsIEnumerable());
        else                                 return table;
    }
}