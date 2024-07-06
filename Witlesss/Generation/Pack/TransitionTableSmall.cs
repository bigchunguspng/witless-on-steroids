using System;
using System.Collections.Generic;
using System.Linq;

namespace Witlesss.Generation.Pack;

public class TransitionTableSmall : TransitionTable
{
    private Transition[] _transitions = [];

    public IEnumerable<Transition> AsIEnumerable => _transitions;

    public Transition this[int index] => _transitions[index];

    public int Count => _transitions.Length;

    public float TotalChance => MathF.Round(_transitions.Sum(x => x.Chance), 1);


    public void Put(int id, float chance)
    {
        var i = ((TransitionTable)this).GetIndexOrAdd(id, chance);
        if (i >= 0) _transitions[i] = _transitions[i].WithChanceIncreasedBy(chance);
    }

    public void PutMax(int id, float chance)
    {
        var i = ((TransitionTable)this).GetIndexOrAdd(id, chance);
        if (i >= 0) _transitions[i] = _transitions[i].WithMaxChance(chance, out _);
    }

    public void Add(Transition transition)
    {
        var original = _transitions;
        _transitions = new Transition[original.Length + 1];
        for (var i = 0; i < original.Length; i++)
        {
            _transitions[i] = original[i];
        }

        _transitions[^1] = transition;
    }
}