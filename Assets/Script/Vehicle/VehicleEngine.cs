using UnityEngine;
using System.Collections;

public class VehicleEngine : MonoBehaviour
{
    private VehicleTransmission transmission;

    private VehicleInput input;

    private float totalPower;

    private float engineRPM;

    [SerializeField] private bool IsStartingEngine;

    [SerializeField] private bool IsRunning;


    [SerializeField][Range(0.001F, 2.0F)] float engineResponseTime;

    public float EnginePower { get { return totalPower; } }

    public float Throttle { get { return input.Throttle; } }

    public float MaxRPM { get { return transmission.powerData.maxRPM; } }

    public float RPM { get { return engineRPM; } }

    public float IdleRPM { get { return transmission.powerData.idleRPM; } }

    private AudioSource starterSource;

    private void Awake()
    {
        transmission = GetComponent<VehicleTransmission>();
        input = GetComponent<VehicleInput>();

        InitialiseStarterAudioSource();
    }

    private void InitialiseStarterAudioSource()
    {
        starterSource = gameObject.AddComponent<AudioSource>();
        starterSource.spatialBlend = 1;
        starterSource.maxDistance = 5;
        starterSource.minDistance = 0.75F;
        starterSource.playOnAwake = false;
        starterSource.enabled = false;
    }

    private void Update()
    {
        if (IsStartingEngine)
        {
            StartCoroutine(StartEngine());
            IsStartingEngine = false;
        }
    }

    private void FixedUpdate()
    {
        if (IsRunning)
            CalculateEnginePower();
    }

    protected virtual void CalculateEnginePower()
    {
        float gearRatio = input.IsInReverse ? transmission.powerData.reverseGearRatio : transmission.powerData.gearRatios[transmission.CurrentGear];

        totalPower = transmission.powerData.torqueCurve.Evaluate(engineRPM) * (gearRatio);

        totalPower *= input.RawThrottle;

        if (input.Brake > 0 && Mathf.Floor(transmission.DrivetrainRPM) > 0)
            totalPower *= input.Throttle;

        float velocity = 0;

        if (transmission.IsChangingGear)
        {
            totalPower *= 0;
        }

        totalPower *= 2.5F; //multiply by 2.5F because the cars are so damn slow.


        //REV_LIMITER

        if (engineRPM >= transmission.powerData.maxRPM)
            engineRPM -= 100;

        engineRPM = Mathf.SmoothDamp(engineRPM, transmission.powerData.idleRPM + (Mathf.Abs(transmission.DrivetrainRPM) * 3.6f * gearRatio), ref velocity, engineResponseTime);

    }

    IEnumerator StartEngine()
    {
        if (starterSource.enabled != true)
            starterSource.enabled = true;

        SoundManager.Instance.PlayInGameSound("VehicleFX_StartVehicle", starterSource, false, out float clipLength);

        yield return new WaitForSeconds(clipLength * .5F);

        float velocity = 0;

        float revOffTimer = 0;

        while (revOffTimer < .5F)
        {
            engineRPM = Mathf.SmoothDamp(engineRPM, 1750, ref velocity, engineResponseTime);

            revOffTimer += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        SoundManager.Instance.PlayInGameSound("VehicleFX_StartVehicle_Tail", starterSource, false);

        IsRunning = true;

        starterSource.enabled = false;

        StopAllCoroutines();
    }
}