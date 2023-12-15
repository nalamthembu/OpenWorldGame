﻿using UnityEngine;

//This script keeps track of inputs for the vehicle, it will be used by other scripts under the vehicle object.
public class VehicleInput : MonoBehaviour
{
    [Range(0, 1)] float throttle;
    [Range(0, 1)] float brake;
    [Range(0, 1)] float handbrake;
    [Range(-1, 1)] float steering;

    float bVel = 0;
    float tVel = 0;

    #region ACCESSORS
    public float Throttle { get { return throttle;  } set { throttle = value; } }
    public float Brake { get { return brake; } set { brake = value; } }
    public float Handbrake { get { return handbrake; } set { handbrake = value; } }
    public float Steering { get { return steering; } set { steering = value; } }
    #endregion

    private AIDriver aiDriver;

    public bool inReverse = false;

    private void Awake() => aiDriver = GetComponent<AIDriver>();

    private void Update() => FloorValues();

    private void FloorValues()
    {
        //FLOAT-POINT NUMBERS ARE FUCKING THIS UP.
        if (aiDriver)
            throttle = Mathf.Floor(throttle - aiDriver.targetThrottle);

        brake = Mathf.Floor(brake);
        Handbrake = Mathf.Floor(Handbrake);
    }
}