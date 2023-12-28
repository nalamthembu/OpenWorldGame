using UnityEngine;

[System.Serializable]
public class DebugCamera : BaseCamera
{
    public static DebugCamera Instance;

    public override void DoUpdatePosition()
    {
        throw new System.NotImplementedException();
    }

    public override void DoUpdateRotation()
    {
        throw new System.NotImplementedException();
    }

    public override void Initialise(Transform selfTransform, float initialFOV)
    {
        if (Instance == null)
        {
            Instance = this;
        }

        base.Initialise(selfTransform, initialFOV);
    }
}