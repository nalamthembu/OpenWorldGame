using UnityEngine;

//This script keeps track of inputs for the vehicle, it will be used by other scripts under the vehicle object.
public class VehicleInput : MonoBehaviour
{
    [Range(0, 1)] float throttle;
    [Range(0, 1)] float brake;
    [Range(0, 1)] float handbrake;
    [Range(-1, 1)] float steering;

    [SerializeField] DebugVehicleInput m_Debug;

    private bool isInReverse;

    private float rawThrottle; //Includes negative numbers if the player/ai decides to reverse.

    #region ACCESSORS
    public float Throttle { get { return throttle; } set { throttle = value; } }
    public float RawThrottle { get { return rawThrottle; } set { rawThrottle = value; } }
    public float Brake { get { return brake; } set { brake = value; } }
    public float Handbrake { get { return handbrake; } set { handbrake = value; } }
    public float Steering { get { return steering; } set { steering = value; } }
    public bool IsInReverse { get { return isInReverse; } set { isInReverse = value; } }
    #endregion

    private void Awake()
    {
        m_Debug = new(this);
    }

    private void Update()
    {
        if (m_Debug.enabled)
            m_Debug.Update();

        FloorValues();
    }

    private void FloorValues()
    {
        brake = Mathf.Floor(brake);
        Handbrake = Mathf.Floor(Handbrake);
    }
}

[System.Serializable]
public struct DebugVehicleInput
{
    [Header("Debugging")]
    public bool enabled;

    [SerializeField][Range(0, 1)] float throttle;
    [SerializeField][Range(0, 1)] float brake;
    [SerializeField][Range(0, 1)] float handbrake;
    [SerializeField][Range(-1, 1)] float steering;

    private readonly VehicleInput vehicleInput;

    public DebugVehicleInput(VehicleInput vehicleInput)
    {
        enabled = false;
        throttle = 0;
        brake = 0;
        handbrake = 0;
        steering = 0;

        this.vehicleInput = vehicleInput;
    }

    public void Update()
    {
        vehicleInput.Throttle = throttle;
        vehicleInput.RawThrottle = throttle;
        vehicleInput.Brake = brake;
        vehicleInput.Handbrake = handbrake;
        vehicleInput.Steering = steering;
    }
}