namespace PF_Tools.Copypaster.TransitionTables;

/// V = Variable size.
public abstract class TransitionTableV
{
    public void Put(int wordId, float chance)
    {
        var index = IndexOfWordId(wordId);
        if (index >= 0) Update(index, chance);
        else Add(new Transition(wordId, chance));
    }

    public abstract int IndexOfWordId(int wordId);

    protected abstract void Update(int index, float chance);

    protected abstract void Add(Transition transition);
}