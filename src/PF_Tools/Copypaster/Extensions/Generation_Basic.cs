using PF_Tools.Copypaster.TransitionTables;

namespace PF_Tools.Copypaster.Extensions;

public static class Generation_Basic
{
    public static LinkedList<int> Generate
        (this GenerationPack pack, int wordId = GenerationPack.START)
    {
        var ids = new LinkedList<int>();

        ids.AddLast(wordId);

        while (ids.Last!.Value != GenerationPack.END)
        {
            var table = pack.GetTransitionTable(ids.Last.Value);
            ids.AddLast(table.PickWordId());
        }

        return ids;
    }

    public static LinkedList<int> GenerateBackwards
        (this GenerationPack pack, int wordId = GenerationPack.END)
    {
        if (wordId == GenerationPack.END)
            wordId = pack.PickRandomLastWordId();

        var ids = new LinkedList<int>();

        ids.AddFirst(wordId);

        while (ids.First!.Value != GenerationPack.START)
        {
            var table = pack.GetWordsBefore(ids.First.Value);
            ids.AddFirst(table.PickWordId(fallback: GenerationPack.START));
        }

        return ids;
    }

    /// <summary>
    /// Finds all word ids, that has given word id in their transition table
    /// </summary>
    private static TransitionTable GetWordsBefore
        (this GenerationPack pack, int wordId)
    {
        var reverseTable = new TransitionTableVU();
        foreach (var pair in pack.Transitions)
        {
            var table = pair.Value;
            if (table.ContainsWordId(wordId, out var index))
            {
                reverseTable.Put(pair.Key, table[index].Chance);
            }
        }
        return reverseTable;
    }

    private static int PickRandomLastWordId
        (this GenerationPack pack)
    {
        for (var i = 0; i < 12; i++)
        {
            var table = pack.Transitions.ElementAt(Random.Shared.Next(pack.TransitionsCount));
            if (table.Value.AsIEnumerable().Any(x => x.WordId == GenerationPack.END)) return table.Key;
        }

        return -1;
    }

    private static int PickWordId
        (this TransitionTable table, int fallback = GenerationPack.END)
    {
        var r = Random.Shared.NextSingle() * table.TotalChance;
        if (r > 0F)
        {
            foreach (var transition in table.AsIEnumerable())
            {
                if (transition.Chance > r) return transition.WordId;
                r -= transition.Chance;
            }

            throw new Exception("UNEXPECTED EXECUTION PATH");
        }

        return fallback;
    }
}