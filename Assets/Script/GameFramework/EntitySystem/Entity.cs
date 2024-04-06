using UnityEngine;
using System;
using MyBox;
using Object = UnityEngine.Object;
using System.Collections;

/// <summary>
/// This base class describes every player interactable object
/// in the game and contains various properties about that object.
/// For example of the entity is on fire or is damaged or has exploded.
/// For example a vehicles, characters, destructable objects.
/// </summary>
public class Entity : MonoBehaviour
{
    [Tooltip("This is the minimum Y position before the game considerers this entity out of of the world.")]
    [SerializeField][Range(-10000, 0)] float m_MinY = -1000.0F;
    [HideInInspector] public bool b_IsOnFire; //Toggling this spawns fire on the object and decreases health if that is allowed.
    [HideInInspector] public bool b_IsDamaged; //Could be used for showing visible damage on an object (example : a damaged television screen)
    [HideInInspector] public bool b_IsExploding; //In the process of exploding.
    [HideInInspector] public bool b_HasExploded; //Has finished exploding.
    [HideInInspector] public Entity Owner { get; protected set; } //This would be used for vehicles, guns and/or pickups.
    [HideInInspector] public bool b_IsInvolvedInMission; //If this is true, it will not be deleted under any circumstance during a mission.
    [HideInInspector] public bool b_DespawnsAfterDeath = true; //True by default.
    [HideInInspector] public float f_DespawnTime = 60; //seconds
    [SerializeField] public bool b_CanBeDamaged;

    public static event Action<Entity> OnOutOfWorld;
    public static event Action<Entity> OnAddToWorld;
                                                                     
    protected void OnEntityDeath() => StartCoroutine(DespawnAfterDelay(f_DespawnTime));

    protected virtual IEnumerator DespawnAfterDelay(float delay)
    {
        Debug.Log($"Scheduled to despawn [{GetType()}]:[{gameObject.name}] in {delay} second(s).");

        while (delay > 0)
        {
            delay -= Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        RemoveEntityFromScene();

        Debug.Log($"Despawning [{GetType()}]:[{gameObject.name}], now.");
    }

    protected virtual void Awake() { }

    protected virtual void Start()
    {
        if (EntityManager.Instance != null)
        {
            OnAddToWorld?.Invoke(this);
        }
    }

    public virtual void SetOwner(Entity owner) => Owner = owner;

    protected virtual void Update()
    {
        //If we're below the min Y,
        if (gameObject.transform.position.y <= m_MinY)
            RemoveEntityFromScene();
    }

    protected virtual void OnBecameInvisible()
    {
        if (!ThirdPersonCamera.Instance || b_IsInvolvedInMission)
            return;

        float distanceFromCamera = Vector3.Distance(transform.position, ThirdPersonCamera.Instance.transform.position);

        //if this entity is really far from the camera
        if (distanceFromCamera >= ThirdPersonCamera.Instance.FarZ * 0.85F)
        {
            RemoveEntityFromScene();
        }
    }

    public void RemoveEntityFromScene()
    {
        //Let every class that's listening know.
        OnOutOfWorld?.Invoke(this);

        //Disable this game object.
        gameObject.SetActive(false);
    }

    public virtual void OnShot(Projectile projectile, Entity OwnerOfProjectile)
    {
        //Process the effect in child class.
    }

    public virtual void Teleport(Vector3 position) => transform.position = position;

    protected virtual void OnCollisionEnter(Collision collision)
    {
        //Was this entity hit by a bullet?
        if (collision.collider.TryGetComponent<Bullet>(out var bullet))
        {
            OnShot(bullet, bullet.Owner);

            return;
        }
    }
}

//Manager for variable levels of damage
[System.Serializable]
public class DamageDetail
{
    [SerializeField] GameObject m_DamageLevelPrefab;
    [SerializeField] public float m_HealthAtThisDamageLevel;

    private Transform transform;

    public void Initialise(Transform transform) => this.transform = transform;

    public void SpawnDamagePrefab() => Object.Instantiate(m_DamageLevelPrefab, transform.position, transform.rotation, transform);
}