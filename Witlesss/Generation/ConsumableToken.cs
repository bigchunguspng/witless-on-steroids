namespace Witlesss.Generation;

public interface IConsumableToken
{
    void RememberTransition(GenerationPack db, IConsumableToken next);
}

/// <summary>
/// Combined word token (2 words, 1 space).
/// </summary>
public readonly struct CombinedToken(int id1, int id2, int idC) : IConsumableToken
{
    public int ID1 { get; } = id1;
    public int ID2 { get; } = id2;
    public int IDC { get; } = idC;

    public void RememberTransition(GenerationPack db, IConsumableToken next)
    {
        db.GetTableByID(ID1).Put(ID2, 1F);

        var table2 = db.GetTableByID(ID2);
        var tableC = db.GetTableByID(IDC);

        if (next is SingleToken simple)
        {
            table2.Put(simple.ID, 1F);
            tableC.Put(simple.ID, 1F);
        }
        else if (next is CombinedToken combined)
        {
            table2.Put(combined.ID1, 0.1F);
            table2.Put(combined.IDC, 2.9F);
            tableC.Put(combined.ID1, 0.1F);
            tableC.Put(combined.IDC, 2.9F);
        }
    }
}

/// <summary>
/// Single word token (no spaces).
/// </summary>
public readonly struct SingleToken(int id) : IConsumableToken
{
    public int ID { get; } = id;

    public void RememberTransition(GenerationPack db, IConsumableToken next)
    {
        var table = db.GetTableByID(ID);

        if (next is SingleToken simple)
        {
            table.Put(simple.ID, 1F);
        }
        else if (next is CombinedToken combined)
        {
            table.Put(combined.ID1, 0.1F);
            table.Put(combined.IDC, 2.9F);
        }
    }
}