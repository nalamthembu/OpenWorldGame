using UnityEngine;

public class Projectile : Entity
{
    [SerializeField] float thrustForce = 10;

    [Tooltip("How long is the projectile allowed to be in the scene?")]
    [SerializeField] float maxLifeTime = 10;

    [SerializeField] AudioClip[] m_GenericHitSound;

    float m_TimeSinceInstantiation;

    protected Rigidbody m_Rigidbody;

    protected float m_Damage, m_Range = -1;

    Vector3 m_StartPoint;

    public float GetDamage() => m_Damage;

    protected override void Awake()
    {
        base.Awake();
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    protected override void Start()
    {
        base.Start();
    }

    public void InitialiseProjectile(Entity Owner, float damage, float range, Vector3 forceDir, Vector3 upwardForce = default)
    {
        this.Owner = Owner;

        m_Damage = damage;

        m_Range = range;

        m_StartPoint = transform.position;

        //Launch
        m_Rigidbody.AddForce(forceDir * thrustForce);
        m_Rigidbody.AddForce(upwardForce * thrustForce);
    }

    protected override void Update()
    {
        m_TimeSinceInstantiation += Time.deltaTime;

        if (m_TimeSinceInstantiation >= maxLifeTime)
        {
            RemoveEntityFromScene();
        }

        if (ThirdPersonCamera.Instance != null && Vector3.Distance(transform.position, m_StartPoint) >= ThirdPersonCamera.Instance.FarZ)
            RemoveEntityFromScene();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawCube(transform.position, Vector3.one * 0.1F);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        //if we whizz by the camera
        if (this is Bullet)
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayInGameSound("BulletFX_WhizBy",transform.position, true, 5);

            Debug.DrawRay(transform.position, transform.forward * 0.5f, Color.yellow, 1);
        }
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        ContactPoint firstContact = collision.GetContact(0);

        GameObject colliderGO = collision.gameObject;

        if (colliderGO.TryGetComponent<CharacterHitReactorComponent>(out _))
            return;

        if (colliderGO.TryGetComponent<Entity>(out var entity))
        {
            if (entity.TryGetComponent<HealthComponent>(out var healthComp))
                healthComp.TakeDamage(m_Damage, Owner.gameObject, gameObject);
        }
        else
        {
            //We hit anything else
            if (this is Bullet)
            {
                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlayInGameSound("BulletFX_Ricochet", firstContact.point, true, 2);
            }
        }

        RemoveEntityFromScene();
    }
}