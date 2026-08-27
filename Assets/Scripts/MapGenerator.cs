using UnityEngine;

[System.Serializable]
public class MapData
{
    public int width;
    public int height;
}

public class MapGenerator : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject tilePrefab;   // tu prefab de tile ya cargado
    public float tileSize = 1f;     // tamaño de cada celda en unidades de Unity

    [Header("Debug / pruebas")]
    public TextAsset testJson;      // arrastra aquí un .json de prueba desde el Inspector

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
    }

    private void BuildGrid(int width, int height)
    {
        // Si ya existe un mapa previo, lo limpiamos primero
        ClearGrid();

        tileGrid = new GameObject[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x * tileSize, 0f, y * tileSize);
                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                tile.name = $"Tile_{x}_{y}";
                tileGrid[x, y] = tile;
            }
        }

        Debug.Log($"Mapa generado: {width} x {height} tiles.");
    }

    private void ClearGrid()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}