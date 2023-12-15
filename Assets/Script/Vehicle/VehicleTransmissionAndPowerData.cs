using UnityEngine;

[CreateAssetMenu(fileName = "PowerData", menuName = "Game/Vehicle/Gear Ratios")]
public class VehicleTransmissionAndPowerData : ScriptableObject
{
    public AnimationCurve torqueCurve;
    public float[] gearRatios;
    public float[] speedAtGear;
    public float differentialRatio;

    public float maxRPM; //before upshifting gear.
    public float minRPM; //before downshifting gear.
}