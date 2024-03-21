using UnityEngine;
using System; 
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ThirdPersonCamera : BaseCamera
{
    [SerializeField] bool m_AutoFocusOnPlayer = true;
    [SerializeField] bool m_AutoRecentre;
    [SerializeField] Vector2 m_PitchLimits = new(-40, 80);
    [SerializeField] float m_MaxIdleTime = 10;
    [SerializeField] protected float m_CameraOffsetSmoothTime = 1; //How long does it take for the camera to get to the correct offset?
    [SerializeField] float m_InterpolationSpeed = 15;
    [SerializeField] LayerMask m_CollisionMask; //What layers are we supposed to collide with?
    [SerializeField] float m_FieldOfViewAdjustTimeWhileAiming = 0.5F;
    bool m_CameraHasNotMovedForExtendedPeriod;
    bool m_DisablePlayerControl;
    Vector3 m_LastCameraRotation;
    float m_IdleTimer; //How long have we been idle.
    protected bool m_IsAiming;

    //Refs
    protected Vector3 RfOffsetVelocity;

    //for calculating camera speed
    float m_CameraSpeed;

    //as far as the camera can see (metres).
    public float FarZ { get { return m_CameraComp.farClipPlane; } }

    public float Pitch { get { return m_Pitch; } }
    public float Yaw { get { return m_Yaw; } }

    public static ThirdPersonCamera Instance;

    protected override void Awake()
    {
        if (Instance != this)
            Instance = this;
        else
            Destroy(gameObject);

        base.Awake();
    }

    private void OnPlayerProne() 
    { 
        m_CurrentCameraSettings = m_CameraSettingsData.ProneSettings;
        m_UsingProneCamSettings = true;
        m_UsingCrouchCamSettings = false;
    }

    private void OnPlayerCrouch()
    {
        m_CurrentCameraSettings = m_CameraSettingsData.CrouchSettings;
        m_UsingCrouchCamSettings = true;
        m_UsingProneCamSettings = false;
    }

    protected override void Start()
    {
        base.Start();

        if (m_AutoFocusOnPlayer)
        {
            //Find the player
            m_Target = FindObjectOfType<PlayerCharacter>().transform.Find("CameraFocus");
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        ProneState.OnPlayerProne += OnPlayerProne;
        PlayerStateMachine.NotCrouchedOrProne += OnPlayerOnFoot;
        CrouchState.OnPlayerCrouch += OnPlayerCrouch;
        PlayerController.OnInventoryHold += OnInventoryHeldOpen;
        PlayerController.OnInventoryClose += OnInventoryClosed;
        PlayerController.OnStartAim += OnPlayerStartAiming;
        PlayerController.OnStopAim += OnPlayerStopAiming;
    }

    protected virtual void OnPlayerStartAiming()
    {
        if (PlayerWeaponHandler.Instance && PlayerWeaponHandler.Instance.GetEquippedWeapon() != null)
        {
            m_IsAiming = true;
        }
    }

    protected virtual void OnPlayerStopAiming()
    {
        m_IsAiming = false;
    }

    private void OnInventoryHeldOpen() => m_DisablePlayerControl = true;
    private void OnInventoryClosed() => m_DisablePlayerControl = false;

    private void OnPlayerOnFoot()
    {
        m_CurrentCameraSettings = m_CameraSettingsData.cameraSettings[m_CameraSettingsIndex];

        //Reset crouch and prone
        m_UsingProneCamSettings = false;
        m_UsingCrouchCamSettings = false;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        ProneState.OnPlayerProne -= OnPlayerProne;
        PlayerStateMachine.NotCrouchedOrProne -= OnPlayerOnFoot;
        CrouchState.OnPlayerCrouch -= OnPlayerCrouch;
        PlayerController.OnInventoryHold -= OnInventoryHeldOpen;
        PlayerController.OnInventoryClose -= OnInventoryClosed;
        PlayerController.OnStartAim -= OnPlayerStartAiming;
        PlayerController.OnStopAim -= OnPlayerStopAiming;
    }

    protected override void DoFOV()
    {
        m_CameraSpeed = m_CameraComp.velocity.magnitude;

        if (PlayerController.Instance)
        {
            if (!PlayerController.Instance.IsProne &&
                !PlayerController.Instance.IsCrouched
                && !m_IsAiming)
            {
                if (m_CameraSpeed <= 0.01F || !PlayerController.Instance.MovementInputPressed)
                    return;
            }
        }

        m_CameraComp.fieldOfView =
            Mathf.SmoothDamp
            (
                m_CameraComp.fieldOfView,
                m_IsAiming ? m_CurrentCameraSettings.fieldOfViewAiming :
                m_CurrentCameraSettings.fieldOfView,
                ref rfFOVVelocity,
                m_IsAiming ? m_FieldOfViewAdjustTimeWhileAiming :
                m_FieldOfViewAdjustTime
            );
    }

    protected override void DoPosition()
    {
        base.DoPosition();
        if (m_Target == null)
        {
            Debug.LogError("There is no target specified!");
            enabled = false;
            return;
        }

        float distanceFromTarget = m_IsAiming ? m_CurrentCameraSettings.distanceFromTargetAiming :
            m_CurrentCameraSettings.distanceFromTarget;


        Vector3 desiredPosition = m_Target.position - transform.forward * distanceFromTarget;

        //Camera Collision
        if (Physics.Raycast(m_Target.position, -transform.forward, out var hit, distanceFromTarget, m_CollisionMask, QueryTriggerInteraction.Ignore))
            desiredPosition = hit.point + transform.forward * m_CameraComp.nearClipPlane;

        transform.position = m_IsAiming ? desiredPosition : Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * m_InterpolationSpeed);
        HandleCameraComponentPosition();
    }

    protected virtual void HandleCameraComponentPosition()
    {
        Vector3 offset = m_IsAiming ?
            m_CurrentCameraSettings.xyAimOffset :  
            m_CurrentCameraSettings.xyOffset;

        //Set the camera offset
        m_CameraComp.transform.localPosition =
        Vector3.SmoothDamp(m_CameraComp.transform.localPosition,
        offset, ref RfOffsetVelocity, m_CameraOffsetSmoothTime);


        //Do oscilation effect if the player is running 
        //**NOTE** this only changes rotation.
        if (m_OscilationEffectEnabled &&
            PlayerStateMachine.Instance.IsRunning &&
            !PlayerStateMachine.Instance.IsCrouchedOrProne)
        {
            DoOscillationEffect(m_CameraComp.velocity.magnitude / m_CameraMaxSpeedAtPeakOscillation, OscillationAxis.X);
        }
        else
        {
            //return the cameras rotation to the identity matrix.
            m_CameraComp.transform.localRotation =
                Quaternion.Lerp(m_CameraComp.transform.localRotation,
                Quaternion.identity, Time.deltaTime * 2);
        }
    }

    private bool IsPlayerMovingCamera()
    {
        if (PlayerController.Instance == null)
            return false;

        if ((m_LastCameraRotation - (Vector3) PlayerController.Instance.CameraXY).sqrMagnitude == 0)
        {
            m_IdleTimer += Time.deltaTime;

            m_CameraHasNotMovedForExtendedPeriod = m_IdleTimer >= m_MaxIdleTime;

            return false;
        }

        m_IdleTimer = 0;
        m_CameraHasNotMovedForExtendedPeriod = false;

        return true;
    }

    //This will recenter the camera after a set "Idle time".
    private void DoIdleCamera()
    {
        m_Pitch = Mathf.LerpAngle(m_Pitch, 0, Time.deltaTime);

        m_Yaw = Mathf.LerpAngle(m_Yaw, 0, Time.deltaTime);
    }


    private Vector3 GetPitchAndYaw()
    {
        Vector3 targetRotation = new()
        {
            x = m_Pitch,
            y = m_Yaw,
            z = 0
        };

        return targetRotation;
    }


    private Quaternion GetCameraQuaternion() => Quaternion.Euler(GetPitchAndYaw());


    protected override void DoRotation()
    {
        if (m_DisablePlayerControl)
            return;

        base.DoRotation();

        if (m_AutoRecentre)
        {
            if (PlayerController.Instance &&
                !IsPlayerMovingCamera() &&
                !PlayerController.Instance.CameraMoved
                && m_CameraHasNotMovedForExtendedPeriod)
            {
                DoIdleCamera();

                transform.rotation = GetCameraQuaternion();

                return;
            }
        }

        if (PlayerController.Instance != null)
        {
            m_Yaw += PlayerController.Instance.CameraXY.x * dSensitivity;
            m_Pitch -= PlayerController.Instance.CameraXY.y * dSensitivity;
            m_Pitch = Mathf.Clamp(m_Pitch, m_PitchLimits.x, m_PitchLimits.y);
        }

        if (PlayerController.Instance != null && PlayerController.Instance.IsAiming)
        {
            transform.rotation = GetCameraQuaternion();
        }
        else
        {
            transform.rotation = m_IsAiming ? GetCameraQuaternion() :  Quaternion.Lerp(transform.rotation, GetCameraQuaternion(), Time.deltaTime * m_InterpolationSpeed);
            m_LastCameraRotation = GetPitchAndYaw();
        }
    }
}

#region EDITORSCRIPT

//This is the editor script that allows the dev to use debugging features.
#if UNITY_EDITOR
[CustomEditor(typeof(ThirdPersonCamera))]
public class TPSCameraEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // for other non-HideInInspector fields

        BaseCamera cameraComp = (BaseCamera)target;

        string debugCameraStatus = cameraComp.m_DebugCameraEnabled ? "ENABLED" : "DISABLED";

        //Do we enabled debug mode?
        if (GUILayout.Button("Debug Camera : " + debugCameraStatus))
        {
            cameraComp.m_DebugCameraEnabled = !cameraComp.m_DebugCameraEnabled;
        }

        //Show debug settings if enabled.
        if (cameraComp.m_DebugCameraEnabled)
        {
            GUILayout.Label("----------Debug Camera Settings-----------");
            cameraComp.dSpeed = EditorGUILayout.FloatField("Debug Camera Speed", cameraComp.dSpeed);
            cameraComp.dZoom = EditorGUILayout.FloatField("Debug Camera Zoom (°)", cameraComp.dZoom);
            cameraComp.dSensitivity = EditorGUILayout.FloatField("Debug Camera Sensitivity", cameraComp.dSensitivity);
        }
    }
}
#endif

#endregion