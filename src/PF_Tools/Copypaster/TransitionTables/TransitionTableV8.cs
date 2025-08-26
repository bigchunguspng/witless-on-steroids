namespace PF_Tools.Copypaster.TransitionTables;

/// V8 = Variable size of up to 8 elements.
public class TransitionTableV8(IEnumerable<Transition> transitions) : TransitionTableV, TransitionTable // 72-112B
{
    private Transition[] _transitions = transitions.ToArray();

    public             Transition this[int index]  => _transitions[index];
    public IEnumerable<Transition> AsIEnumerable() => _transitions;

    public int   Count       => _transitions.Length;
    public float TotalChance => MathF.Round(_transitions.Sum(x => x.Chance), 1);

    public bool ShouldBeUpgradedToPut(int wordId)
    {
        return _transitions.Length >= 8 && _transitions.All(x => x.WordId != wordId);
    }

    public override int IndexOfWordId(int wordId)
    {
        return Array.FindIndex(_transitions, x => x.WordId == wordId);
    }

    protected override void Update(int index, float chance)
    {
        _transitions[index] = _transitions[index].WithChanceIncreasedBy(chance);
    }

    protected override void Add(Transition transition)
    {
        var original = _transitions;
        _transitions = new Transition[original.Length + 1];
        _transitions[^1] = transition;
        Array.Copy(original, _transitions, original.Length);
    }
}