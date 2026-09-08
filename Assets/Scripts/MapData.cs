using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapData
{
    public int width;
    public int height;
    public List<CellData> tiles;
}

[System.Serializable]
public class CellData
{
    public int x;
    public int y;
    public WallsData walls;
    public int fire; // 0=empty, 1=smoke, 2=fire
    public int poi;  // 0=none, 1=exist(no revelado), 2=empty(falsa alarma), 3=victim
    public List<int> agentIds;

}

[System.Serializable]
public class WallsData
{
    public int up;
    public int down;
    public int left;
    public int right;
}

[System.Serializable]
public class AgentData
{
    public int id;
    public bool knockdown;
    public bool victim; // true = está cargando una víctima
    public int x;
    public int y;
    public int ap;
}

[System.Serializable]
public class MovementData
{
    public int step;
    public int agentId;
    public string type; // "move", "chop", etc.
    public string dir;
    public int prevX;
    public int prevY;
    public int newX;
    public int newY;
}