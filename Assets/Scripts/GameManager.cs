using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AgentListWrapper
{
    public List<AgentData> agents;
}

public class GameManager : MonoBehaviour
{
    [Header("Referencias")]
    public MapGenerator mapGenerator;
    public HudController hudController;
    public AgentManager agentManager;

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
        mapGenerator.GenerateMap(json);
        hudController.ApplyGameState(json);

        AgentListWrapper agentData = JsonUtility.FromJson<AgentListWrapper>(json);
        if (agentData != null)
        {
            agentManager.ApplyAgents(agentData.agents);
        }
    }
}