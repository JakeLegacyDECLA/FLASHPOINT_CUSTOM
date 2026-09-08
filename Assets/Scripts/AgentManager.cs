using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    [Header("Referencias")]
    public MapGenerator mapGenerator;
    public GameObject firefighterPrefab;

    [Header("Ajustes visuales")]
    public float heightOffset = 0.1f;
    [Range(0f, 0.5f)]
    public float stackOffsetFraction = 0.3f;

    private Dictionary<int, GameObject> agentInstances = new Dictionary<int, GameObject>();

    public void ApplyAgents(List<AgentData> agents)
    {
        if (agents == null || mapGenerator == null || firefighterPrefab == null) return;

        Dictionary<(int, int), List<AgentData>> byTile = new Dictionary<(int, int), List<AgentData>>();

        foreach (AgentData agent in agents)
        {
            var key = (agent.x, agent.y);
            if (!byTile.ContainsKey(key))
            {
                byTile[key] = new List<AgentData>();
            }
            byTile[key].Add(agent);
        }

        float stackOffsetRadius = mapGenerator.tileSize * stackOffsetFraction;

        foreach (var kvp in byTile)
        {
            List<AgentData> agentsInTile = kvp.Value;
            Vector3 basePos = mapGenerator.GetTileVisualPosition(kvp.Key.Item1, kvp.Key.Item2); // <-- cambio
            basePos.y += heightOffset;

            for (int i = 0; i < agentsInTile.Count; i++)
            {
                Vector3 offset = GetStackOffset(i, agentsInTile.Count, stackOffsetRadius);
                PlaceAgent(agentsInTile[i], basePos + offset);
            }
        }
    }

    private Vector3 GetStackOffset(int index, int totalInTile, float radius)
    {
        if (totalInTile <= 1) return Vector3.zero;

        float angle = (360f / totalInTile) * index;
        float rad = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;
    }

    private void PlaceAgent(AgentData agent, Vector3 worldPos)
    {
        GameObject instance;

        if (!agentInstances.TryGetValue(agent.id, out instance) || instance == null)
        {
            instance = Instantiate(firefighterPrefab, transform);
            instance.name = $"Firefighter_{agent.id}";
            agentInstances[agent.id] = instance;
        }

        instance.transform.position = worldPos;
    }
}