using UnityEngine;

[CreateAssetMenu(fileName = "EntityEffectDataCharacter", menuName = "Game/Entity/Entity Effect Data Character")]
public class EntityEffectDataCharacter : BaseEntityEffectData
{
    [Header("Collision FX")]
    public EntityCollisionFXData HeadCollisionFX;
    public EntityCollisionFXData BodyCollisionFX;
    public EntityCollisionFXData LimbCollisionFX;


    [Header("Gunshot FX")]
    public EntityShotFXData HeadShotFX;
    public EntityShotFXData BodyShotFX;
}