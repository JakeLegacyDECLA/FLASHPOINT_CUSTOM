using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovementPlayer : MonoBehaviour
{
    [Header("Referencias")]
    public AgentManager agentManager;
    public MapGenerator mapGenerator;

    [Header("Animator - nombres de parámetros (ajusta si no coinciden)")]
    public string runBoolParam = "IsRunning";   // bool: true mientras camina
    public string chopTriggerParam = "Shoot";   // trigger: golpear/disparar 

    [Header("Timing")]
    public float moveDuration = 0.5f;
    public float chopDuration = 0.6f; // segundos que dura la animación de chop
    public float victimHoldTime = 0.8f; // segundos que la víctima se queda visible antes de absorberse

    [Header("UI")]
    public PerfilPersonajeController perfilPersonaje;

    [Header("Debug: clic derecho en este componente > 'Debug: reveal de víctima'")]
    public int debugVictimX = 0;
    public int debugVictimY = 0;
    public int debugVictimAgentId = 0;

    private List<MovementData> currentMovements;

    public IEnumerator PlayMovements(List<MovementData> movements)
    {
        if (movements == null || movements.Count == 0) yield break;
        currentMovements = movements;

        var byStep = movements
            .Where(m => m != null)
            .GroupBy(m => m.step)
            .OrderBy(g => g.Key);

        foreach (var stepGroup in byStep)
        {
            int activeAgentId = stepGroup.First().agentId;
            GameObject activeAgentObj = agentManager.GetAgentInstance(activeAgentId);

            if (activeAgentObj != null && CameraController.Instance != null)
            {
                CameraController.Instance.FollowAgent(activeAgentObj.transform);
            }
            AgentData activeAgentData = agentManager.GetAgentData(activeAgentId);
            if (activeAgentData != null && perfilPersonaje != null)
            {
                perfilPersonaje.MostrarSurvivor(activeAgentData);
            }
            List<Coroutine> running = new List<Coroutine>();
            foreach (MovementData move in stepGroup)
            {
                running.Add(StartCoroutine(PlayOneMovement(move)));
            }

            foreach (Coroutine c in running)
            {
                yield return c;
            }
        }

        if (CameraController.Instance != null)
        {
            CameraController.Instance.ReturnToOverview();
        }
    }

    private IEnumerator PlayOneMovement(MovementData move)
    {
        if (agentManager == null || mapGenerator == null) yield break;

        GameObject agentObj = agentManager.GetAgentInstance(move.agentId);
        if (agentObj == null)
        {
            Debug.LogWarning($"No se encontró el agente con id {move.agentId} para animar.");
            yield break;
        }

        Animator animator = agentObj.GetComponentInChildren<Animator>();

        switch (move.type)
        {
            case "move":
                yield return PlayMove(agentObj, animator, move);
                break;
            case "chop":
                yield return PlayChop(agentObj, animator, move);
                break;
            case "turnOver":
                yield return PlayTurnOver(agentObj, move);
                break;
            default:
                Debug.LogWarning($"Tipo de movimiento no reconocido: {move.type}");
                break;
        }
    }

    // "turnOver" = el bombero reveló el POI de su celda. Python ya resolvió si
    // era víctima o falsa alarma antes de exportar el turno, así que se deduce
    // del resultado: el agente terminó el turno cargando víctima, o la salvó
    // en un paso posterior de este mismo turno. Falsa alarma -> nada que mostrar.
    private IEnumerator PlayTurnOver(GameObject agentObj, MovementData move)
    {
        AgentData data = agentManager.GetAgentData(move.agentId);
        bool savedLater = currentMovements != null && currentMovements.Any(m =>
            m != null && m.agentId == move.agentId && m.type == "saveVictim" && m.step > move.step);
        bool isVictim = (data != null && data.victim) || savedLater;
        if (!isVictim) yield break;

        // El agente puede seguir moviéndose después del turnOver en el mismo
        // turno, y ApplyAgents ya lo dejó en su posición final: se ancla a la
        // celda del POI mientras dura la animación.
        agentObj.transform.position = GetAgentWorldPosition(move.prevX, move.prevY);

        yield return PlayVictimReveal(move.prevX, move.prevY, agentObj.transform, data);
    }

    private IEnumerator PlayVictimReveal(int x, int y, Transform agentTransform, AgentData data)
    {
        TileController tile = mapGenerator.GetTileController(x, y);
        if (tile == null)
        {
            Debug.LogWarning($"No hay TileController en ({x},{y}) para revelar la víctima.");
            yield break;
        }

        yield return tile.PlayVictimAppear();
        yield return new WaitForSeconds(victimHoldTime);

        Vector3 absorbTarget = agentTransform != null ? agentTransform.position : GetAgentWorldPosition(x, y);
        yield return tile.PlayVictimAbsorb(absorbTarget);

        if (data != null && perfilPersonaje != null)
        {
            perfilPersonaje.MostrarSurvivor(data);
        }
    }

    [ContextMenu("Debug: reveal de víctima")]
    private void DebugPlayVictimReveal()
    {
        GameObject agentObj = agentManager != null ? agentManager.GetAgentInstance(debugVictimAgentId) : null;
        StartCoroutine(PlayVictimReveal(debugVictimX, debugVictimY, agentObj != null ? agentObj.transform : null, null));
    }

    private IEnumerator PlayMove(GameObject agentObj, Animator animator, MovementData move)
    {
        Vector3 startPos = GetAgentWorldPosition(move.prevX, move.prevY);
        Vector3 endPos = GetAgentWorldPosition(move.newX, move.newY);

        agentObj.transform.position = startPos;

        Vector3 dir = endPos - startPos;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
        {
            agentObj.transform.rotation = Quaternion.LookRotation(dir);
        }

        if (animator != null) animator.SetBool(runBoolParam, true);

        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            agentObj.transform.position = Vector3.Lerp(startPos, endPos, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        agentObj.transform.position = endPos;

        if (animator != null) animator.SetBool(runBoolParam, false);
    }

    private IEnumerator PlayChop(GameObject agentObj, Animator animator, MovementData move)
    {
        Vector3 dir = DirToVector(move.dir);
        if (dir.sqrMagnitude > 0.0001f)
        {
            agentObj.transform.rotation = Quaternion.LookRotation(dir);
        }

        if (animator != null) animator.SetTrigger(chopTriggerParam);
        yield return new WaitForSeconds(chopDuration);
    }

    private Vector3 GetAgentWorldPosition(int x, int y)
    {
        Vector3 pos = mapGenerator.GetTileVisualPosition(x, y);
        pos.y += agentManager.heightOffset;
        return pos;
    }

    private Vector3 DirToVector(string dir)
    {
        switch (dir)
        {
            case "up":    return Vector3.forward;  // +Z
            case "down":  return Vector3.back;     // -Z
            case "left":  return Vector3.left;     // -X
            case "right": return Vector3.right;    // +X
            default:
                Debug.LogWarning($"Dirección de chop no reconocida: {dir}");
                return Vector3.zero;
        }
    }
}