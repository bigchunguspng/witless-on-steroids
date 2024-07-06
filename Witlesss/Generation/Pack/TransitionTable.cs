using System.Collections.Generic;

namespace Witlesss.Generation.Pack;

public interface TransitionTable
{
    IEnumerable<Transition> AsIEnumerable { get; }

    Transition this[int index] { get; }

    int Count { get; }

    float TotalChance { get; }

    void Put   (int id, float chance);
    void PutMax(int id, float chance);

    void Add  (Transition transition);

    int FindIndexByID(int id)
    {
        var i = 0;
        foreach (var transition in AsIEnumerable)
        {
            if (transition.WordID == id) return i;
            i++;
        }

        return -1;
    }

    protected internal int GetIndexOrAdd(int id, float chance)
    {
        var index = FindIndexByID(id);
        if (index < 0) Add(new Transition(id, chance));

        return index;
    }
}