using System;
using System.Collections.Generic;
using System.Linq;

namespace Witlesss.Generation.Pack;

public class TransitionTableSmall(IEnumerable<Transition> transitions) : TransitionTable
{
    private Transition[] _transitions = transitions.ToArray();

    public float TotalChance => MathF.Round(_transitions.Sum(x => x.Chance), 1);

    public Transition this[int index]
    {
        get => _transitions[index];
        set => _transitions[index] = value;
    }

    public bool CanAccept(int id)
    {
        return _transitions.Length < 8 || _transitions.Any(x => x.WordID == id);
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

    public IEnumerable<Transition> AsIEnumerable() => _transitions;
}