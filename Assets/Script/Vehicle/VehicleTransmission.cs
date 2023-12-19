using System.Collections;
using UnityEngine;

//TO-DO : WILL KEEP TRANSMISSION AUTO FOR NOW.
public class VehicleTransmission : MonoBehaviour
{
    public VehicleTransmissionAndPowerData powerData;

    private int currentGear;

    private VehicleInput input;

    private Vehicle vehicle;

    private VehicleEngine engine;

    private bool isChangingGear;

    private float avgWheelSlippage;

    const float MAX_SLIPPAGE = 0.6F;

    private AudioSource source;

    public bool IsChangingGear { get { return isChangingGear; } }
    public float DrivetrainRPM { get; private set; }
    public int CurrentGear { get { return currentGear; } }

    public bool IsInReverse { get; private set; }

    [SerializeField] private float timeToChangeGears;

    private float gearChangeTimer;

    private void Awake()
    {
        input = GetComponent<VehicleInput>();
        vehicle = GetComponent<Vehicle>();
        engine = GetComponent<VehicleEngine>();
        InitSound();
    }

    private void InitSound()
    {
        if (GameManager.instance.IsInGarage)
            return;

        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;

        if (GetComponent<PlayerVehicleInput>().enabled)
            source.minDistance = 5;
        else
            source.minDistance = 2;
    }

    private void LateUpdate()
    {
        CalculateDrivetrainRPM();
        CalculateAvgWheelSlip();
        DistributePowerAmongWheels();
        ProcessGearChanges();
    }

    private void CalculateDrivetrainRPM()
    {
        float sumRPM = 0;

        int numOfWheels = 0;

        foreach (Wheel w in vehicle.AllWheels)
        {
            sumRPM += w.RPM;

            numOfWheels++;
        }

        DrivetrainRPM = numOfWheels != 0 ? sumRPM / numOfWheels : 0;

    }

    protected void CalculateAvgWheelSlip()
    {
        foreach (Wheel wheel in vehicle.AllWheels)
        {
            avgWheelSlippage += (wheel.WheelSlip.sideways + wheel.WheelSlip.forward) / 2;
        }

        avgWheelSlippage /= vehicle.AllWheels.Count;

        avgWheelSlippage = Mathf.Abs(avgWheelSlippage);
    }

    private void DistributePowerAmongWheels()
    {
        int poweredAxisCount = vehicle.PoweredAxis.Count;
        float engineForce;
        float BrakeForce;

        //ACCELERATION
        for (int i = 0; i < poweredAxisCount; i++)
        {
            for (int j = 0; j < vehicle.PoweredAxis[i].wheels.Length; j++)
            {
                engineForce = Mathf.Abs(engine.EnginePower);

                vehicle.PoweredAxis[i].wheels[j].SetBrakeTorque(0);

                vehicle.PoweredAxis[i].wheels[j].SetMotorTorque(input.IsInReverse ? -engineForce : engineForce);
            }
        }

        //BRAKING
        for (int i = 0; i < vehicle.axes.Length; i++)
        {
            for (int j = 0; j < vehicle.axes[i].wheels.Length; j++)
            {
                BrakeForce = input.Brake * powerData.brakeForce;

                if (!vehicle.ABSActive)
                    vehicle.axes[i].wheels[j].SetBrakeTorque(BrakeForce);

                if (vehicle.axes[i].wheels[j].Position == WheelPosition.BACK)
                {
                    if (input.Handbrake > 0)
                        vehicle.axes[i].wheels[j].SetBrakeTorque(input.Handbrake * 100);
                }
            }
        }
    }

    protected void ProcessGearChanges()
    {
        IsInReverse = input.IsInReverse;

        if (!vehicle.IsGrounded() || DrivetrainRPM < 0 || input.IsInReverse)
            return;

        int m_GearCount = powerData.gearRatios.Length - 1;


        if (engine.RPM >= powerData.maxRPM && currentGear < m_GearCount && vehicle.SpeedKMH >= powerData.speedAtGear[currentGear])
        {
            //Don't try and change up a gear while burning out.
            if (avgWheelSlippage >= MAX_SLIPPAGE)
                return;

            StartCoroutine(ChangeUP());
        }

        if (engine.RPM <= powerData.minRPM && currentGear > 0)
        {
            StartCoroutine(ChangeDOWN());
        }
    }

    IEnumerator ChangeUP()
    {
        isChangingGear = true;

        while (gearChangeTimer < powerData.timeToChangeGears)
        {
            gearChangeTimer += Time.deltaTime;

            if (gearChangeTimer >= powerData.timeToChangeGears)
            {
                currentGear++;

                SoundManager.instance.PlaySound("VehicleFX_Shifting", source);

                gearChangeTimer = 0;

                isChangingGear = false;

                break;
            }

            yield return new WaitForEndOfFrame();
        }

        StopAllCoroutines();

    }

    IEnumerator ChangeDOWN()
    {
        isChangingGear = true;

        while (gearChangeTimer < powerData.timeToChangeGears)
        {
            gearChangeTimer += Time.deltaTime;

            if (gearChangeTimer >= powerData.timeToChangeGears)
            {
                SoundManager.instance.PlaySound("VehicleFX_Shifting", source);

                currentGear--;

                gearChangeTimer = 0;

                isChangingGear = false;

                break;
            }

            yield return new WaitForEndOfFrame();
        }

        StopAllCoroutines();

    }
}