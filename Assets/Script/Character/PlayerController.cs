using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(CharacterStateMachine))]
public class PlayerController : Character, ICharacter
{
    public static PlayerController Instance;

    [SerializeField][Range(0, 30)] float m_JumpHeight = 1.25F;

    [SerializeField] private Transform m_CameraFocus;

    public Transform CameraFocus { get { return m_CameraFocus; } }

    [Header("Debugging")]
    [SerializeField] public DebugPlayer m_DebugPlayer;

    //Movement Values
    private float m_TargetRotation;
    private float m_DeltaAngle;
    private float m_VelocityY;
    private Vector3 m_Velocity;
    private float m_MoveSpeedVelocity;
    private float m_RotSpeedVelocity;
    public float TargetRotation { get { return m_TargetRotation; } }

    private CharacterController m_CharacterController;
    public CharacterController CharacterController { get { return m_CharacterController; } }

    public bool IsJumping { get; set; }

    protected override void Awake()
    {
        if (Instance is null)
            Instance = this;
        else
            Destroy(gameObject);

        base.Awake();

        m_CharacterController = GetComponent<CharacterController>();
    }

    public void EnterVehicle(Vehicle vehicle)
    {

    }

    public void EquipWeapon()
    {

    }

    public void ExitVehicle()
    {

    }

#if UNITY_EDITOR
    protected void OnDrawGizmos()
    {
        if (m_DebugPlayer.showPlayerRotationValues)
        {
            if (PlayerInput.Instance || CameraController.Instance)
            {
                // Draw a line for player's forward direction
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + transform.forward);

                // Draw a line for the rotation forward direction
                Quaternion targetRotation = Quaternion.Euler(0f, m_TargetRotation, 0f);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + targetRotation * Vector3.forward);


                // Draw a handle to display the degrees
                //Clamped target rotation.
                float targetRot = (m_TargetRotation + 360f) % 360f;

                Vector3 targetGizmoPos = transform.position + (Vector3)m_DebugPlayer.RotationGizmoPos;


                Handles.Label(targetGizmoPos, targetRot.ToString("F2") + " °");

                // Calculate the angle difference relative to the player's forward direction
                float angleDifference = m_DeltaAngle;
                Handles.Label(targetGizmoPos + Vector3.up * 0.15F, "Angle Difference: " + angleDifference.ToString("F2") + " °");
            }
        }
        if (m_DebugPlayer.showPlayerSpeedValues)
        {
            Handles.Label(transform.position + (Vector3) m_DebugPlayer.SpeedGizmoPos, "Player Speed Values : " + CurrentSpeed.ToString("F2"));
        }

    }
#endif

    public void CalculateRotation()
    {
        Vector2 dir = PlayerInput.Instance.InputDir;

        float cameraYAxis = CameraController.Instance.transform.eulerAngles.y;

        m_TargetRotation = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg + cameraYAxis;

        m_DeltaAngle = Mathf.DeltaAngle(transform.eulerAngles.y, m_TargetRotation);
    }

    public void CalculateSpeed()
    {
        m_CurrentSpeed = Mathf.SmoothDamp
        (
            m_CurrentSpeed,
            TargetSpeed,
            ref m_MoveSpeedVelocity,
            m_SpeedSmoothTime
        );
    }

    public float GetAngle()
    {
        return m_DeltaAngle;
    }

    public void RotateCharacter()
    {
        transform.eulerAngles = Vector3.up *
            Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                m_TargetRotation,
                ref m_RotSpeedVelocity,
                m_RotSmoothTime);
    }

    private void Jump()
    {
        float g = m_Gravity;
        float h = m_JumpHeight;
        m_VelocityY = Mathf.Sqrt(-2 * g * h);
        IsJumping = false;
    }

    public void MoveCharacter()
    {
        m_VelocityY += m_Gravity * Time.deltaTime;

        if (IsGrounded())
            m_VelocityY = 0;


        if (IsJumping)
            Jump();

        m_Velocity = (transform.forward * m_CurrentSpeed) + Vector3.up * m_VelocityY;

        m_CharacterController.Move(m_Velocity * Time.deltaTime);
    }
}

[System.Serializable]
public struct DebugPlayer
{
    public bool showPlayerRotationValues;
    public bool showPlayerSpeedValues;

    public Vector2 SpeedGizmoPos, RotationGizmoPos;
}