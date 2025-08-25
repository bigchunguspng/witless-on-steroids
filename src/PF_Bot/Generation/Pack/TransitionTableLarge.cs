namespace PF_Bot.Generation.Pack;

public class TransitionTableLarge : TransitionTable
{
    private Transition[] _transitions;

    private int Count { get; set; }
    private int Capacity
    {
        get => _transitions.Length;
        set
        {
            var original = _transitions;
            _transitions = new Transition[value];
            for (var i = 0; i < original.Length; i++)
            {
                _transitions[i] = original[i];
            }
        }
    }

    public float TotalChance { get; private set; }

    private void IncreaseTotalChanceBy(float value)
        => TotalChance = TotalChance.CombineRound(value);

    public Transition this[int index]
    {
        get => _transitions[index];
        set => _transitions[index] = value;
    }

    public TransitionTableLarge()
    {
        _transitions = new Transition[4];
        Count = 0;
    }

    public TransitionTableLarge(IEnumerable<Transition> transitions)
    {
        _transitions = transitions.ToArray();
        Count = _transitions.Length;
    }

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

    public void Add(Transition transition)
    {
        if (Count >= Capacity) IncreaseCapacity();
        _transitions[Count] = transition;
        IncreaseTotalChanceBy(transition.Chance);
        Count++;
    }

    // ~50% of tables contain only 1 element, and ~95% - 8 or less. Consequences:
    // 1. Their capacity should be increased slowly to avoid wasting lots of space.
    // 2. Less bloated structure should be used for small tables.

    // [        TransitionTableSmall        ] -> [        TransitionTableLarge                       -> ...
    // [1 -> 2 -> 3 -> 4 -> 5 -> 6 -> 7 -> 8] -> [10 -> 12 -> 15 -> 18 -> 22 -> 27 -> 33 -> 41 -> 51 -> ...

    private void IncreaseCapacity() => Capacity = Math.Max(Capacity * 5 >> 2, Capacity + 1);

    public IEnumerable<Transition> AsIEnumerable() => _transitions.TakeWhile(x => x.Chance > 0);
}