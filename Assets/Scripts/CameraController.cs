using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    public Vector3 overviewPosition = new Vector3(100, 70, -35);
    public Vector3 overviewEulerAngles = new Vector3(90, 0, 0);

    [Header("Seguimiento")]
    public float followHeight = 80f;
    public float followSpeed = 4f;

    private Transform currentTarget;
    private Vector3 fixedFocusPoint;
    private bool usingFixedPoint = false;
    private bool following = false;

    void Awake()
    {
        Instance = this;
    }

    void LateUpdate()
    {
        Vector3 targetPos;

        if (!following)
        {
            targetPos = overviewPosition;
        }
        else if (usingFixedPoint)
        {
            targetPos = fixedFocusPoint;
        }
        else if (currentTarget != null)
        {
            targetPos = new Vector3(currentTarget.position.x, followHeight, currentTarget.position.z);
        }
        else
        {
            targetPos = overviewPosition;
        }

        Quaternion targetRot = Quaternion.Euler(overviewEulerAngles);

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * followSpeed);
    }

    public void FollowAgent(Transform target)
    {
        currentTarget = target;
        usingFixedPoint = false;
        following = true;
    }

    public void FocusPoint(Vector3 worldPos) 
    {
        fixedFocusPoint = new Vector3(worldPos.x, followHeight, worldPos.z);
        usingFixedPoint = true;
        currentTarget = null;
        following = true;
    }

    public void ReturnToOverview()
    {
        following = false;
        usingFixedPoint = false;
        currentTarget = null;
    }
}