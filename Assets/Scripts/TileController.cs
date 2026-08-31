using UnityEngine;

[System.Serializable]
public class WallDirection
{
    public GameObject doorOpenWall;   // state 3
    public GameObject doorClosedWall; // state 4
    public GameObject fullWall;       // state 2 (Wall no door)
    public GameObject damagedWall;    // state 1 (Damaged wall)
}

public class TileController : MonoBehaviour
{
    [Header("Paredes por dirección")]
    public WallDirection up;
    public WallDirection down;
    public WallDirection left;
    public WallDirection right;

    [Header("Prefabs de estado de fuego")]
    public GameObject toxicCloudPrefab; // smoke (fire == 1)
    public GameObject zombiePrefab;     // fire (fire == 2)

    [Header("Prefabs de POI")]
    public GameObject poiUnknownPrefab; // exist, no revelado
    public GameObject poiVictimPrefab;

    private GameObject currentFireObj;
    private GameObject currentPoiObj;

    public void ApplyCellData(CellData cell)
    {
        ApplyWall(up, cell.walls.up);
        ApplyWall(down, cell.walls.down);
        ApplyWall(left, cell.walls.left);
        ApplyWall(right, cell.walls.right);

        ApplyFire(cell.fire);
        ApplyPoi(cell.poi);
    }

    private void ApplyWall(WallDirection wall, int state)
    {
        if (wall == null) return;

        // Apaga todo primero
        if (wall.doorOpenWall != null) wall.doorOpenWall.SetActive(false);
        if (wall.doorClosedWall != null) wall.doorClosedWall.SetActive(false);
        if (wall.fullWall != null) wall.fullWall.SetActive(false);
        if (wall.damagedWall != null) wall.damagedWall.SetActive(false);

        switch (state)
        {
            case 0: // libre - todo queda apagado
                break;
            case 1: // pared 1 vida
                if (wall.damagedWall != null) wall.damagedWall.SetActive(true);
                break;
            case 2: // pared 2 vida
                if (wall.fullWall != null) wall.fullWall.SetActive(true);
                break;
            case 3: // puerta abierta
                if (wall.doorOpenWall != null) wall.doorOpenWall.SetActive(true);
                break;
            case 4: // puerta cerrada
                if (wall.doorClosedWall != null) wall.doorClosedWall.SetActive(true);
                break;
        }
    }

    private void ApplyFire(int fireState)
    {
        if (currentFireObj != null)
        {
            Destroy(currentFireObj);
            currentFireObj = null;
        }

        if (fireState == 1 && toxicCloudPrefab != null)
        {
            currentFireObj = Instantiate(toxicCloudPrefab, transform.position, Quaternion.identity, transform);
        }
        else if (fireState == 2 && zombiePrefab != null)
        {
            currentFireObj = Instantiate(zombiePrefab, transform.position, Quaternion.identity, transform);
        }
    }

    private void ApplyPoi(int poiState)
    {
        if (currentPoiObj != null)
        {
            Destroy(currentPoiObj);
            currentPoiObj = null;
        }

        if (poiState == 1 && poiUnknownPrefab != null)
        {
            currentPoiObj = Instantiate(poiUnknownPrefab, transform.position, Quaternion.identity, transform);
        }
        else if (poiState == 3 && poiVictimPrefab != null)
        {
            currentPoiObj = Instantiate(poiVictimPrefab, transform.position, Quaternion.identity, transform);
        }
    }
}