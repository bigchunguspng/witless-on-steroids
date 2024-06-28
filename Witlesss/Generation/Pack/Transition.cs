namespace Witlesss.Generation.Pack;

public struct Transition(int wordID, float chance)
{
    public int   WordID { get; }              = wordID;
    public float Chance { get; private set; } = chance;

    public void IncreaseChanceBy(float value) => Chance = Chance.CombineRound(value);

    public void SetChanceTo(float value, out float difference)
    {
        difference = value - Chance;
        Chance = value;
    }
}