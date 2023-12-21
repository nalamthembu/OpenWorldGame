using UnityEngine;
using System.Collections;

public class VehicleEngine : MonoBehaviour
{
    private VehicleTransmission transmission;

    private VehicleInput input;

    private float totalPower;

    private float engineRPM;

    private AudioSource starterSource;

    [SerializeField] private bool isStartingEngine;

    [SerializeField] private bool isRunning;

    [SerializeField][Range(0.001F, 2.0F)] float engineResponseTime;

    public float EnginePower { get { return totalPower; } }

    public float Throttle { get { return input.Throttle; } }

    public float MaxRPM { get { return transmission.powerData.maxRPM; } }

    public float RPM { get { return engineRPM; } }

    public float IdleRPM { get { return transmission.powerData.idleRPM; } }

    public bool IsRunning { get { return isRunning; } }

    public bool IsStarting { get { return isStartingEngine; } }

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
        if (isStartingEngine)
        {
            StartCoroutine(StartEngine());
            isStartingEngine = false;
        }
    }

    private void FixedUpdate()
    {
        if (isRunning)
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

        SoundManager.Instance.PlayInGameSound("VehicleFX_StartVehicle", starterSource, false, out float startClipLength);

        yield return new WaitForSeconds(startClipLength * .5F);

        if (Random.Range(0, 100) >= 10)
        {
            float velocity = 0;

            float revOffTimer = 0;

            float startUpRPM = Random.Range(1750, 2000);

            while (revOffTimer < .5F)
            {
                engineRPM = Mathf.SmoothDamp(engineRPM, startUpRPM, ref velocity, engineResponseTime);

                revOffTimer += Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }

            isRunning = true;
        }

        SoundManager.Instance.PlayInGameSound("VehicleFX_StartVehicle_Tail", starterSource, false, out float tailClipLength);

        yield return new WaitForSeconds(tailClipLength);

        starterSource.enabled = false;

        StopAllCoroutines();
    }
}