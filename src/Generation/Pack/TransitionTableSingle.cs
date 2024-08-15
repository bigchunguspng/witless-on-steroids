namespace Witlesss.Generation.Pack;

public class TransitionTableSingle : TransitionTable // 24B
{
    private Transition _transition = Transition.Impossible;

    public float TotalChance => _transition.Chance;

    public Transition this[int index]
    {
        get => _transition;
        set => _transition = value;
    }

    public bool CanAccept(int id)
    {
        return _transition.WordID == id || _transition.WordID == GenerationPack.NO_WORD;
    }

    public void Add(Transition transition) => _transition = transition;

    public IEnumerable<Transition> AsIEnumerable()
    {
        if (_transition.IsPossible()) yield return _transition;
    }
}