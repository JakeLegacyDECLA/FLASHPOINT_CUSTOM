using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

// Plays back a game exported by Flashpoint_Multiagentes/export_simulation.py.
// Loads Assets/StreamingAssets/<fileName>, then feeds each frame into the
// existing GameManager/MapGenerator/HudController pipeline exactly as the
// old testJson did, one frame at a time (either automatically or via
// NextFrame()/PreviousFrame() from UI buttons).
public class SimulationPlayer : MonoBehaviour
{
    [Header("Referencias")]
    public GameManager gameManager;

    [Header("Archivo (dentro de Assets/StreamingAssets)")]
    public string fileName = "simulation.json";

    [Header("Reproduccion")]
    public bool autoPlay = true;
    public float secondsPerFrame = 0.5f;

    private SimulationData simulation;
    private int currentFrame = -1;

    public int CurrentFrameIndex => currentFrame;
    public int FrameCount => simulation?.frames?.Count ?? 0;

    void Start()
    {
        StartCoroutine(LoadAndPlay());
    }

    private IEnumerator LoadAndPlay()
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        string json;

        // En Android/WebGL, streamingAssetsPath es una URL (jar:/http),
        // no un path de disco: hay que leerlo con UnityWebRequest.
        if (path.Contains("://"))
        {
            using (UnityWebRequest request = UnityWebRequest.Get(path))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"No se pudo cargar {path}: {request.error}");
                    yield break;
                }
                json = request.downloadHandler.text;
            }
        }
        else
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"No se encontro el archivo de simulacion en {path}");
                yield break;
            }
            json = File.ReadAllText(path);
        }

        simulation = JsonUtility.FromJson<SimulationData>(json);

        if (simulation == null || simulation.frames == null || simulation.frames.Count == 0)
        {
            Debug.LogError("El archivo de simulacion no contiene frames.");
            yield break;
        }

        Debug.Log($"Simulacion cargada: {simulation.frames.Count} turnos.");

        PlayFrame(0);

        if (autoPlay)
        {
            while (currentFrame < simulation.frames.Count - 1)
            {
                yield return new WaitForSeconds(secondsPerFrame);
                NextFrame();
            }
        }
    }

    public void PlayFrame(int index)
    {
        if (simulation == null || simulation.frames == null) return;
        if (index < 0 || index >= simulation.frames.Count) return;

        currentFrame = index;
        GameFrame frame = simulation.frames[currentFrame];
        gameManager.ApplyGameUpdate(JsonUtility.ToJson(frame));
    }

    public void NextFrame() => PlayFrame(currentFrame + 1);

    public void PreviousFrame() => PlayFrame(currentFrame - 1);
}
