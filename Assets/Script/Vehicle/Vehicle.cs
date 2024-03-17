using System.Collections.Generic;
using UnityEngine;

[
    RequireComponent
    (
        typeof(VehicleInput),
        typeof(VehicleEngine),
        typeof(VehicleTransmission)
    )
]

[
    RequireComponent
    (
        typeof(Rigidbody),
        typeof(VehicleVisuals)
    )
]

public class Vehicle : Entity
{
    //TO-DO : ABS ON WHEELS.

    public Axis[] axes;

    public Seat[] seats;

    public SecuritySystem securitySystem;

    public Horn horn;

    private Rigidbody rigidBody;

    private VehicleInput input;

    private VehicleEngine engine;

    private VehicleTransmission transmission;

    private Axis steeringAxis;
    readonly private List<Axis> poweredAxis = new();
    readonly private List<Wheel> poweredWheels = new();
    readonly private List<Wheel> allWheels = new();
    readonly private List<Wheel> steeringWheels = new();

    public Axis SteeringAxis { get { return steeringAxis; } }
    public List<Axis> PoweredAxis { get { return poweredAxis; } }
    public List<Wheel> AllWheels { get { return allWheels; } }
    public List<Wheel> SteeringWheels { get { return steeringWheels; } }
    public bool ABSActive { get; private set; }
    public float SpeedKMH { get { return rigidBody.velocity.magnitude * 3.5F; } }
    public VehicleEngine Engine { get { return engine; } }
    public VehicleTransmission Transmission { get { return transmission; } }

    public Seat DriversSeat { get; private set; }

    //ORIENTATION RESET
    private const float TIME_BEFORE_AUTORESET = 3;
    private float currentResetTimer = 0;

    protected override void Awake()
    {
        base.Awake();

        input = GetComponent<VehicleInput>();

        engine = GetComponent<VehicleEngine>();

        transmission = GetComponent<VehicleTransmission>();

        rigidBody = GetComponent<Rigidbody>();

        rigidBody.mass = Mathf.Round(rigidBody.mass < 800 ? 800 : rigidBody.mass);

        for (int i = 0; i < axes.Length; i++)
        {
            if (axes[i].isPowered)
            {
                poweredAxis.Add(axes[i]);
            }

            for (int j = 0; j < axes[i].wheels.Length; j++)
            {
                allWheels.Add(axes[i].wheels[j]);

                if (axes[i].isSteering)
                {
                    if (!steeringAxis.Equals(axes[i]))
                        steeringAxis = axes[i];

                    steeringWheels.Add(axes[i].wheels[j]);
                }

                if (axes[i].isPowered)
                {
                    poweredWheels.Add(axes[i].wheels[j]);
                }
            }
        }

        SetMaxSteerAngle();

        InitialiseSecuritySystem();

        InitialiseHorn();

        InitialiseDriversSeat();
    }

    private void InitialiseDriversSeat()
    {
        foreach(Seat s in seats)
        {
            if (s.GetSeatType() == SeatType.DRIVER)
            {
                DriversSeat = s;
            }
        }
    }

    private void OnValidate()
    {
        securitySystem.OnValidate();
    }

    protected override void Start()
    {
        base.Start();

        if (securitySystem != null)
        {
            securitySystem.Start();
            horn.Start();
        }
    }

    protected override void Update()
    {
        if (securitySystem.IsEngaged)
            securitySystem.Update();

        //horn.Update();

        //horn.disturbanceDetected = securitySystem.DetectedDisturbance;
    }
   
    private void SetMaxSteerAngle()
    {
        for (int i = 0; i < axes.Length; i++)
        {
            if (!axes[i].isSteering)
                continue;

            for (int j = 0; j < axes[i].wheels.Length; j++)
            {

                axes[i].wheels[0].SetMaxSteeringAngle(Mathf.Abs(
                    Mathf.Rad2Deg *
                    Mathf.Atan(2.55F / (axes[i].steerRadius + (1.5f / 2)))
                    * 1));

                axes[i].wheels[1].SetMaxSteeringAngle(Mathf.Abs(
                    Mathf.Rad2Deg *
                    Mathf.Atan(2.55F / (axes[i].steerRadius - (1.5f / 2)))
                    * 1));
            }
        }
    }

    private void FixedUpdate()
    {
        ControlVariableDrag();
        ControlVariableWheelStiffness();
        ControlSteering();
        ControlABS();

        if (IsFlippedOver())
        {
            currentResetTimer += Time.deltaTime;

            if (currentResetTimer >= TIME_BEFORE_AUTORESET)
            {
                transform.localPosition += Vector3.up;

                transform.localEulerAngles *= 0;

                currentResetTimer = 0;
            }
        }
    }

    public bool IsFlippedOver() => Vector3.Dot(transform.up, Vector3.up) <= -0.50F;

    private void ControlVariableDrag()
    {
        rigidBody.drag = (Mathf.Floor(input.Throttle) == 0 || transmission.IsChangingGear) ?
            Mathf.Lerp(rigidBody.drag, 0.2F, Time.deltaTime) :
            Mathf.Lerp(rigidBody.drag, 0.005F, Time.deltaTime);
    }

    private void ControlVariableWheelStiffness()
    {
        float speedT = SpeedKMH / 170.0F;

        float sF_Powered, fF_Powered;

        for (int i = 0; i < poweredWheels.Count; i++)
        {
            sF_Powered = Mathf.Lerp(0.05F, 3.0F, speedT);
            fF_Powered = Mathf.Lerp(0.1f, 3.0F, speedT);

            poweredWheels[i].SetWheelStiffness(sF_Powered, fF_Powered);
        }
    }

    public float GetPoweredWheelSlip()
    {
        float totalSlip = 0;

        for (int i = 0; i < poweredAxis.Count; i++)
        {
            foreach (Wheel wheel in poweredAxis[i].wheels)
            {
                totalSlip += (wheel.WheelSlip.forward + wheel.WheelSlip.sideways) / poweredAxis[i].wheels.Length;
            }
        }

        totalSlip /= poweredAxis.Count;

        return totalSlip;
    }

    private void ControlABS()
    {
        float maxWheelSlippage = 0.95F;

        for (int i = 0; i < allWheels.Count; i++)
        {
            ABSActive = Mathf.Abs(allWheels[i].WheelSlip.forward) > maxWheelSlippage && input.Brake > 0;

            if (ABSActive)
            {
                allWheels[i].SetBrakeTorque(0);
            }
        }
    }

    private void ControlSteering()
    {
        for (int i = 0; i < axes.Length; i++)
        {
            if (!axes[i].isSteering)
                continue;

            for (int j = 0; j < axes[i].wheels.Length; j++)
            {
                if (input.Steering > 0)
                {
                    axes[i].wheels[0].SetSteerAngle(
                        Mathf.Rad2Deg *
                        Mathf.Atan(2.55F / (axes[i].steerRadius + (1.5f / 2)))
                        * input.Steering);

                    axes[i].wheels[1].SetSteerAngle(
                        Mathf.Rad2Deg *
                        Mathf.Atan(2.55F / (axes[i].steerRadius - (1.5f / 2)))
                        * input.Steering);
                }
                else if (input.Steering < 0)
                {
                    axes[i].wheels[0].SetSteerAngle(
                        Mathf.Rad2Deg *
                        Mathf.Atan(2.55F / (axes[i].steerRadius - (1.5f / 2)))
                        * input.Steering);

                    axes[i].wheels[1].SetSteerAngle(
                        Mathf.Rad2Deg *
                        Mathf.Atan(2.55F / (axes[i].steerRadius + (1.5f / 2)))
                        * input.Steering);
                }
            }
        }
    }

    public bool IsGrounded()
    {
        foreach (Axis axis in poweredAxis)
        {
            foreach (Wheel w in axis.wheels)
            {
                //IF ANY OF THE WHEELS ARE NOT TOUCHING THE GROUND
                if (!w.IsGrounded)
                    return false;
            }
        }

        //THIS ASSUMES ALL THE WHEELS ARE TOUCHING THE GROUND.
        return true;
    }

    private void InitialiseSecuritySystem() => securitySystem.Initialise(this);

    private void InitialiseHorn() => horn.Initialise(this);

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        //If the car got hit at a high velocity
        if (collision.relativeVelocity.magnitude >= 5.0F)
            securitySystem.NotifySecuritySystemOfDisturbance();
        
        //if the car got hit by a projectile 
        if (collision.collider.TryGetComponent<Projectile>(out _))
        {
            securitySystem.NotifySecuritySystemOfDisturbance();
        }
    }

}

[System.Serializable]
public struct Axis
{
    public Wheel[] wheels;
    public bool isPowered;
    public bool isSteering;
    public float steerRadius;

    public override bool Equals(object obj)
    {
        return obj is Axis axis &&
               EqualityComparer<Wheel[]>.Default.Equals(wheels, axis.wheels) &&
               isPowered == axis.isPowered &&
               isSteering == axis.isSteering &&
               steerRadius == axis.steerRadius;
    }
}

[System.Serializable]
public class Seat
{
    [SerializeField] SeatType seatType;

    [SerializeField] Transform seatTransform;

    [SerializeField] Transform entryPoint;

    public Transform GetEntryPoint() => entryPoint;

    public bool IsOccupied;

    public SeatType GetSeatType() => seatType;
}

public enum SeatType
{
    LEFT_BACK,
    RIGHT_BACK,
    PASSENGER,
    DRIVER
}

[System.Serializable]
public class SecuritySystem
{
    [SerializeField] bool disturbanceDetected;

    [SerializeField] float alarmTimeout;

    public bool IsEngaged;
    public bool DetectedDisturbance => disturbanceDetected;

    private Vehicle vehicle;
    private AudioSource alarmAudioSource;
    float currentTime;

    int assignedSoundID;

    public void Initialise(Vehicle vehicle)
    {
        this.vehicle = vehicle;

        currentTime = 0;

        //Random chance of having alarm system engaged.
        IsEngaged = true; // Random.Range(0, 100) >= 50;

        disturbanceDetected = false;

        //Init AudioSource.
        alarmAudioSource = vehicle.gameObject.AddComponent<AudioSource>();

        alarmAudioSource.spatialBlend = 1;

        alarmAudioSource.minDistance = 1;

        alarmAudioSource.maxDistance = 20;

        alarmAudioSource.playOnAwake = false;

        alarmAudioSource.dopplerLevel = 0.1F;

        alarmAudioSource.enabled = false;
    }

    public void NotifySecuritySystemOfDisturbance() => disturbanceDetected = true;

    public void Start()
    {
        if (!vehicle)
        {
            Debug.LogError("There is no vehicle assigned to this Security system.");
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.TryGetInGameSound("VehicleFX_Alarm", out Sound sound);

            assignedSoundID = Random.Range(0, sound.clips.Length);
        }
    }

    public void OnValidate()
    {
        //Alarm will turn off after 15 seconds.
        if (alarmTimeout <= 0)
            alarmTimeout = 15.0F;
    }

    public void Update()
    {
        if (disturbanceDetected)
        {
            //While alarm is on, count how long its been going off.
            if (alarmAudioSource.isPlaying)
            {
                currentTime += Time.deltaTime;

                //If we reach the timeout, turn off the alarm. Disable the AudioSource (optimisation).
                if (currentTime >= alarmTimeout)
                {
                    alarmAudioSource.Stop();
                    alarmAudioSource.enabled = false;
                    disturbanceDetected = false;
                    currentTime = 0;
                }

                return;
            }


            //Set off alarm
            if (!alarmAudioSource.enabled)
            {
                alarmAudioSource.enabled = true;

                SoundManager.Instance.PlayInGameSound("VehicleFX_Alarm", assignedSoundID, alarmAudioSource, true, out _);
            }
        }
    }
}

[System.Serializable]
public class Horn
{
    [HideInInspector] public bool disturbanceDetected;
    [SerializeField] float alarmDelay;
    [SerializeField] float audioLoopPointMS;
    private Vehicle vehicle;
    private AudioSource hornAudioSource;
    float alarmTimer;
    public bool enabled;

    public void Initialise(Vehicle vehicle)
    {
        this.vehicle = vehicle;

        alarmTimer = 0;

        disturbanceDetected = false;

        //Init AudioSource.
        hornAudioSource = this.vehicle.gameObject.AddComponent<AudioSource>();

        hornAudioSource.spatialBlend = 1;

        hornAudioSource.minDistance = 5;

        hornAudioSource.maxDistance = 20;

        hornAudioSource.playOnAwake = false;

        hornAudioSource.dopplerLevel = 0.1F;

        hornAudioSource.enabled = false;

        hornAudioSource.loop = true;
    }

    public void Start()
    {
        if (SoundManager.Instance.TryGetInGameSound("VehicleFX_Horn", out Sound sound))
        {
            hornAudioSource.clip = sound.GetRandomClip();
        }
    }


    public void Update()
    {
        if (disturbanceDetected)
        {
            AlarmMode();

            return;
        }

        if (enabled)
        {
            if (!hornAudioSource.enabled)
                hornAudioSource.enabled = true;

            if (!hornAudioSource.isPlaying)
            {
                hornAudioSource.Play();
            }
        }
        else
        {
            if (hornAudioSource.enabled)
                hornAudioSource.enabled = false;

            hornAudioSource.Stop();
        }
    }

    private void AlarmMode()
    {
        if (!hornAudioSource.enabled)
            hornAudioSource.enabled = true;

        alarmTimer += Time.deltaTime;

        if (alarmTimer >= alarmDelay)
        {
            if (!hornAudioSource.isPlaying)
                hornAudioSource.Play();
            else
                hornAudioSource.Stop();

            alarmTimer = 0;
        }
    }
}