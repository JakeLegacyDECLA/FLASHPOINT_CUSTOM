public enum RevealType
{
    Smoke,
    Zombie,
    Poi
}

public class RevealEvent
{
    public int x;
    public int y;
    public RevealType type;
    public CellData cellData;
}