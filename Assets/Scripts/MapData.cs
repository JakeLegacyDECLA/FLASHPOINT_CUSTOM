using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapData
{
    public int width;
    public int height;
    public List<CellData> cells;
}

[System.Serializable]
public class CellData
{
    public int x;
    public int y;
    public WallsData walls;
    public int fire; // 0=empty, 1=smoke, 2=fire
    public int poi;  // 0=none, 1=exist(no revelado), 2=empty(falsa alarma), 3=victim
}

[System.Serializable]
public class WallsData
{
    public int up;
    public int down;
    public int left;
    public int right;
}