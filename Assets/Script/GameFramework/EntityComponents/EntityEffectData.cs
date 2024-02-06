using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "EntityEffectData", menuName = "Game/Entity/Entity Effect Data")]
public class EntityEffectData : ScriptableObject
{
    public string entityName;

    public EntityCollisionFXData CollisionFX;

    public bool CanReactToBullets;
    
    public EntityShotFXData ShotFX;
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