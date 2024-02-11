using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// This is the base class for the various types of cameras such as the ThirdPersonCamera.
/// </summary>
public class BaseCamera : MonoBehaviour
{
    [HideInInspector] public bool m_DebugCameraEnabled;

    [Header("----------General----------")]
    [SerializeField] protected CameraSettingsData m_CameraSettingsData;
    [Tooltip("This is how long it takes for the field of view to adjust to the correct values.")]
    [SerializeField][Range(0, 10)] protected float m_FieldOfViewAdjustTime;
    [Tooltip("This is what the camera focuses on.")]
    [SerializeField] protected Transform m_Target;

    protected Camera m_CameraComp;

    public Camera CameraComponent { get { return m_CameraComp; } }

    /// <summary>
    /// This is the index of which camera setting this camera is using from m_CameraSettingsData.
    /// </summary>
    protected int m_CameraSettingsIndex;
    protected CameraSettings m_CurrentCameraSettings;
    protected bool m_UsingProneCamSettings, m_UsingCrouchCamSettings;

    //ref floats
    protected float rfFOVVelocity;

    //Debug Cam Settings ('d' stands to debug)
    [Tooltip("This is the speed rate of the debug camera.")]
    [HideInInspector][Min(1)] public float dSpeed;
    [HideInInspector][Min(2)] public float dZoom = 60; //Degrees (field of view)
    [HideInInspector][Range(0.001F, 2)] public float dSensitivity = 0.25F;
    private bool dIsFrozen;

    [Header("----------Handheld Effect----------")]
    [SerializeField] protected bool m_HandHeldEffectEnabled;
    [SerializeField][Min(0.01F)] float m_HandheldRange;
    [SerializeField][Min(0.01F)] protected float m_HandHeldSmoothing;
    Vector3 m_HandHeldVelocity;

    [Header("----------Oscillation Effect----------")]
    [SerializeField] protected bool m_OscilationEffectEnabled = true;
    [SerializeField][Min(0.01F)] float m_OscillationRange;
    [SerializeField][Min(0.01F)] protected float m_OscillationSmoothing;
    [SerializeField][Min(0.01F)] protected float m_CameraMaxSpeedAtPeakOscillation;


    //Camera floats
    protected float m_Pitch, m_Yaw;

    protected virtual void OnEnable()
    {
        PlayerController.OnChangeCameraView += OnChangeCameraView;
    }

    protected virtual void OnDisable()
    {
        PlayerController.OnChangeCameraView -= OnChangeCameraView;
    }

    protected void DoHandHeldEffect(float percent, float smoothing = 5)
    {
        Vector3 handHeldPosition = new()
        {
            x = Mathf.Sin(Time.time * m_HandHeldSmoothing) * Random.Range(-m_HandheldRange, m_HandheldRange),
            y = Mathf.Cos(Time.time * m_HandHeldSmoothing) * Random.Range(-m_HandheldRange, m_HandheldRange),
        };

        handHeldPosition *= Mathf.Lerp(0, 1, Mathf.Clamp01(percent));

        m_CameraComp.transform.localPosition = Vector3.SmoothDamp(m_CameraComp.transform.localPosition, handHeldPosition, ref m_HandHeldVelocity, smoothing);
    }

    protected void DoOscillationEffect(float percent, OscillationAxis axis)
    {
        percent = percent > 1 ? 1 : percent;

        float Oscillation = Mathf.Sin(Time.time * m_OscillationSmoothing) * m_OscillationRange;

        switch(axis)
        {
            case OscillationAxis.X:

                m_CameraComp.transform.localEulerAngles += Oscillation * percent * Vector3.right;

                break;

            case OscillationAxis.Y:

                m_CameraComp.transform.localEulerAngles += Oscillation * percent * Vector3.up;

                break;

            case OscillationAxis.Z:

                m_CameraComp.transform.localEulerAngles += Oscillation * percent * Vector3.forward;

                break;
        }
    }

    protected virtual void Awake()
    {
        //TODO : Load saved settings from storage.

        m_CameraComp = GetComponentInChildren<Camera>();

        if (!m_CameraSettingsData)
        {
            Debug.LogError("There is no camera settings data object attached!");
            enabled = false;
        }
        else
        {
            m_CurrentCameraSettings = m_CameraSettingsData.cameraSettings[m_CameraSettingsIndex];
        }
    }


    protected virtual void Start() { }

    protected virtual void Update()
    {
        if (m_DebugCameraEnabled)
        {
            UseDebugCamera();
            return;
        }
    }

    protected virtual void LateUpdate()
    {
        if (m_DebugCameraEnabled)
            return;

        UseCameraSettings();
    }

    protected virtual void DoRotation() { /* This should be implemented in child classes. */ }
    protected virtual void DoPosition() { /* This should be implemented in child classes. */ }
    protected virtual void DoFOV() { /* This should be implemented in child classes. */ }

    private void OnChangeCameraView()
    {
        if (m_UsingProneCamSettings || m_UsingCrouchCamSettings)
            return;

        m_CameraSettingsIndex++;

        if (m_CameraSettingsIndex > m_CameraSettingsData.cameraSettings.Length - 1)
        {
            m_CameraSettingsIndex = 0;
        }

        m_CurrentCameraSettings = m_CameraSettingsData.cameraSettings[m_CameraSettingsIndex];
    }

    protected virtual void UseCameraSettings()
    {
        if (m_CameraSettingsData != null && m_CameraComp != null) 
        {
            if (m_Target != null)
            {
                DoRotation();
                DoPosition();
                DoFOV();
            }
        }
    }

    void UseDebugCamera()
    {
        if (!PlayerController.Instance)
            return;

        //Toggle camera freeze.
        if (Input.GetKeyDown(KeyCode.F))
            dIsFrozen = !dIsFrozen;

        //This sounds kinda funny actually.
        if (dIsFrozen)
            return;

        //Position
        Vector3 deltaMovement =
            (transform.forward * PlayerController.Instance.PlayerMovement.y) +
            (transform.right * PlayerController.Instance.PlayerMovement.x);

        if (Input.GetKey(KeyCode.Q))
            deltaMovement += Vector3.up;
        else if (Input.GetKey(KeyCode.E))
            deltaMovement += Vector3.down;

        dSpeed += Input.mouseScrollDelta.y;
        dSpeed = Mathf.Clamp(dSpeed, 1, 100);

        deltaMovement *= dSpeed * Time.deltaTime;

        transform.position += deltaMovement;

        //TODO : ROTATION IS UTTERLY BROKEN.

        //Rotation
        m_Yaw += PlayerController.Instance.CameraXY.x * dSensitivity;
        m_Pitch -= PlayerController.Instance.CameraXY.y * dSensitivity;

        Vector3 targetRotation = new()
        {
            x = m_Pitch,
            y = m_Yaw,
            z = 0
        };

        m_Pitch = Mathf.Clamp(m_Pitch, -40, 80);

        Quaternion desiredQuat = Quaternion.Euler(targetRotation);

        transform.rotation = desiredQuat;
    }
}

#region EDITOR
//This is the editor script that allows the dev to use debugging features.
#if UNITY_EDITOR
[CustomEditor(typeof(BaseCamera))]
public class CameraEditorScript : Editor
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

public enum OscillationAxis { X,Y,Z };