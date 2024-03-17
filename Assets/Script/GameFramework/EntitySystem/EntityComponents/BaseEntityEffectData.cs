using UnityEngine;

public class BaseEntityEffectData : ScriptableObject
{
    public string EntityName;
    public bool CanReactToBullets;
}


[System.Serializable]
public struct EntityCollisionFXData
{
    public string collisionSoundID;

    public ParticleSystem collisionFXParticle;
}


[System.Serializable]
public struct EntityShotFXData
{
    public string shotCollisionSoundID;

    public ParticleSystem hitFXParticle;
}