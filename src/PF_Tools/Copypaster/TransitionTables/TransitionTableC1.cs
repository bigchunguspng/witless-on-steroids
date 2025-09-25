namespace PF_Tools.Copypaster.TransitionTables;

/// C1 = Constant size of 1 element.
public class TransitionTableC1 : TransitionTable // 24B
{
    private Transition _transition = Transition.Empty;

    public TransitionTableC1() { }
    public TransitionTableC1(Transition transition)
    {
        _transition = transition;
    }

    public Transition this[int index] => _transition;
    public IEnumerable<Transition> AsIEnumerable()
    {
        if (_transition.IsNotEmpty()) yield return _transition;
    }

    public int Count       => _transition.IsEmpty() ? 0 : 1;
    public int TotalChance => _transition.Chance;

    public bool ShouldBeUpgradedToPut(int wordId)
    {
        return _transition.CanBeUsedFor(wordId) == false;
    }

    public void Put(int wordId, int chance)
    {
        if /**/ (_transition.IsEmpty())        _transition = new Transition(wordId, chance);
        else if (_transition.WordId == wordId) _transition = _transition.WithChanceIncreasedBy(chance);
    }

    public int IndexOfWordId(int wordId)
    {
        return _transition.WordId == wordId ? 1 : -1;
    }
}