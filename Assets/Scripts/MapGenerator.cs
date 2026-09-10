using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject tilePrefab;
    public float tileSize = 1f;

    [Header("Debug / pruebas")]
    public TextAsset testJson;

    private GameObject[,] tileGrid;
    private Dictionary<(int, int), CellData> previousCells = new Dictionary<(int, int), CellData>();
    private Dictionary<(int, int), CellData> pendingExtinguish = new Dictionary<(int, int), CellData>();

    void Start()
    {
        if (testJson != null)
        {
            GenerateMap(testJson.text);
        }
    }

    public List<RevealEvent> GenerateMap(string json)
    {
        MapData data = JsonUtility.FromJson<MapData>(json);

        if (data == null)
        {
            Debug.LogError("No se pudo parsear el JSON del mapa.");
            return new List<RevealEvent>();
        }

        bool esPrimeraGeneracion = (tileGrid == null);

        if (esPrimeraGeneracion)
        {
            BuildGrid(data.width, data.height);
        }

        return ApplyCells(data.tiles, esPrimeraGeneracion);
    }

    private void BuildGrid(int width, int height)
    {
        ClearGrid();
        tileGrid = new GameObject[width, height];
        previousCells.Clear();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = GetWorldPosition(x, y);
                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                tile.name = $"Tile_{x}_{y}";
                tileGrid[x, y] = tile;
            }
        }

        Debug.Log($"Mapa generado: {width} x {height} tiles.");
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x * tileSize, 0f, -y * tileSize);
    }

    private List<RevealEvent> ApplyCells(List<CellData> cells, bool esPrimeraGeneracion)
    {
        List<RevealEvent> events = new List<RevealEvent>();
        if (cells == null) return events;

        foreach (CellData cell in cells)
        {
            if (cell.x < 0 || cell.x >= tileGrid.GetLength(0) ||
                cell.y < 0 || cell.y >= tileGrid.GetLength(1))
            {
                Debug.LogWarning($"Celda fuera de rango: ({cell.x},{cell.y})");
                continue;
            }

            GameObject tileObj = tileGrid[cell.x, cell.y];
            TileController controller = tileObj.GetComponent<TileController>();

            if (controller == null)
            {
                Debug.LogWarning("El prefab de tile no tiene componente TileController.");
                continue;
            }

            controller.ApplyWalls(cell);

            var key = (cell.x, cell.y);

            if (esPrimeraGeneracion)
            {
                controller.ApplyFireAndPoi(cell);
                previousCells[key] = cell;
                continue;
            }

            previousCells.TryGetValue(key, out CellData prevCell);
            int prevFire = prevCell != null ? prevCell.fire : 0;
            int prevPoi = prevCell != null ? prevCell.poi : 0;

            bool esHumoNuevo   = cell.fire == 1 && prevFire != 1;
            bool esZombieNuevo = cell.fire == 2 && prevFire != 2;
            bool esPoiNuevo    = cell.poi != 0 && cell.poi != prevPoi;
            bool esExtincion   = prevFire >= 1 && cell.fire < prevFire;

            if (esZombieNuevo && prevFire == 0)
            {
                (int sx, int sy) = FindAdjacentZombie(cell);
                events.Add(new RevealEvent
                {
                    x = cell.x, y = cell.y, type = RevealType.Zombie, cellData = cell,
                    sourceX = sx, sourceY = sy
                });
            }
            else if (esHumoNuevo)
            {
                events.Add(new RevealEvent { x = cell.x, y = cell.y, type = RevealType.Smoke, cellData = cell });
            }
            else if (esZombieNuevo)
            {
                events.Add(new RevealEvent { x = cell.x, y = cell.y, type = RevealType.Zombie, cellData = cell });
            }
            else if (esPoiNuevo)
            {
                events.Add(new RevealEvent { x = cell.x, y = cell.y, type = RevealType.Poi, cellData = cell });
            }
            else if (esExtincion)
            {
                pendingExtinguish[key] = cell;
            }
            else
            {
                controller.ApplyFireAndPoi(cell);
            }

            previousCells[key] = cell;
        }

        return events.OrderBy(e => (int)e.type).ToList();
    }

    private (int x, int y) FindAdjacentZombie(CellData cell)
    {
        var dirs = new (int dx, int dy, int wallState)[]
        {
            (0, -1, cell.walls.up),
            (0, 1, cell.walls.down),
            (-1, 0, cell.walls.left),
            (1, 0, cell.walls.right),
        };

        foreach (var (dx, dy, wallState) in dirs)
        {
            bool pasable = wallState == 0 || wallState == 3;
            if (!pasable) continue;

            int nx = cell.x + dx, ny = cell.y + dy;
            if (previousCells.TryGetValue((nx, ny), out CellData neighborPrev) && neighborPrev.fire == 2)
            {
                return (nx, ny);
            }
        }

        return (-1, -1);
    }

    public IEnumerator PlayZombiePropagation(RevealEvent ev, float walkDuration)
    {
        if (tileGrid == null) yield break;
        if (ev.x < 0 || ev.x >= tileGrid.GetLength(0) || ev.y < 0 || ev.y >= tileGrid.GetLength(1)) yield break;

        GameObject tileObj = tileGrid[ev.x, ev.y];
        TileController controller = tileObj != null ? tileObj.GetComponent<TileController>() : null;
        if (controller == null) yield break;

        Vector3 fromWorldPos = (ev.sourceX >= 0)
            ? GetTileVisualPosition(ev.sourceX, ev.sourceY)
            : GetTileVisualPosition(ev.x, ev.y);

        yield return controller.PlayZombieArrival(fromWorldPos, walkDuration);
    }

    public void ApplyCellVisual(CellData cell)
    {
        if (tileGrid == null) return;
        if (cell.x < 0 || cell.x >= tileGrid.GetLength(0) || cell.y < 0 || cell.y >= tileGrid.GetLength(1)) return;

        GameObject tileObj = tileGrid[cell.x, cell.y];
        TileController controller = tileObj != null ? tileObj.GetComponent<TileController>() : null;
        if (controller != null) controller.ApplyFireAndPoi(cell);
    }

    public bool TryConsumePendingCell(int x, int y, out CellData cell)
    {
        if (pendingExtinguish.TryGetValue((x, y), out cell))
        {
            pendingExtinguish.Remove((x, y));
            return true;
        }
        cell = null;
        return false;
    }

    public void ApplyAllPendingCells()
    {
        if (tileGrid == null) return;
        foreach (var kvp in pendingExtinguish)
        {
            ApplyCellVisual(kvp.Value);
        }
        pendingExtinguish.Clear();
    }

    private void ClearGrid()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public Vector3 GetTileVisualPosition(int x, int y)
    {
        if (tileGrid == null || x < 0 || x >= tileGrid.GetLength(0) || y < 0 || y >= tileGrid.GetLength(1))
        {
            return GetWorldPosition(x, y);
        }

        GameObject tileObj = tileGrid[x, y];
        if (tileObj == null) return GetWorldPosition(x, y);

        Transform floor = tileObj.transform.Find("Tile");
        return floor != null ? floor.position : tileObj.transform.position;
    }
}