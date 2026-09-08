using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.InputSystem;

// Los 3 campos que le interesan a Unity ademas del tablero: cuando
// parar de pedir turnos y por que termino la partida. El resto del
// JSON (turn/width/height/cells/etc.) ya lo leen MapData y HudData,
// asi que no hace falta duplicarlo aqui.
[System.Serializable]
public class GameStatus
{
    public bool gameOver;
    public bool win;
    public string loseReason;
}

[System.Serializable]
public class NewGameRequest
{
    public int num_firefighters = 6;
    public int seed = 42;
    public string strategy = "optimized";
}

[System.Serializable]
public class StepRequest
{
    public int count = 1;
}

// Cliente en vivo del servidor FastAPI (ver Flashpoint_Multiagentes/server.py).
// Reemplaza a SimulationPlayer.cs cuando quieres que Unity controle el
// avance turno a turno en lugar de reproducir un archivo ya grabado.
// Alimenta cada respuesta directo a GameManager.ApplyGameUpdate(), que
// no cambia entre este modo y el modo offline.
public class SimulationClient : MonoBehaviour
{
    [Header("Referencias")]
    public GameManager gameManager;

    [Header("Servidor (uvicorn server:app --port 8000)")]
    public string baseUrl = "http://localhost:8000";

    [Header("Nueva partida")]
    public int numFirefighters = 6;
    public int seed = 42;
    public string strategy = "optimized";

    [Header("Iniciar automaticamente al entrar a Play")]
    public bool autoStartOnPlay = true;

    [Header("Controles (Input System nuevo)")]
    [Tooltip("Tecla para avanzar un turno.")]
    public Key stepKey = Key.Space;
    [Tooltip("Tecla para reiniciar la partida (llama a /game/new de nuevo).")]
    public Key restartKey = Key.R;

    public GameStatus LastStatus { get; private set; }

    void Start()
    {
        if (autoStartOnPlay) StartNewGame();
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current[stepKey].wasPressedThisFrame)
        {
            StepOneTurn();
        }
        else if (Keyboard.current[restartKey].wasPressedThisFrame)
        {
            StartNewGame();
        }
    }

    // Wrapper sin parametros: los botones de UI (Button.onClick) enlazan
    // metodos de 0 argumentos mucho mas facil que uno con parametro
    // default como StepTurn(int count = 1).
    public void StepOneTurn()
    {
        if (LastStatus != null && LastStatus.gameOver)
        {
            Debug.Log("La partida ya termino. Presiona R (o el boton de reiniciar) para empezar otra.");
            return;
        }
        StepTurn();
    }

    public void StartNewGame()
    {
        NewGameRequest body = new NewGameRequest
        {
            num_firefighters = numFirefighters,
            seed = seed,
            strategy = strategy,
        };
        StartCoroutine(PostJson("/game/new", JsonUtility.ToJson(body)));
    }

    public void StepTurn(int count = 1)
    {
        StepRequest body = new StepRequest { count = count };
        StartCoroutine(PostJson("/game/step", JsonUtility.ToJson(body)));
    }

    public void FocusAgent(int agentId)
    {
        StartCoroutine(PostJson($"/game/focus/{agentId}", ""));
    }

    public void ClearFocus()
    {
        StartCoroutine(PostJson("/game/focus/clear", ""));
    }

    public void RefreshState()
    {
        StartCoroutine(GetJson("/game/state"));
    }

    private IEnumerator PostJson(string path, string jsonBody)
    {
        using (UnityWebRequest request = new UnityWebRequest(baseUrl + path, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody ?? "");
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();
            HandleResponse(path, request);
        }
    }

    private IEnumerator GetJson(string path)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(baseUrl + path))
        {
            yield return request.SendWebRequest();
            HandleResponse(path, request);
        }
    }

    private void HandleResponse(string path, UnityWebRequest request)
    {
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error llamando a {path}: {request.error} ({request.downloadHandler.text})");
            return;
        }

        string json = request.downloadHandler.text;
        LastStatus = JsonUtility.FromJson<GameStatus>(json);
        gameManager.ApplyGameUpdate(json);

        if (LastStatus != null && LastStatus.gameOver)
        {
            Debug.Log($"Juego terminado. Gano={LastStatus.win}, razon={LastStatus.loseReason}");
        }
    }
}
