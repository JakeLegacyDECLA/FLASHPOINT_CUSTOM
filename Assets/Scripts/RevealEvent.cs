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

    //Para el zombie 
    public int sourceX = -1; 
    public int sourceY = -1;
}