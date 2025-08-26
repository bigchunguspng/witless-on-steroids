namespace PF_Tools.Copypaster.TransitionTables;

/// VU = Variable size (unlimited).
public class TransitionTableVU : TransitionTableV, TransitionTable // 128+B
{
    private Transition[] _transitions;

    public TransitionTableVU()
    {
        _transitions = new Transition[4];
        Count = 0;
    }

    public TransitionTableVU(IEnumerable<Transition> transitions)
    {
        _transitions = transitions.OrderBy(x => x.WordId).ToArray();
        Count = _transitions.Length;
        TotalChance = MathF.Round(_transitions.Sum(x => x.Chance), 1);
    }

    public             Transition this[int index]  => _transitions[index];
    public IEnumerable<Transition> AsIEnumerable() => _transitions.Take(Count);

    public int   Count       { get; private set; }
    public float TotalChance { get; private set; }

    private void IncreaseTotalChanceBy(float value)
        => TotalChance = TotalChance.CombineRound(value, 1);

    public bool ShouldBeUpgradedToPut(int wordId) => false;

    public override int IndexOfWordId(int wordId)
    {
        return Array.BinarySearch(_transitions, 0, Count, new Transition(wordId, 0), _comparer);
    }

    protected override void Update(int index, float chance)
    {
        _transitions[index] = _transitions[index].WithChanceIncreasedBy(chance);
        IncreaseTotalChanceBy(chance);
    }

    protected override void Add(Transition transition)
    {
        if (Count >= Capacity) IncreaseCapacity();
        _transitions[Count] = transition;
        IncreaseTotalChanceBy(transition.Chance);
        Count++;

        var shouldBeSorted = Count >= 2 && transition.WordId < _transitions[Count - 2].WordId;
        if (shouldBeSorted)
        {
            var i = Array.BinarySearch(_transitions, 0, Count - 1, transition, _comparer);
            if (i < 0) i = ~i;
            Array.Copy(_transitions, i, _transitions, i + 1, Count - i - 1);
            _transitions[i] = transition;
        }
    }

    private int Capacity
    {
        get => _transitions.Length;
        set
        {
            var original = _transitions;
            _transitions = new Transition[value];
            Array.Copy(original, _transitions, original.Length);
        }
    }

    private void IncreaseCapacity() => Capacity = Math.Max(Capacity * 5 >> 2, Capacity + 1);

    private static readonly Comparer<Transition> _comparer = Comparer<Transition>.Create ((a, b) => a.WordId.CompareTo(b.WordId));
}