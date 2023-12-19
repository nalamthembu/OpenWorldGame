using UnityEngine;

public class VehicleEngine : MonoBehaviour
{
    private VehicleTransmission transmission;

    private VehicleInput input;

    private float totalPower;

    private float engineRPM;


    [SerializeField][Range(0.001F, 2.0F)] float engineResponseTime;

    public float EnginePower { get { return totalPower; } }

    public float Throttle { get { return input.Throttle; } }

    public float MaxRPM { get { return transmission.powerData.maxRPM; } }

    public float RPM { get { return engineRPM; } }

    public float IdleRPM { get { return transmission.powerData.idleRPM; } }

    private void Awake()
    {
        transmission = GetComponent<VehicleTransmission>();
        input = GetComponent<VehicleInput>();
    }

    private void FixedUpdate()
    {
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
}