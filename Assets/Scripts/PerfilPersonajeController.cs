using UnityEngine;
using UnityEngine.UI;

public class PerfilPersonajeController : MonoBehaviour
{
    [Header("Referencias")]
    public Image circuloAvatar;

    [Header("Colores")]
    public Color colorNormal = new Color(0f, 0.8078432f, 0.8196079f, 1f); 
    public Color colorVictima = new Color(1f, 0.647f, 0f, 1f);

    public void MostrarSurvivor(AgentData agent)
    {
        if (agent == null || circuloAvatar == null) return;
        circuloAvatar.color = agent.victim ? colorVictima : colorNormal;
    }
}