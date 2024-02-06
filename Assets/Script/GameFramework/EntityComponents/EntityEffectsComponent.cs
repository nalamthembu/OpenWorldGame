using UnityEngine;

//TODO : Explosion FX

public class EntityEffectsComponent : MonoBehaviour
{
    [SerializeField] EntityEffectData m_EntityFX;

    private Entity thisEntity;

    private void Awake() => thisEntity = GetComponent<Entity>();

    private void OnEnable()
    {
        Entity.OnCollision += OnEntityCollision;
        Entity.OnShot += OnEntityShot;
    }

    private void OnDisable()
    {
        Entity.OnCollision -= OnEntityCollision;
        Entity.OnShot -= OnEntityShot;
    }

    private void OnEntityShot(Entity inComingEntity, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (!m_EntityFX)
            return;

        //if this entity can react to bullets.
        if (inComingEntity == thisEntity && m_EntityFX.CanReactToBullets)
        {
            if (SoundManager.Instance)
            {
                SoundManager.Instance.PlayInGameSound(m_EntityFX.ShotFX.shotCollisionSoundID, hitPoint, true, 1);

                if (m_EntityFX.ShotFX.hitFXParticle)
                {
                    GameObject collisionParticle =
                    Instantiate(m_EntityFX.ShotFX.hitFXParticle,
                    hitPoint, Quaternion.LookRotation(-hitNormal)).gameObject;

                    Destroy(collisionParticle, 5.0F);
                }
            }
        }
    }

    private void OnEntityCollision(Entity inComingEntity, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (!m_EntityFX)
            return;

        if (inComingEntity == thisEntity)
        {
            if (SoundManager.Instance)
            {
                SoundManager.Instance.PlayInGameSound(m_EntityFX.CollisionFX.collisionSoundID, hitPoint, true, 1);

                if (m_EntityFX.CollisionFX.collisionFXParticle)
                {
                    GameObject collisionParticle =
                        Instantiate(m_EntityFX.CollisionFX.collisionFXParticle,
                        hitPoint, Quaternion.LookRotation(-hitNormal)).gameObject;

                    Destroy(collisionParticle, 5.0F);
                }
            }
        }
    }
}