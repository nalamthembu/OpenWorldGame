using UnityEngine;
using System.Collections.Generic;

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

public class Vehicle : MonoBehaviour
{
    //TO-DO : ABS ON WHEELS.

    public Axis[] axes;

    public Seat[] seats;

    private Rigidbody rigidBody;

    private VehicleInput input;

    private VehicleEngine engine;

    private VehicleTransmission transmission;

    readonly private List<Axis> poweredAxis = new();
    readonly private List<Wheel> allWheels = new();

    public List<Axis> PoweredAxis { get { return poweredAxis; } }
    public List<Wheel> AllWheels { get { return allWheels; } }
    public bool ABSActive { get; private set; }

    public float SpeedKMH { get { return rigidBody.velocity.magnitude * 3.5F;  } }

    public VehicleEngine Engine { get { return engine; } }
    public VehicleTransmission Transmission { get { return transmission; } }

    private void Awake()
    {
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
            }
        }
    }

    private void FixedUpdate()
    {
        ControlSteering();
        ControlABS();
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
}

[System.Serializable]
public struct Axis
{
    public Wheel[] wheels;
    public bool isPowered;
    public bool isSteering;
    public float steerRadius;
}

[System.Serializable]
public struct Seat
{
    public Transform seatTransform;

    public SeatPosition seatPosition;
}

public enum SeatPosition
{
    LF, //Left Front
    LB, //Left Back
    RF, //Right Front
    RB //Right Back
}