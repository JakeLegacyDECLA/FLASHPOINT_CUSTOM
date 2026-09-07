using UnityEngine;
using TMPro;

[System.Serializable]
public class HudData
{
    public int turn;
    public int buildingDamage;
    public int saved;
    public int lost;
    public int focus;
}

public class HudController : MonoBehaviour
{
    [Header("Turno")]
    public TextMeshProUGUI turnoText;

    [Header("Daño")]
    public TextMeshProUGUI danioText;

    [Header("Rescatados (arrastra los 7 iconos en orden)")]
    public GameObject[] rescatadosIcons;

    [Header("Infectados (arrastra los 4 iconos en orden)")]
    public GameObject[] infectadosIcons;

    public void ApplyGameState(string json)
    {
        HudData data = JsonUtility.FromJson<HudData>(json);

        if (data == null)
        {
            Debug.LogError("No se pudo parsear el JSON del HUD.");
            return;
        }

        UpdateTurn(data.turn);
        UpdateDamage(data.buildingDamage);
        UpdateIconGroup(rescatadosIcons, data.saved);
        UpdateIconGroup(infectadosIcons, data.lost);
    }

    private void UpdateTurn(int turn)
    {
        if (turnoText != null) turnoText.text = turn.ToString();
    }

    private void UpdateDamage(int damage)
    {
        if (danioText != null) danioText.text = damage.ToString();
    }

    private void UpdateIconGroup(GameObject[] icons, int activeCount)
    {
        if (icons == null) return;

        for (int i = 0; i < icons.Length; i++)
        {
            if (icons[i] != null)
            {
                icons[i].SetActive(i < activeCount);
            }
        }
    }
}