using UnityEngine;

/// <summary>
/// This player controller class will update the events from the vehicle player.
/// Standar coding documentation can be found in 
/// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments
/// </summary>
public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f;
    public float turnSpeed = 0.0f;
    public float horizontalInput;
    public float forwardInput;

    //Variables cámara
    public Camera mainCamera;
    public Camera hoodCamera;
    public KeyCode switchKey; //Tecla que permite cambiar entre cámaras

    //Variables multijugador
    public string inputId;

    /// <summary>
    /// This method is called before the first frame update
    /// </summary>
    void Start()
    {
        
    }
    /// <summary>
    /// This method is called once per frame
    /// </summary>
    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal" + inputId);
        forwardInput = Input.GetAxis("Vertical" + inputId);

        transform.Translate(Vector3.forward * Time.deltaTime * speed * forwardInput);
        transform.Rotate(Vector3.up, Time.deltaTime * turnSpeed * horizontalInput);

        //Cambio entre cámaras
        if(Input.GetKeyDown(switchKey))
        {
            mainCamera.enabled = !mainCamera.enabled;
            hoodCamera.enabled = !hoodCamera.enabled;
        }
    
    }
}