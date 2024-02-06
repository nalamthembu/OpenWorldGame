using UnityEngine;

public class Projectile : Entity
{
    [SerializeField] float thrustForce = 10;

    [Tooltip("How long is the projectile allowed to be in the scene?")]
    [SerializeField] float maxLifeTime = 10;

    float m_TimeSinceInstantiation;

    Rigidbody m_Rigidbody;

    float m_Damage;

    protected override void Awake()
    {
        base.Awake();
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    public void InitialiseProjectile(Entity Owner, float damage)
    {
        this.Owner = Owner;

        if (m_Rigidbody)
        {
            m_Rigidbody.AddForce(transform.forward * thrustForce, ForceMode.Impulse);
        }
        else
        {
            Debug.LogError("This projectile does not have a rigidbody attached! : " + gameObject.name);
        }

        m_Damage = damage;
    }

    protected override void Update()
    {
        m_TimeSinceInstantiation += Time.deltaTime;

        if (m_TimeSinceInstantiation >= maxLifeTime)
        {
            RemoveEntityFromScene();
        }
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        //Show the collision point.
        Debug.DrawLine(collision.contacts[0].point, collision.contacts[0].point + -collision.contacts[0].normal, Color.red);

        if (collision.collider.TryGetComponent<Entity>(out var entity))
        {
            if (entity.b_CanBeDamaged && entity.TryGetComponent<HealthComponent>(out var healthComponent))
            {
                healthComponent.TakeDamage(m_Damage, Owner, this);

                //TODO : Could be a problem since TakeDamage(..) needs an entity to determine what caused damage.

                RemoveEntityFromScene();
            }
        }
    }
}