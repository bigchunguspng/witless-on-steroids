namespace PF_Bot.Generation.Pack;

public interface TransitionTable
{
    IEnumerable<Transition> AsIEnumerable();

    Transition this[int index] { get; set; }

    float TotalChance { get; }

    bool CanAccept(int id);

    public void Put   (int id, float chance)
    {
        var index = GetIndexOrAdd(id, chance);
        if (index >= 0) this[index] = this[index].WithChanceIncreasedBy(chance);
    }

    public void PutMax(int id, float chance)
    {
        var index = GetIndexOrAdd(id, chance);
        if (index >= 0) this[index] = this[index].WithMaxChance(chance, out _);
    }

    void Add(Transition transition);

    int FindIndexByID(int id)
    {
        var i = 0;
        foreach (var transition in AsIEnumerable())
        {
            if (transition.WordID == id) return i;
            i++;
        }

        return -1;
    }

    protected internal int GetIndexOrAdd(int id, float chance)
    {
        var index = FindIndexByID(id);
        if (index < 0) Add(new Transition(id, chance));

        return index;
    }

    public static void Upgrade(ref TransitionTable table)
    {
        if /**/ (table is TransitionTableSingle) table = new TransitionTableDouble(table[0]);
        else if (table is TransitionTableDouble) table = new TransitionTableSmall (table.AsIEnumerable());
        else if (table is TransitionTableSmall ) table = new TransitionTableLarge (table.AsIEnumerable());
    }
}