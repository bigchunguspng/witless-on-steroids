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
            var table = pack.FindWordsComingBefore(ids.First.Value);
            ids.AddFirst(table.PickWordId());
        }

        return ids;
    }

    private static int PickWordId
        (this TransitionTable table)
    {
        var r = Random.Shared.Next(table.TotalChance);
        foreach (var transition in table.AsIEnumerable())
        {
            if  (transition.Chance > r) return transition.WordId;
            r -= transition.Chance;
        }

        throw new Exception("UNEXPECTED EXECUTION PATH");
    }

    // BACKWARDS GENERATION HACKS

    private static int PickRandomLastWordId
        (this GenerationPack pack)
    {
        for (var i = 0; i < 12; i++)
        {
            var index = Random.Shared.Next(pack.TransitionsCount);
            var (id, table) = pack.Transitions.ElementAt(index);
            var transitions = table.AsIEnumerable();
            var transitions_to_END = transitions.Any(x => x.WordId == GenerationPack.END);
            if (transitions_to_END) return id;
        }

        return GenerationPack.NO_WORD;
    }

    private static TransitionTable FindWordsComingBefore
        (this GenerationPack pack, int wordId)
    {
        var reverseTable = new TransitionTableVU();

        foreach (var (id, table) in pack.Transitions)
        {
            if (table.ContainsWordId(wordId, out var index))
            {
                reverseTable.Put(id, table[index].Chance);
            }
        }

        return reverseTable;
    }
}