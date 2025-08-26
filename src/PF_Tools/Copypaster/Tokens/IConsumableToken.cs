namespace PF_Tools.Copypaster.Tokens;

public interface IConsumableToken
{
    void RememberTransition(GenerationPack db, IConsumableToken next, float chance);

    //

    public static (float low, float high) SplitChance(float chance)
    {
        var l = MathF.Round(chance * 0.2F, 1);
        var h = MathF.Round(chance - l,    1);
        return (l, h);
    }
}