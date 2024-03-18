using UnityEngine;

public enum BodyPart
{
    Limb,
    Head,
    Body,
    Foot
};

public class CharacterHitReactorComponent : MonoBehaviour, IEntityComponent
{
    [SerializeField] HealthComponent m_HealthComponent;
    [SerializeField] BodyPart m_BodyPart;
    private BaseCharacter m_Character;

    private void Start() => m_Character = m_HealthComponent.Character;
    public BodyPart BodyPart => m_BodyPart;

    protected virtual void OnCollisionEnter(Collision collision)
    {
        ContactPoint firstContact = collision.contacts[0];

        Collider otherCollider = firstContact.otherCollider;

        if (firstContact.impulse.sqrMagnitude >= 3F * 3F)
        {
            string audio_id;

            //if this was hit by some entity
            if (otherCollider.TryGetComponent<Entity>(out var component))
            {
                switch (component)
                {
                    //if the component is a projectile (bullets, arrows, grenades, etc.)
                    case Projectile:

                        //Determine what was hit 
                        audio_id = m_BodyPart == BodyPart.Head ? "BulletFX_Headshot" : "BulletFX_Flesh";

                        //Take damage
                        Projectile projectile = component as Projectile;
                        m_Character.OnShot(projectile, projectile.Owner);
                        m_HealthComponent.TakeDamage(
                            m_BodyPart == BodyPart.Head ? 100 : projectile.GetDamage(),
                            projectile.Owner.gameObject,
                            projectile.gameObject);

                        PlaySound(audio_id, 5);

                        break;

                    //If hit by a car
                    case Vehicle:
                        //Take damage
                        Vehicle vehicle = component as Vehicle;

                        m_HealthComponent.TakeDamage(firstContact.impulse.magnitude,
                                                     vehicle.Owner ? vehicle.Owner.gameObject : null, //do a quick check to see if the owner is null.
                                                     vehicle.gameObject);
                        break;
                }

                SpawnBloodFX();
            }
            else
            {
                //otherwise it was a general collision
                audio_id = m_BodyPart switch
                {
                    BodyPart.Body => "CharFX_BodyHit",
                    BodyPart.Limb => "CharFX_LimbHit",
                    BodyPart.Head => "CharFX_BodyHit",
                    BodyPart.Foot => "CharFX_FootHit",
                    _ => "CharFX_BodyHit"
                };

                //take damage
                m_HealthComponent.TakeDamage(
                    firstContact.impulse.magnitude,
                    collision.gameObject,
                    collision.gameObject);

                PlaySound(audio_id, 3);
            }
        }
    }

    protected virtual void SpawnBloodFX()
    {
        if (ObjectPoolManager.Instance != null && ObjectPoolManager.Instance.TryGetPool("HitFX_Blood", out var hitFX))
        {
            if (hitFX.TryGetGameObject(out var go))
            {
                go.transform.SetPositionAndRotation(transform.position, transform.rotation);

                ObjectPoolManager.Instance.ReturnGameObject(go, 5.0F);
            }
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
