using UnityEngine;

public class Projectile : Entity
{
    [SerializeField] float thrustForce = 10;

    [Tooltip("How long is the projectile allowed to be in the scene?")]
    [SerializeField] float maxLifeTime = 10;

    float m_TimeSinceInstantiation;

    Rigidbody m_Rigidbody;

    float m_Damage, m_Range = -1;

    Vector3 m_StartPoint;

    protected override void Awake()
    {
        base.Awake();
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    protected override void Start()
    {
        base.Start();
    }

    public void InitialiseProjectile(Entity Owner, float damage, float range) //Height is used to create an arch (useful for simulating bullet drop or throwables)
    {
        this.Owner = Owner;

        m_Damage = damage;

        m_Range = range;

        m_StartPoint = transform.position;

        //Launch
        m_Rigidbody.AddForce(transform.forward * thrustForce);
    }

    protected override void Update()
    {
        m_TimeSinceInstantiation += Time.deltaTime;

        if (m_TimeSinceInstantiation >= maxLifeTime)
        {
            RemoveEntityFromScene();
        }

        if (m_Range > -1 && Vector3.Distance(transform.position, m_StartPoint) >= m_Range)
            RemoveEntityFromScene();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawCube(transform.position, Vector3.one * 0.1F);
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        Transform rootTransform = collision.collider.transform.root;

        print(rootTransform.name);

        if (rootTransform.TryGetComponent<Entity>(out var entity))
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