namespace PF_Tools.Copypaster.TransitionTables;

/// C2 = Constant size of 2 elements.
public class TransitionTableC2 : TransitionTable // 32B
{
    private Transition _transition1;
    private Transition _transition2 = Transition.Empty;

    public TransitionTableC2(Transition transition)
    {
        _transition1 = transition;
    }

    public TransitionTableC2(Transition transition1, Transition transition2)
    {
        _transition1 = transition1;
        _transition2 = transition2;
    }

    public Transition this[int index] => index == 0 ? _transition1 : _transition2;
    public IEnumerable<Transition> AsIEnumerable()
    {
        yield return _transition1;
        if (_transition2.IsNotEmpty()) yield return _transition2;
    }

    public int Count       => _transition2.IsEmpty() ? 1 : 2;
    public int TotalChance => _transition1.Chance + _transition2.Chance;

    public bool ShouldBeUpgradedToPut(int wordId)
    {
        return AsIEnumerable().Any(x => x.CanBeUsedFor(wordId)) == false;
    }

    public void Put(int wordId, int chance)
    {
        if /**/ (_transition1.IsEmpty())        _transition1 = new Transition(wordId, chance);
        else if (_transition1.WordId == wordId) _transition1 = _transition1.WithChanceIncreasedBy(chance);
        else if (_transition2.IsEmpty())        _transition2 = new Transition(wordId, chance);
        else if (_transition2.WordId == wordId) _transition2 = _transition2.WithChanceIncreasedBy(chance);
    }

    public int IndexOfWordId(int wordId)
    {
        if (_transition1.WordId == wordId) return 1;
        if (_transition2.WordId == wordId) return 2;
        return -1;
    }
}