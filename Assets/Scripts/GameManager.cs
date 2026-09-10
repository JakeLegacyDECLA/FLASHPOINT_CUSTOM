using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AgentListWrapper
{
    public List<AgentData> agents;
}

[System.Serializable]
public class MovementListWrapper
{
    public List<MovementData> movements;
}

public class GameManager : MonoBehaviour
{
    [Header("Referencias")]
    public MapGenerator mapGenerator;
    public HudController hudController;
    public AgentManager agentManager;
    public MovementPlayer movementPlayer;

    [Header("Timing del corte (humo / zombies / POIs)")]
    public float revealTravelTime = 1f;   // tiempo para que la cámara llegue a la celda
    public float revealHoldTime = 1f;     // tiempo que se queda mostrando lo nuevo
    public float zombieWalkDuration = 1f; // tiempo que tarda el zombie en caminar hasta su celda

    [Header("Debug / pruebas")]
    public TextAsset testJson;

    void Start()
    {
        if (testJson != null)
        {
            ApplyGameUpdate(testJson.text);
        }
    }

    public void ApplyGameUpdate(string json)
    {
        StartCoroutine(ApplyGameUpdateSequenced(json));
    }

    private IEnumerator ApplyGameUpdateSequenced(string json)
    {
        List<RevealEvent> revealEvents = mapGenerator.GenerateMap(json);

        AgentListWrapper agentData = JsonUtility.FromJson<AgentListWrapper>(json);
        if (agentData != null)
        {
            agentManager.ApplyAgents(agentData.agents);
        }

        hudController.ApplyGameState(json);

        // 3. Fase del jugador: la cámara sigue al agente que actúa
        MovementListWrapper movementData = JsonUtility.FromJson<MovementListWrapper>(json);
        if (movementData != null && movementData.movements != null && movementData.movements.Count > 0 && movementPlayer != null)
        {
            yield return StartCoroutine(movementPlayer.PlayMovements(movementData.movements));
        }

        // 4. Corte: revela humo, zombies y POIs uno por uno
        yield return StartCoroutine(RevealEnvironmentChanges(revealEvents));

        mapGenerator.ApplyAllPendingCells(); 

    }

    private IEnumerator RevealEnvironmentChanges(List<RevealEvent> events)
    {
        if (events == null || events.Count == 0) yield break;

        foreach (RevealEvent ev in events)
        {
            Vector3 focusPos = mapGenerator.GetTileVisualPosition(ev.x, ev.y);

            if (CameraController.Instance != null)
            {
                CameraController.Instance.FocusPoint(focusPos);
            }

            yield return new WaitForSeconds(revealTravelTime);

            bool esPropagacionDeZombie = ev.type == RevealType.Zombie && ev.sourceX >= 0;

            if (esPropagacionDeZombie)
            {
                yield return mapGenerator.PlayZombiePropagation(ev, zombieWalkDuration);
            }
            else
            {
                mapGenerator.ApplyCellVisual(ev.cellData);
            }

            yield return new WaitForSeconds(revealHoldTime);
        }

        if (CameraController.Instance != null)
        {
            CameraController.Instance.ReturnToOverview();
        }
    }
}