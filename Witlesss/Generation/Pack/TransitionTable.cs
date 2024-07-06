using System;
using System.Collections.Generic;
using System.Linq;

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

    int FindIndexByID(int id);
}

public class VastTransitionTable : List<Transition>, TransitionTable
{
    public IEnumerable<Transition> AsIEnumerable => this;

    public float TotalChance { get; private set; }

    private void IncreaseTotalChanceBy(float value)
        => TotalChance = TotalChance.CombineRound(value);

    public VastTransitionTable() { }
    public VastTransitionTable(IEnumerable<Transition> transitions) : base(transitions) { }

    public void Put(int id, float chance)
    {
        var index = GetIndexOrAdd(id, chance);
        if (index >= 0)
        {
            this[index] = this[index].WithChanceIncreasedBy(chance);
            IncreaseTotalChanceBy(chance);
        }
    }

    public void PutMax(int id, float chance)
    {
        var index = GetIndexOrAdd(id, chance);
        if (index >= 0)
        {
            this[index] = this[index].WithMaxChance(chance, out var difference);
            IncreaseTotalChanceBy(difference);
        }
    }

    private int GetIndexOrAdd(int id, float chance)
    {
        var index = FindIndexByID(id);
        if (index < 0) Add(new Transition(id, chance));

        return index;
    }

    public new void Add(Transition transition)
    {
        if (Capacity == Count) IncreaseCapacity();
        base.Add(transition);
        IncreaseTotalChanceBy(transition.Chance);
    }

    // High % of tables contain only 1 element, so their capacity
    // should be increased slowly to avoid wasting lots of space
    // 1 -> 2 -> 3 -> 4 -> 5 -> 6 -> 7 -> 8 -> 10 -> 12 -> 15 -> 18 -> 22 -> 27 -> 33 -> 41 -> 51 -> ...
    private void IncreaseCapacity() => Capacity = Math.Max(Capacity * 5 >> 2, Capacity + 1);

    public int FindIndexByID(int id)
    {
        var i = 0;
        foreach (var transition in this)
        {
            if (transition.WordID == id) return i;
            i++;
        }

        return -1;
    }
}

public class TinyTransitionTable : TransitionTable
{
    private Transition[] _transitions = [];

    public IEnumerable<Transition> AsIEnumerable => _transitions;

    public Transition this[int index] => _transitions[index];

    public int Count => _transitions.Length;

    public float TotalChance => MathF.Round(_transitions.Sum(x => x.Chance), 1);

    public void Put(int id, float chance)
    {
        var i = GetIndexOrAdd(id, chance);
        if (i >= 0) _transitions[i] = _transitions[i].WithChanceIncreasedBy(chance);
    }

    public void PutMax(int id, float chance)
    {
        var i = GetIndexOrAdd(id, chance);
        if (i >= 0) _transitions[i] = _transitions[i].WithMaxChance(chance, out _);
    }

    private int GetIndexOrAdd(int id, float chance)
    {
        var index = FindIndexByID(id);
        if (index < 0) Add(new Transition(id, chance));

        return index;
    }

    public void Add(Transition transition)
    {
        var old = _transitions;
        _transitions = new Transition[old.Length + 1];
        for (var i = 0; i < old.Length; i++)
        {
            _transitions[i] = old[i];
        }

        _transitions[^1] = transition;
    }

    public int FindIndexByID(int id)
    {
        var i = 0;
        foreach (var transition in _transitions)
        {
            if (transition.WordID == id) return i;
            i++;
        }

        return -1;
    }
}