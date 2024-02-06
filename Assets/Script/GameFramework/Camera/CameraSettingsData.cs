using UnityEngine;

/// <summary>
/// This scriptable object stores the camera settings for each view that can be switched between.
/// </summary>
[CreateAssetMenu(fileName = "Camera Settings Data" ,menuName = "Game/Camera/Camera Settings Data")]
public class CameraSettingsData : ScriptableObject
{
    public CameraSettings[] cameraSettings;

    public CameraSettings ProneSettings;

    public CameraSettings CrouchSettings;
}