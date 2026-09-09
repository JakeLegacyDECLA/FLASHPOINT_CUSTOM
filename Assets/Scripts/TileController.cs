using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class WallDirection
{
    public GameObject doorOpenWall;
    public GameObject doorClosedWall;
    public GameObject fullWall;
    public GameObject damagedWall;
}

public class TileController : MonoBehaviour
{
    [Header("Paredes por dirección")]
    public WallDirection up;
    public WallDirection down;
    public WallDirection left;
    public WallDirection right;

    [Header("Objetos de fuego (ya existen en el prefab)")]
    public GameObject toxicCloudObj; // smoke (fire == 1)
    public GameObject zombieObj;     // fire (fire == 2)

    [Header("POI desconocido (poi == 1)")]
    public GameObject poiUnknownObj; // objeto fijo, un solo "?"

    [Header("Variantes de víctima (poi == 3, elige una al azar)")]
    public GameObject[] poiVictimVariants; // Kitty, Dog, POI1-4, Pinguin, Survivor1...

    [Header("Bombero (se activa si hay al menos un agentId en la tile)")]
    public GameObject firefighterObj;

    [Header("Animación del zombie")]
    public string moveSpeedParam = "MoveSpeed";
    public float walkSpeedValue = 1f;

    private GameObject currentVictimVariant;

    private void Awake()
    {
        SetWallDefault(up);
        SetWallDefault(down);
        SetWallDefault(left);
        SetWallDefault(right);

        if (toxicCloudObj != null) toxicCloudObj.SetActive(false);
        if (zombieObj != null) zombieObj.SetActive(false);
        if (poiUnknownObj != null) poiUnknownObj.SetActive(false);
        if (firefighterObj != null) firefighterObj.SetActive(false);
        SetAllVictimVariantsInactive();
    }

    public void ApplyCellData(CellData cell)
    {
        ApplyWalls(cell);
        ApplyFireAndPoi(cell);
    }

    public void ApplyWalls(CellData cell) // <-- nuevo (antes era parte de ApplyCellData)
    {
        ApplyWall(up, cell.walls.up);
        ApplyWall(down, cell.walls.down);
        ApplyWall(left, cell.walls.left);
        ApplyWall(right, cell.walls.right);
    }

    public void ApplyFireAndPoi(CellData cell) // <-- nuevo
    {
        ApplyFire(cell.fire);
        ApplyPoi(cell.poi);
    }
    private void SetWallDefault(WallDirection wall)
    {
        if (wall == null) return;
        if (wall.doorOpenWall != null) wall.doorOpenWall.SetActive(false);
        if (wall.doorClosedWall != null) wall.doorClosedWall.SetActive(false);
        if (wall.fullWall != null) wall.fullWall.SetActive(false);
        if (wall.damagedWall != null) wall.damagedWall.SetActive(false);
    }

    private void ApplyWall(WallDirection wall, int state)
    {
        if (wall == null) return;

        SetWallDefault(wall);

        switch (state)
        {
            case 0: break; // libre
            case 1:
                if (wall.damagedWall != null) wall.damagedWall.SetActive(true);
                break;
            case 2:
                if (wall.fullWall != null) wall.fullWall.SetActive(true);
                break;
            case 3:
                if (wall.doorOpenWall != null) wall.doorOpenWall.SetActive(true);
                break;
            case 4:
                if (wall.doorClosedWall != null) wall.doorClosedWall.SetActive(true);
                break;
        }
    }

    private void ApplyFire(int fireState)
    {
        if (toxicCloudObj != null) toxicCloudObj.SetActive(false);
        if (zombieObj != null) zombieObj.SetActive(false);

        if (fireState == 1 && toxicCloudObj != null)
        {
            toxicCloudObj.SetActive(true);
        }
        else if (fireState == 2 && zombieObj != null)
        {
            zombieObj.SetActive(true);
        }
    }

    private void SetAllVictimVariantsInactive()
    {
        if (poiVictimVariants == null) return;

        foreach (GameObject variant in poiVictimVariants)
        {
            if (variant != null) variant.SetActive(false);
        }

        currentVictimVariant = null;
    }

    private void ApplyPoi(int poiState)
    {
        if (poiUnknownObj != null) poiUnknownObj.SetActive(false);
        SetAllVictimVariantsInactive();

        if (poiState == 1 && poiUnknownObj != null)
        {
            poiUnknownObj.SetActive(true);
        }
        else if (poiState == 3 && poiVictimVariants != null && poiVictimVariants.Length > 0)
        {
            int randomIndex = Random.Range(0, poiVictimVariants.Length);
            currentVictimVariant = poiVictimVariants[randomIndex];

            if (currentVictimVariant != null)
            {
                currentVictimVariant.SetActive(true);
            }
        }
    }

    private void ApplyFirefighter(List<int> agentIds)
    {
        bool hasFirefighter = agentIds != null && agentIds.Count > 0;

        if (firefighterObj != null)
        {
            firefighterObj.SetActive(hasFirefighter);
        }
    }

    public IEnumerator PlayZombieArrival(Vector3 fromWorldPos, float walkDuration)
    {
        if (zombieObj == null) yield break;

        Vector3 finalLocalPos = zombieObj.transform.localPosition; 
        Vector3 finalWorldPos = zombieObj.transform.position;

        zombieObj.SetActive(true);
        zombieObj.transform.position = fromWorldPos;

        Vector3 dir = finalWorldPos - fromWorldPos;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
        {
            zombieObj.transform.rotation = Quaternion.LookRotation(dir);
        }

        Animator animator = zombieObj.GetComponentInChildren<Animator>();
        
        if (animator != null) animator.SetFloat(moveSpeedParam, walkSpeedValue);

        float elapsed = 0f;
        while (elapsed < walkDuration)
        {
            zombieObj.transform.position = Vector3.Lerp(fromWorldPos, finalWorldPos, elapsed / walkDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        zombieObj.transform.position = finalWorldPos;
        zombieObj.transform.localPosition = finalLocalPos; // asegura que quede exacto en su lugar

        if (animator != null) animator.SetFloat(moveSpeedParam, 0f);
    }
}