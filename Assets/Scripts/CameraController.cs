using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    public Vector3 overviewPosition = new Vector3(100, 80, -40);
    public Vector3 overviewEulerAngles = new Vector3(90, 0, 0);

    [Header("Seguimiento")]
    public float followHeight = 80f;   // mantiene la misma altura, solo se mueve en X/Z
    public float followSpeed = 4f;     // qué tan rápido se mueve/gira la cámara

    private Transform currentTarget;
    private bool following = false;

    void Awake()
    {
        Instance = this;
    }

    void LateUpdate()
    {
        Vector3 targetPos = (following && currentTarget != null)
            ? new Vector3(currentTarget.position.x, followHeight, currentTarget.position.z)
            : overviewPosition;

        Quaternion targetRot = Quaternion.Euler(overviewEulerAngles); // siempre top-down

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * followSpeed);
    }

    public void FollowAgent(Transform target)
    {
        currentTarget = target;
        following = true;
    }

    public void ReturnToOverview()
    {
        following = false;
        currentTarget = null;
    }
}