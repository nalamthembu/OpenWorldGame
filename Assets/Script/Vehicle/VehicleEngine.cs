using UnityEngine;

public class VehicleEngine : MonoBehaviour
{
    private VehicleTransmission transmission;

    private VehicleInput input;

    private float totalEnginePower;

    private float engineRPM;

    private const float IDLERPM = 1000;

    [SerializeField][Range(0.5F, 10F)] float rpmSmoothTime;

    public float EnginePower { get { return totalEnginePower; } }

    public float RPM { get { return engineRPM; } }

    private void Awake()
    {
        transmission = GetComponent<VehicleTransmission>();
        input = GetComponent<VehicleInput>();
    }

    private void FixedUpdate()
    {
        CalculateEnginePower();
    }

    private void CalculateEnginePower()
    {
        //REV-LIMITER
        if (engineRPM > transmission.powerData.maxRPM)
        {
            engineRPM = transmission.powerData.maxRPM - 1000;
        }

        float gearRatio = transmission.powerData.gearRatios[transmission.CurrentGear];

        totalEnginePower = transmission.powerData.torqueCurve.Evaluate(engineRPM) * (gearRatio);

        totalEnginePower *= (!transmission.IsChangingGear) ? input.Throttle : 0;

        float velocity = 0;

        engineRPM = Mathf.SmoothDamp(engineRPM, 1000 + (Mathf.Abs(transmission.DrivetrainRPM) * 3.6f * gearRatio), ref velocity, rpmSmoothTime);
    }
}