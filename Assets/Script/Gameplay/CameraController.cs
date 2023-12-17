using System;
using UnityEngine;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    [SerializeField] float mouseSensitivity = 5F;
    [SerializeField] Transform target;
    [SerializeField] Vector2 pitchMinMax = new(-40, 85);
    [SerializeField] float distanceFromTarget = 2F;
    [SerializeField] CameraSetting[] cameraSettings;
    [SerializeField] DebugCamera _debug;
    [SerializeField] bool IsInputDisabled;

    Dictionary<CamType, CameraSetting> camSetDic = new();

    [SerializeField] private LayerMask m_ObstructionLayer = -1;

    public static event Action DebugFreeCamEnabled;

    float pitch;
    float yaw;
    float lastRecordedPitch;
    float lastRecordedYaw;
    float fovVel;

    new Camera camera;

    public static CameraController Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        camera = GetComponentInChildren<Camera>();

        if (!camera)
            Debug.LogError("Make sure there is a camera as a child object");

        InitDictionary();
    }

    public void SetTarget(Transform target) => this.target = target;

    private void InitDictionary()
    {
        camSetDic.Clear();

        for (int i = 0; i < cameraSettings.Length; i++)
        {
            camSetDic.Add(cameraSettings[i].cameraType, cameraSettings[i]);
        }
    }

    private void OnEnable() { GameStateMachine.OnPaused += OnGamePaused; GameStateMachine.OnResume += OnGameResumed; }
    private void OnDisable() { GameStateMachine.OnPaused -= OnGamePaused; GameStateMachine.OnResume -= OnGameResumed; }

    private void Update() => HandleInput();

    private void OnValidate()
    {
        InitDictionary();

        _debug.OnValidate();

        for (int i = 0; i < cameraSettings.Length; i++)
            cameraSettings[i].OnValidate();

    }

    private void LateUpdate()
    {
        //Debug Free Cam (Kinda like Unreal Engines)
        if (_debug.enabled)
        {
            //Enable/Disable FreeCam
            if (Input.GetKeyDown(KeyCode.Keypad9))
            {
                DebugFreeCamEnabled?.Invoke();
                _debug.debug_FreeCam = !_debug.debug_FreeCam;
            }

            if (_debug.debug_FreeCam)
            {
                Vector3 debugTargetRot = new(pitch, yaw);

                transform.eulerAngles = debugTargetRot;

                float speed = Input.GetKey(KeyCode.LeftShift) ? 100.0F : 25.0F;

                transform.position += speed * Time.deltaTime * (transform.forward * PlayerInput.Instance.InputDir.y + transform.right * PlayerInput.Instance.InputDir.x);

                if (Input.GetKey(KeyCode.Q))
                {
                    transform.position += speed * Time.deltaTime * Vector3.up;
                }

                if (Input.GetKey(KeyCode.E))
                {
                    transform.position += speed * Time.deltaTime * Vector3.down;
                }

                _debug.debug_fov += Input.mouseScrollDelta.y * 5;

                camera.fieldOfView = Mathf.SmoothDamp(camera.fieldOfView, _debug.debug_fov, ref fovVel, 0.15F);

                return;
            }
        }

        if (IsInputDisabled)
            return;


        Vector3 targetRotation = new(pitch, yaw);

        transform.eulerAngles = targetRotation;

        bool isAiming = false;

        camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, isAiming ? camSetDic[CamType.AIMING].offset : camSetDic[CamType.NORMAL].offset, Time.deltaTime * 1.5F);

        Vector3 desiredCamPos = target.position - transform.forward * distanceFromTarget;

        // Check for camera collision
        float obstacleOffset = 0.1f; // Adjust this offset value as needed
        if (Physics.Raycast(target.position, -transform.forward, out RaycastHit hit, distanceFromTarget + obstacleOffset, m_ObstructionLayer))
        {
            // If there is an obstacle, pull the camera in closer with an offset
            desiredCamPos = hit.point + transform.forward * obstacleOffset;
        }

        transform.position = desiredCamPos;

        camera.fieldOfView = Mathf.SmoothDamp(camera.fieldOfView, isAiming ? camSetDic[CamType.AIMING].fov : camSetDic[CamType.NORMAL].fov, ref fovVel, 0.001F);
    }

    public Vector2 GetCameraPitchYaw()
    {
        return new Vector2(pitch, yaw);
    }

    private void OnGamePaused()
    {
        IsInputDisabled = true;

        //Keep track of last pitch and yaw.
        lastRecordedPitch = pitch;
        lastRecordedYaw = yaw;
    }

    private void OnGameResumed()
    {
        IsInputDisabled = false;

        //Set pitch and yaw to what we last remember
        pitch = lastRecordedPitch;
        yaw = lastRecordedYaw;
    }

    private void HandleInput()
    {

        if (PlayerInput.Instance.IsHoldingInventoryKey)
            return;

        yaw += PlayerInput.Instance.GetMouseX(mouseSensitivity);
        pitch -= PlayerInput.Instance.GetMouseY(mouseSensitivity);
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        if (!_debug.enabled)
            return;

        for (int i = 0; i < cameraSettings.Length; i++)
        {
            if (cameraSettings[i].cameraType == _debug.debug_camera_type)
            {
                Vector3 origin = transform.position + (Vector3)cameraSettings[i].offset;

                Vector3 endPoint = transform.forward;

                if (camera != null)
                {
                    origin = camera.transform.position;
                    endPoint = camera.transform.forward;
                }

                endPoint *= _debug.debug_farClipPlane;

                Gizmos.DrawRay(origin, endPoint);

                break;
            }
        }
    }
}

[System.Serializable]
public struct CameraSetting
{
    public CamType cameraType;
    public float fov;
    public Vector2 offset;

    public void OnValidate()
    {
        if (fov <= 0)
        {
            fov = 60.0F;
        }
    }
}

[System.Serializable]
public struct DebugCamera
{
    public CamType debug_camera_type;
    public float debug_nearClipPlane;
    public float debug_farClipPlane;
    public float debug_fov;
    public bool enabled;
    public bool debug_FreeCam;

    public void OnValidate()
    {
        if (debug_fov <= 0)
        {
            debug_fov = 60.0F;
        }

        if (debug_farClipPlane <= 0)
            debug_farClipPlane = 1000.0F;
    }
}

public enum CamType
{
    AIMING,
    NORMAL,
    VEHICLE
}