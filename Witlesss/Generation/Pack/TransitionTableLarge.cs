using System;
using System.Collections.Generic;

namespace Witlesss.Generation.Pack;

public class TransitionTableLarge : List<Transition>, TransitionTable
{
    public float TotalChance { get; private set; }
    private void IncreaseTotalChanceBy(float value) => TotalChance = TotalChance.CombineRound(value);

    public TransitionTableLarge() { }
    public TransitionTableLarge(IEnumerable<Transition> transitions) : base(transitions) { }

    public bool CanAccept(int id) => true;

    public void Put(int id, float chance)
    {
        var index = ((TransitionTable)this).GetIndexOrAdd(id, chance);
        if (index >= 0)
        {
            this[index] = this[index].WithChanceIncreasedBy(chance);
            IncreaseTotalChanceBy(chance);
        }
    }

    public void PutMax(int id, float chance)
    {
        var index = ((TransitionTable)this).GetIndexOrAdd(id, chance);
        if (index >= 0)
        {
            this[index] = this[index].WithMaxChance(chance, out var difference);
            IncreaseTotalChanceBy(difference);
        }
    }

    public new void Add(Transition transition)
    {
        if (Capacity == Count) IncreaseCapacity();
        base.Add(transition);
        IncreaseTotalChanceBy(transition.Chance);
    }

    // ~50% of tables contain only 1 element, and ~95% - 8 or less. Consequences:
    // 1. Their capacity should be increased slowly to avoid wasting lots of space.
    // 2. Less bloated structure should be used for small tables.

    // [        TransitionTableSmall        ] -> [        TransitionTableLarge                       -> ...
    // [1 -> 2 -> 3 -> 4 -> 5 -> 6 -> 7 -> 8] -> [10 -> 12 -> 15 -> 18 -> 22 -> 27 -> 33 -> 41 -> 51 -> ...

    private void IncreaseCapacity() => Capacity = Math.Max(Capacity * 5 >> 2, Capacity + 1);

    public IEnumerable<Transition> AsIEnumerable() => this;
}