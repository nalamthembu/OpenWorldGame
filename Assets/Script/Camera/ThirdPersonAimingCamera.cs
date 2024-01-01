using UnityEngine;

[System.Serializable]
public class ThirdPersonAimingCamera : ThirdPersonCamera
{
    new public static ThirdPersonAimingCamera Instance;

    public override void Initialise(Transform selfTransform, float initialFOV)
    {
        if (Instance == null)
        {
            Instance = this;
        }

        base.Initialise(selfTransform, initialFOV);

        m_CheckForInactivity = false;
    }
}