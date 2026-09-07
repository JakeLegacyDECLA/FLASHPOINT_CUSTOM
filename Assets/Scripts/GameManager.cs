using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Referencias")]
    public MapGenerator mapGenerator;
    public HudController hudController;

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
    }
}