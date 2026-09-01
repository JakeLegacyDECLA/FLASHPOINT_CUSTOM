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
        ApplyCells(data.cells); // <-- línea nueva
    }

    private void BuildGrid(int width, int height)
    {
        ClearGrid();
        tileGrid = new GameObject[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x * tileSize, 0f, -y * tileSize); // <-- cambio: signo negativo en Z
                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                tile.name = $"Tile_{x}_{y}";
                tileGrid[x, y] = tile;
            }
        }

        Debug.Log($"Mapa generado: {width} x {height} tiles.");
    }

    // ---- método nuevo ----
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
    // ---- fin método nuevo ----

    private void ClearGrid()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}