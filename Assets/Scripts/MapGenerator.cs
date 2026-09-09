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

        if (tileGrid == null) // solo se construye la primera vez
        {
            BuildGrid(data.width, data.height);
        }

        return ApplyCells(data.tiles);
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

    private List<RevealEvent> ApplyCells(List<CellData> cells)
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

            // Las paredes se aplican siempre al instante, no son parte del "corte"
            controller.ApplyWalls(cell);

            var key = (cell.x, cell.y);
            previousCells.TryGetValue(key, out CellData prevCell);
            int prevFire = prevCell != null ? prevCell.fire : 0;
            int prevPoi = prevCell != null ? prevCell.poi : 0;

            bool esHumoNuevo   = cell.fire == 1 && prevFire != 1;
            bool esZombieNuevo = cell.fire == 2 && prevFire != 2;
            bool esPoiNuevo    = cell.poi != 0 && cell.poi != prevPoi;

            if (esHumoNuevo)
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
            else
            {
                // Nada nuevo que revelar aquí: se aplica normal, sin esperar al corte
                controller.ApplyFireAndPoi(cell);
            }

            previousCells[key] = cell;
        }

        // Orden del corte: primero humo, luego zombies, luego POIs
        return events.OrderBy(e => (int)e.type).ToList();
    }

    public void ApplyCellVisual(CellData cell)
    {
        if (tileGrid == null) return;
        if (cell.x < 0 || cell.x >= tileGrid.GetLength(0) || cell.y < 0 || cell.y >= tileGrid.GetLength(1)) return;

        GameObject tileObj = tileGrid[cell.x, cell.y];
        TileController controller = tileObj != null ? tileObj.GetComponent<TileController>() : null;
        if (controller != null) controller.ApplyFireAndPoi(cell);
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