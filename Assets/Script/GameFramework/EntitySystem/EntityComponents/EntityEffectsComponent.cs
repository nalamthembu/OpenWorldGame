using UnityEditor;
using UnityEngine;

//TODO : Explosion FX

public class EntityEffectsComponent : MonoBehaviour
{
    [SerializeField] BaseEntityEffectData m_EntityFX;

    private Entity m_AttachedEntity;

    private void Awake() => m_AttachedEntity = GetComponent<Entity>();

    #region Event Subscription
    private void OnEnable()
    {
        //Subscribe to events
        if (m_AttachedEntity.TryGetComponent<HealthComponent>(out var health))
        {
            health.OnEndFall += OnEndOfFall;
        }
    }

    private void OnDisable()
    {
        //Unsubscribe to events
        if (m_AttachedEntity.TryGetComponent<HealthComponent>(out var health))
        {
            health.OnEndFall -= OnEndOfFall;
        }
    }
    #endregion



    private void OnEndOfFall(float initialHeight, Vector3 impactPoint)
    {
        if (initialHeight > 10) //That's a really high fall
        {
            SoundManager.Instance.PlayInGameSound("BodyFX_HighImpactThud", impactPoint, true, 5);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint firstContact = collision.contacts[0];

        if (firstContact.impulse.sqrMagnitude < 5 * 5)
            return;

        Collider otherCollider = firstContact.otherCollider;

        string audio_id = string.Empty;

        switch (m_AttachedEntity)
        {
            case Vehicle:

                //if the car was shot
                if (otherCollider.TryGetComponent<Bullet>(out var bullet))
                {
                    audio_id = "BulletFX_VehicleHit";
                    PlaySound(audio_id, 10);

                    //Sparks
                    if (VFXManager.Instance != null)
                        VFXManager.Instance.SpawnVisualEffect("VFX_Sparks", firstContact.point, Quaternion.LookRotation(firstContact.normal));

                    if (TryGetComponent<HealthComponent>(out var healthComponent))
                        healthComponent.TakeDamage(bullet.GetDamage(), bullet.Owner.gameObject, bullet.gameObject);
                }

                if (otherCollider.TryGetComponent<CharacterHitReactorComponent>(out _))
                {
                    //if we DID hit the body (I just changed it to negative, it's not supposed to make sense)
                    if (firstContact.thisCollider.gameObject.name != gameObject.name)
                        audio_id = "VehicleFX_BodyHit";
                    else
                        audio_id = "VehicleFX_PanelHit";

                    PlaySound(audio_id, 2);
                }

                return;
        }
    }

    protected virtual void PlaySound(string id, float minAudibleDistance = 0.25F)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayInGameSound(id, transform.position, true, minAudibleDistance);
        else
            Debug.LogError("There is no Sound Manager in the scene!");
    }
}