namespace PF_Tools.Copypaster.Tokens;

public interface IConsumableToken
{
    void RememberTransition(GenerationPack db, IConsumableToken next, int chance);

    //

    public static (int low, int high) SplitChance(int chance)
    {
        var l = chance / 5;
        var h = chance - l;
        return (l, h);
    }
}