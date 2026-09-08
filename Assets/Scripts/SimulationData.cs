using System.Collections.Generic;
using UnityEngine;

// One exported turn from the Python/Mesa simulation (see
// Flashpoint_Multiagentes/export_simulation.py). Combines the fields
// MapData and HudData each read, so JsonUtility.ToJson(frame) can be
// fed straight into GameManager.ApplyGameUpdate() unmodified.
[System.Serializable]
public class GameFrame
{
    public int turn;
    public int width;
    public int height;
    public int buildingDamage;
    public int saved;
    public int lost;
    public int focus;
    public List<CellData> cells;
}

// Wrapper around the full exported game: JsonUtility can't parse a
// bare top-level JSON array, so export_simulation.py writes
// {"frames": [...]} and this mirrors that shape.
[System.Serializable]
public class SimulationData
{
    public List<GameFrame> frames;
}
