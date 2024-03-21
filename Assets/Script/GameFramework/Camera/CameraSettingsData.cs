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

    private void OnValidate()
    {
        //Validate Field Of Views
        foreach (var camSet in cameraSettings)
        {
            if (camSet.fieldOfView <= 0)
            {
                Debug.LogWarning($"Field of view on camera setting : {camSet.name} is set to 0. Defaulting to 50");
                camSet.fieldOfView = 50;
            }
        }

        if (ProneSettings.fieldOfView <= 0)
        {
            Debug.LogWarning($"Field of view on camera setting : {ProneSettings.name} is set to 0. Defaulting to 50");
            ProneSettings.fieldOfView = 50;
        }

        if (CrouchSettings.fieldOfView <= 0)
        {
            Debug.LogWarning($"Field of view on camera setting : {CrouchSettings.name} is set to 0. Defaulting to 50");
            CrouchSettings.fieldOfView = 50;
        }
    }
}