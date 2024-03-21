using UnityEngine;
/// <summary>
/// These are camera settings that can be cycled through by the player.
/// </summary>
[System.Serializable]
public class CameraSettings
{
    public string name;

    [Header("Normal")]

    public float distanceFromTarget = 3;

    [Min(0)] public float fieldOfView;

    public Vector2 xyOffset;

    [Header("Aiming")]

    public float distanceFromTargetAiming = 3;

    public Vector2 xyAimOffset;

    [Min(0)] public float fieldOfViewAiming;
}