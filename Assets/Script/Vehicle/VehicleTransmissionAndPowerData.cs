using UnityEngine;

[CreateAssetMenu(fileName = "PowerData", menuName = "Game/Vehicle/Gear Ratios")]
public class VehicleTransmissionAndPowerData : ScriptableObject
{
    public AnimationCurve torqueCurve;
    public float reverseGearRatio;
    public float[] gearRatios;
    public float[] speedAtGear;
    public float differentialRatio;
    public float timeToChangeGears = 0.25F;
    public float brakeForce = 15F;

    public float idleRPM = 950; //Idling RPM
    public float maxRPM; //before upshifting gear.
    public float minRPM; //before downshifting gear.
}