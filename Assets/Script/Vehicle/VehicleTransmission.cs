using UnityEngine;

//TO-DO : WILL KEEP TRANSMISSION AUTO FOR NOW.
public class VehicleTransmission : MonoBehaviour
{
    public VehicleTransmissionAndPowerData powerData;

    private int currentGear;

    private VehicleInput input;

    private Vehicle vehicle;

    private VehicleEngine engine;

    public float DrivetrainRPM { get; private set; }
    public int CurrentGear { get { return currentGear; } }

    [SerializeField] private float timeToChangeGears;

    private float gearChangeTimer;
    public bool IsChangingGear { get; private set; }

    private void Awake()
    {
        input = GetComponent<VehicleInput>();
        vehicle = GetComponent<Vehicle>();
        engine = GetComponent<VehicleEngine>();
    }

    private void LateUpdate()
    {
        CalculateDrivetrainRPM();
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

    private void DistributePowerAmongWheels()
    { 
        int poweredAxisCount = vehicle.PoweredAxis.Count;
        float engineForce = 0;
        float BrakeForce;

        //ACCELERATION
        for (int i = 0; i < poweredAxisCount; i++)
        {
            for (int j = 0; j < vehicle.PoweredAxis[i].wheels.Length; j++)
            {
                engineForce = Mathf.Abs(engine.EnginePower);

                vehicle.PoweredAxis[i].wheels[j].SetMotorTorque(engineForce);
            }
        }

        //BRAKING
        for (int i = 0; i < vehicle.axes.Length; i++)
        {
            for (int j = 0; j < vehicle.axes[i].wheels.Length; j++)
            {
                BrakeForce = (input.Brake * 5000) - engineForce;

                BrakeForce = Mathf.Clamp(BrakeForce, 0, int.MaxValue);

                if (!vehicle.ABSActive)
                    vehicle.axes[i].wheels[j].SetBrakeTorque(BrakeForce);
            }

            //Handbrake is applied to wheels that don't get any power from the engine.
            if (!vehicle.axes[i].isPowered)
            {
                for (int j = 0; j < vehicle.axes[i].wheels.Length; j++)
                    vehicle.axes[i].wheels[j].SetBrakeTorque(input.Handbrake * 15); //STOP THE WHEELS
            }
        }
    }
    private void ProcessGearChanges()
    {
        if (!vehicle.IsGrounded())
            return;

        if (IsChangingGear)
        {
            gearChangeTimer += Time.deltaTime;

            if (gearChangeTimer >= timeToChangeGears)
            {
                gearChangeTimer = 0;

                IsChangingGear = false;
            }

            return;
        }

        int gearCount = powerData.gearRatios.Length - 1;

        if (engine.RPM > powerData.maxRPM && currentGear < gearCount)
        {
            IsChangingGear = true;

            currentGear++;
        }
        else if (engine.RPM < powerData.minRPM && currentGear > 0)
        {
            IsChangingGear = true;

            currentGear--;
        }
    }
        
}