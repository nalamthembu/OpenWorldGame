using UnityEngine;

[CreateAssetMenu(fileName = "EntityEffectData", menuName = "Game/Entity/Entity Effect Data Object")]
public class EntityEffectDataObject : BaseEntityEffectData
{
    public EntityCollisionFXData CollisionFX;
    public EntityShotFXData ShotFX;
}
