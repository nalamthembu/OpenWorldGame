#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class PlayerStateMachine : BaseCharacterStateMachine
{
    [SerializeField] bool m_DebugAiming;

    protected enum FootUp
    {
        Left,
        Right
    };

    PlayerCharacter m_PlayerCharacter;

    public PlayerState currentState;
    public IdleState idleState = new();
    public StopState stopState = new();
    public RunState runState = new();
    public WalkState walkState = new();
    public CrouchState crouchState = new();
    public ProneState proneState = new();

    [HideInInspector] public string editorString;

    public static PlayerStateMachine Instance;

    public static event System.Action NotCrouchedOrProne;

    public Vector2 InputDirection { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        m_Character = GetComponent<PlayerCharacter>();
        m_PlayerCharacter = m_Character as PlayerCharacter;
    }

    private void Start() => SwitchState(idleState);

    protected override void Update()
    {
        base.Update();

        if (m_Animator)
        {
            //Set Player Speed to match animation speed.
            PlayerSpeed = m_Animator.velocity.magnitude;
        }

        SetInputValues();

        currentState?.OnUpdate(this);
    }

    private void SetInputValues()
    {
        if (PlayerController.Instance)
        {
            InputMagnitude = PlayerController.Instance.PlayerMovement.normalized.magnitude;
            InputDirection = PlayerController.Instance.PlayerMovement.normalized;
        }
    }

    protected override void SetAnimationValues()
    {

        if (m_Animator)
        {
            //Input
            m_Animator.SetFloat(GameStrings.INPUT_MAGNITUDE, InputMagnitude);
            //Crouching
            m_Animator.SetBool(GameStrings.IS_CROUCHING, PlayerController.Instance.IsCrouched);
            //Going Prone
            m_Animator.SetBool(GameStrings.IS_PRONE, PlayerController.Instance.IsProne);
            //Jumping
            m_Animator.SetBool(GameStrings.IS_JUMPING, PlayerController.Instance.JumpPressed);
            //IsGrounded
            m_Animator.SetBool(GameStrings.IS_GROUNDED, m_PlayerCharacter.IsGrounded());

            if (m_WeaponHandler)
            {
                Gun equippedWeapon = m_WeaponHandler.GetEquippedWeapon();
                GunData equippedWeaponGunData = null;

                if (equippedWeapon)
                {
                    equippedWeaponGunData = (GunData)equippedWeapon.WeaponData;
                }

                bool isAiming = equippedWeapon != null && PlayerController.Instance != null && PlayerController.Instance.IsAiming || m_DebugAiming;

                m_Character.IsAiming = isAiming;
                m_Character.IsFiring = isAiming && PlayerController.Instance.IsFiring;

                m_Animator.SetBool(GameStrings.IS_RIFLE, equippedWeapon != null && equippedWeaponGunData.WeaponClass == WeaponClassification.Rifle);
                m_Animator.SetBool(GameStrings.IS_PISTOL, equippedWeapon != null && equippedWeaponGunData.WeaponClass == WeaponClassification.Pistol);
                m_Animator.SetBool(GameStrings.IS_AIMING, m_Character.IsAiming);
                m_Animator.SetBool(GameStrings.IS_FIRING, m_Character.IsFiring);
            }


            if (PlayerController.Instance)
            {
                //Speed, Rotation, InputX/Y Gets Set regardless of whether or not we are moving.
                m_Animator.SetFloat(GameStrings.SPEED,
                    PlayerController.Instance.RunHeld ? m_PlayerCharacter.RunSpeed : m_PlayerCharacter.WalkSpeed,
                    m_PlayerCharacter.SpeedSmoothTime, Time.deltaTime);

                m_Animator.SetFloat(GameStrings.INPUT_X, InputDirection.x,
                    m_PlayerCharacter.SpeedSmoothTime / 2, Time.deltaTime);
                m_Animator.SetFloat(GameStrings.INPUT_Y, InputDirection.y,
                    m_PlayerCharacter.SpeedSmoothTime / 2, Time.deltaTime);


                if (ThirdPersonCamera.Instance)
                {
                    //Camera XY
                    float clampedDeltaAngle = Mathf.DeltaAngle(transform.eulerAngles.y, ThirdPersonCamera.Instance.Yaw);
                    clampedDeltaAngle = Mathf.Clamp(clampedDeltaAngle, -90, 90);
                    m_Animator.SetFloat(GameStrings.CAMERA_X, ThirdPersonCamera.Instance.Pitch, 0.1F, Time.deltaTime);
                    m_Animator.SetFloat(GameStrings.CAMERA_Y, clampedDeltaAngle, 0.1F, Time.deltaTime);

                    Rotation = Mathf.Atan2(InputDirection.x,
                        InputDirection.y) * Mathf.Rad2Deg +
                        ThirdPersonCamera.Instance.transform.eulerAngles.y;

                    //if player is aiming a weapon
                    if (PlayerController.Instance.IsAiming &&
                        m_WeaponHandler &&
                        m_WeaponHandler.GetEquippedWeapon())
                    {
                        //match camera rotation 1:1
                        Rotation = ThirdPersonCamera.Instance.transform.eulerAngles.y;
                    }
                }
            }

            //Set Rotation
            m_Animator.SetFloat(GameStrings.TARGET_ROTATION,
                Mathf.DeltaAngle(transform.eulerAngles.y, Rotation),
                m_PlayerCharacter.RotationSmoothTime / 2, Time.deltaTime);

        }
    }
    
    public void ResetFeet()
    {
        //Reset stopping bools
        m_Animator.SetBool(GameStrings.LU, false);
        m_Animator.SetBool(GameStrings.RU, false);
    }

    public void SetFeet()
    {
        m_Animator.SetBool(GameStrings.LU, CheckWhichFootIsUp() == FootUp.Left);
        m_Animator.SetBool(GameStrings.RU, CheckWhichFootIsUp() == FootUp.Right);
    }

    public void RotatePlayerToFaceCameraDirection(bool IsProne = false) => m_PlayerCharacter.DoRotation(Rotation, IsProne);
    
    protected FootUp CheckWhichFootIsUp()
    {
        Vector3 leftFootPos = m_Animator.GetBoneTransform(HumanBodyBones.LeftFoot).position;
        Vector3 rightFootPos = m_Animator.GetBoneTransform(HumanBodyBones.RightFoot).position;

        if (leftFootPos.y > rightFootPos.y)
            return FootUp.Left;
        else
            return FootUp.Right;
    }

    public void SwitchState(PlayerState nextState)
    {
        if (currentState != null)
            currentState.OnExit(this);

        currentState = nextState;

        //At this point the current state is the next state.

        IsCrouchedOrProne = currentState == crouchState || currentState == proneState;

        if (!IsCrouchedOrProne)
            NotCrouchedOrProne?.Invoke();

        IsRunning = currentState == runState;

        currentState.OnEnter(this);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PlayerStateMachine))]
public class PlayerStateMachineEditor : Editor
{
    GUIStyle style = new();

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlayerStateMachine component = (PlayerStateMachine)target;

        string boxText = "???";

        style.normal.textColor = Color.green;
        switch (component.currentState)
        {
            case null:
                boxText = "CURRENT STATE IS NULL!";
                style.normal.textColor = Color.red;
                break;

            case IdleState:
                boxText = "Idle";
                break;

            case WalkState:
                boxText = "Walking";
                break;

            case RunState:
                boxText = "Running";
                break;

            case StopState:
                boxText = "Stopping";
                break;

            case CrouchState:
                boxText = "Crouching";
                break;

            case ProneState:
                boxText = "Prone";
                break;
        }

        GUILayout.Label(boxText, style);

    }
}
#endif