namespace PF_Tools.Copypaster_Legacy.Pack; // ReSharper disable ReplaceWithPrimaryConstructorParameter

public class TransitionTableDouble(Transition transition) : TransitionTable // 32B
{
    private Transition _transition1 = transition;
    private Transition _transition2 = Transition.Impossible;

    public float TotalChance => _transition1.Chance.CombineRound(_transition2.Chance, 1);

    public Transition this[int index]
    {
        get => index == 0 ? _transition1 : _transition2;
        set
        {
            if (index == 0) _transition1 = value;
            else            _transition2 = value;
        }
    }

    public bool CanAccept(int id)
    {
        return AsIEnumerable().Any(x => x.WordID == id || x.WordID == GenerationPack.NO_WORD);
    }

    public void Add(Transition transition)
    {
        var firstEmpty = _transition1.WordID == GenerationPack.NO_WORD;
        if (firstEmpty)  _transition1 = transition;
        else             _transition2 = transition;
    }

    public IEnumerable<Transition> AsIEnumerable()
    {
        yield return _transition1;
        if (_transition2.IsPossible()) yield return _transition2;
    }
}