using UnityEngine;

[System.Serializable]
public class ThirdPersonCamera : BaseCamera
{
    public static ThirdPersonCamera Instance;

    [SerializeField][Range(1, 10)] float m_DistFromTarget = 3.0F;

    [SerializeField][Range(1, 50)] float m_MaxDistance = 15.0F;

    [SerializeField] Vector2 m_Offset = new(0.5F, 0.0F);

    [SerializeField] Vector2 m_PitchLimits = new(-45, 90);

    [SerializeField] LayerMask m_CollisionLayers;

    [SerializeField] [Range(1, 10)] float m_InactivityTimeOut = 5.0F;

    [SerializeField] [Range(1, 50)] float m_Sensitivity = 10;

    private float m_InactivityTimer;

    private Vector3 m_LastMousePos;


    //Flags
    bool m_MouseHasNotMovedInAWhile;

    public override void DoUpdatePosition()
    {
        if (m_Targets.Count > 0)
        {
            Vector3 centrePoint = GetCentrePoint();

            float desiredDist = m_DistFromTarget;

            Vector3 desiredPosition = centrePoint + transform.forward * -(Mathf.Lerp(desiredDist, 50, GetGreatestDistance() / m_MaxDistance));

            // Check for camera collision
            float obstacleOffset = 0.1f; // Adjust this offset value as needed
            if (Physics.Raycast(centrePoint, -transform.forward, out RaycastHit hit, m_DistFromTarget + obstacleOffset, m_CollisionLayers))
            {
                // If there is an obstacle, pull the camera in closer with an offset
                desiredPosition = hit.point + transform.forward * obstacleOffset;
            }

            transform.position = desiredPosition;

            m_AttachedCamera.transform.localPosition = Vector3.Lerp(m_AttachedCamera.transform.localPosition, m_Offset, Time.deltaTime);
        }
    }

    private bool IsPlayerMovingMouse()
    {
        if ((m_LastMousePos - Input.mousePosition).sqrMagnitude == 0)
        {
            m_InactivityTimer += Time.deltaTime;

            m_MouseHasNotMovedInAWhile = m_InactivityTimer >= m_InactivityTimeOut;

            return false;
        }

        m_InactivityTimer = 0;
        m_MouseHasNotMovedInAWhile = false;

        return true;
    }

    public override void DoUpdateRotation()
    {
        Vector3 targetRotation;
        Quaternion desiredQuat;

        if (!IsPlayerMovingMouse() && m_MouseHasNotMovedInAWhile)
        {
            m_Pitch = Mathf.LerpAngle(m_Pitch, 0, Time.deltaTime);

            m_Yaw = Mathf.LerpAngle(m_Yaw, 0, Time.deltaTime);

            targetRotation = new()
            {
                x = m_Pitch,
                y = m_Yaw,
                z = 0
            };

            desiredQuat = Quaternion.Euler(targetRotation);

            transform.rotation = desiredQuat;

            return;
        }

        m_Yaw += PlayerInput.Instance.GetMouseX(m_Sensitivity);
        m_Pitch -= PlayerInput.Instance.GetMouseY(m_Sensitivity);
        m_Pitch = Mathf.Clamp(m_Pitch, m_PitchLimits.x, m_PitchLimits.y);

        targetRotation = new()
        {
            x = m_Pitch,
            y = m_Yaw,
            z = 0
        };

        desiredQuat = Quaternion.Euler(targetRotation);

        transform.rotation = desiredQuat;

        m_LastMousePos = Input.mousePosition;
    }


    public override void Initialise(Transform selfTransform, float initialFOV)
    {
        if (Instance == null)
        {
            Instance = this;
        }

        base.Initialise(selfTransform, initialFOV);
    }
}