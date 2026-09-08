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
    public string chopTriggerParam = "Shoot";   // trigger: golpear/disparar (chop)

    [Header("Timing")]
    public float moveDuration = 0.5f; // segundos para cruzar una tile
    public float chopDuration = 0.6f; // segundos que dura la animación de chop

    public IEnumerator PlayMovements(List<MovementData> movements)
    {
        if (movements == null || movements.Count == 0) yield break;

        var byStep = movements
            .Where(m => m != null)
            .GroupBy(m => m.step)
            .OrderBy(g => g.Key);

        foreach (var stepGroup in byStep)
        {
            List<Coroutine> running = new List<Coroutine>();

            foreach (MovementData move in stepGroup)
            {
                running.Add(StartCoroutine(PlayOneMovement(move)));
            }

            // Espera a que TODOS los agentes de este step terminen antes de pasar al siguiente
            foreach (Coroutine c in running)
            {
                yield return c;
            }
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
                yield return PlayChop(agentObj, animator, move); // <-- ahora pasamos agentObj y move
                break;
            default:
                Debug.LogWarning($"Tipo de movimiento no reconocido: {move.type}");
                break;
        }
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
        // Rota al agente para que apunte hacia la dirección indicada antes de disparar
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