using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject tilePrefab;
    public float tileSize = 1f;

    [Header("Debug / pruebas")]
    public TextAsset testJson;

    private GameObject[,] tileGrid;

    void Start()
    {
        if (testJson != null)
        {
            GenerateMap(testJson.text);
        }
    }

    public void GenerateMap(string json)
    {
        MapData data = JsonUtility.FromJson<MapData>(json);

        if (data == null)
        {
            Debug.LogError("No se pudo parsear el JSON del mapa.");
            return;
        }

        BuildGrid(data.width, data.height);
        ApplyCells(data.tiles);
    }

    private void BuildGrid(int width, int height)
    {
        ClearGrid();
        tileGrid = new GameObject[width, height];

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

    private void ApplyCells(List<CellData> cells)
    {
        if (cells == null) return;

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

            if (controller != null)
            {
                controller.ApplyCellData(cell);
            }
            else
            {
                Debug.LogWarning($"El prefab de tile no tiene componente TileController.");
            }
        }
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