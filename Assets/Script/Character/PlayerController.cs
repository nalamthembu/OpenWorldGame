using UnityEngine;

[RequireComponent(typeof(CharacterStateMachine))]
public class PlayerController : Character, ICharacter
{
    public static PlayerController Instance;

    [SerializeField][Range(0, 30)] float m_JumpHeight = 1.25F;

    [SerializeField] private Transform m_CameraFocus;

    public Transform CameraFocus { get { return m_CameraFocus; } }

    //Movement Values
    private float m_TargetRotation;
    private float m_VelocityY;
    private Vector3 m_Velocity;
    private float m_MoveSpeedVelocity;
    private float m_RotSpeedVelocity;
    private float m_ControllerSpeed;
    public float ControllerSpeed { get { return m_ControllerSpeed; } }
    public float TargetRotation { get { return m_TargetRotation; } }

    private CharacterController m_CharacterController;
    public CharacterController CharacterController { get { return m_CharacterController; } }

    public bool IsJumping { get; set; }

    protected override void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;

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

    public void RotateCharacter()
    {
        Vector2 dir = PlayerInput.Instance.InputDir;

        float cameraYAxis = CameraController.Instance.transform.eulerAngles.y;

        m_TargetRotation = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg + cameraYAxis;

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

        m_CurrentSpeed = Mathf.SmoothDamp
           (
               m_CurrentSpeed,
               TargetSpeed,
               ref m_MoveSpeedVelocity,
               m_SpeedSmoothTime
           ) * PlayerInput.Instance.InputMagnitude;

        if (IsJumping)
            Jump();

        m_Velocity = (transform.forward * m_CurrentSpeed) + Vector3.up * m_VelocityY;

        m_CharacterController.Move(m_Velocity * Time.deltaTime);
    }
}