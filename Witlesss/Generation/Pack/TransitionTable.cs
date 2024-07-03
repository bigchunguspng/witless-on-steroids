﻿using System;
using System.Collections.Generic;

namespace Witlesss.Generation.Pack;

public class TransitionTable(int capacity = 1) : List<Transition>(capacity)
{
    public float TotalChance { get; private set; }

    private void IncreaseTotalChanceBy(float value)
        => TotalChance = TotalChance.CombineRound(value);

    public void Put(int id, float chance)
    {
        var index = GetIndexOrAdd(id, chance);
        if (index >= 0)
        {
            var transition = this[index];
            transition.IncreaseChanceBy(chance);
            this[index] = transition;
            IncreaseTotalChanceBy(chance);
        }
    }

    public void PutMax(int id, float chance)
    {
        var index = GetIndexOrAdd(id, chance);
        if (index >= 0)
        {
            var transition = this[index];
            var max = Math.Max(transition.Chance, chance);
            transition.SetChanceTo(max, out var difference);
            this[index] = transition;
            IncreaseTotalChanceBy(difference);
        }
    }

    private int GetIndexOrAdd(int id, float chance)
    {
        var index = FindIndexByID(id);
        if (index < 0)
        {
            if (Capacity == Count) IncreaseCapacity();
            Add(new Transition(id, chance));
        }

        return index;
    }

    public new void Add(Transition transition)
    {
        base.Add(transition);
        IncreaseTotalChanceBy(transition.Chance);
    }

    // High % of tables contain only 1 element, so their capacity
    // should be increased slowly to avoid wasting lots of space
    private void IncreaseCapacity() => Capacity = Math.Max(Capacity * 5 >> 2, Capacity + 1); // * 1.25  OR  + 1

    private int FindIndexByID(int id)
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