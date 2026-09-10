using UnityEngine;
using TMPro;
using System;

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
    public static bool firstPart = true;
    [Header("Turno")]
    public TextMeshProUGUI turnoText;

    [Header("Daño")]
    public TextMeshProUGUI danioText;

    public TextMeshProUGUI focusText;

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

        UpdateFocus(data.turn);
        UpdateTurn(data.turn);
        UpdateDamage(data.buildingDamage);
        UpdateIconGroup(rescatadosIcons, data.saved, 7, true, true);
        UpdateIconGroup(infectadosIcons, data.lost, 4, false, false);
    }

    private void UpdateTurn(int turn)
    {
        if (turnoText != null) turnoText.text = turn.ToString();
    }

    private void UpdateFocus(int focus)
    {
        if(focus != 0)
        {
            if (focusText != null) focusText.text = "Agente "+(((focus-1) % 6)+1).ToString();
        }
        
    }

    private void UpdateDamage(int damage)
    {
        if (danioText != null)
        {
            danioText.text = (24 - damage).ToString();
            if(damage >= 24)
            {
                UIManager.Instance.LoseWithDelay(2f);
            }
        } 

    }

    private void UpdateIconGroup(GameObject[] icons, int activeCount, int limit, bool win, bool number)
    {
        if (icons == null) return;

        for (int i = 0; i < icons.Length; i++)
        {
            if (icons[i] != null)
            {
                icons[i].SetActive(i < activeCount);
            }
        }
        if (win)
            {
                if(activeCount >= limit)
                {
                    UIManager.Instance.WinWithDelay(2f);
                }
            }
            else
            {
                if(activeCount >= limit)
                {
                    UIManager.Instance.LoseWithDelay(2f);
                }
            }
    }
}