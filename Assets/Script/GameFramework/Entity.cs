using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(EntityEffectsComponent))]
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
    [HideInInspector] public bool b_CanBeDamaged;
    [HideInInspector] public Entity Owner = null; //This would be used for vehicles, guns and/or pickups.

    //Only shows when b_CanBeDamaged = true
    [HideInInspector] public GameObject m_DamagedObject;
  
    public static event Action<Entity> OnOutOfWorld;

    public static event Action<Entity> OnAddToWorld;

    //<this entity, hitPoint, hitNormal>
    public static event Action<Entity, Vector3, Vector3> OnShot; //Fires when a bullet collides with this entity.

    //<this entity, hitPoint, hitNormal>
    public static event Action<Entity, Vector3, Vector3> OnCollision; //This is mainly for entities with singular colliders,
                                                    //this might not work on a complex object like a character.
    
    public bool HasHealthComponent { get; private set; }

    protected virtual void Awake() 
    {
        OnAddToWorld?.Invoke(this);
    }

    protected virtual void Update()
    {
        //If we're below the min Y,
        if (gameObject.transform.position.y <= m_MinY)
            RemoveEntityFromScene();
    }

    protected virtual void OnBecameInvisible()
    {
        //TODO : Make this exclude mission entities or other objects that will have to be really far.

        if (!ThirdPersonCamera.Instance)
            return;

        float distanceFromCamera = Vector3.Distance(transform.position, ThirdPersonCamera.Instance.transform.position);

        //if this entity is really far from the camera
        if (distanceFromCamera >= ThirdPersonCamera.Instance.FarZ / 0.85F)
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

    public virtual void Teleport(Vector3 position) => transform.position = position;

    protected virtual void OnCollisionEnter(Collision collision)
    {
        //Was this entity hit by a bullet?
        if (collision.collider.TryGetComponent<Bullet>(out var bullet))
        {
            OnShot?.Invoke(this, collision.contacts[0].point, collision.contacts[0].normal);
            return;
        }
        else
        {
            //All other collision, including other projectiles, like a grenade.
            OnCollision?.Invoke(this, collision.contacts[0].point, collision.contacts[0].normal);
        }
    }
    
}

#if UNITY_EDITOR
[CustomEditor(typeof(Entity))]
public class EntityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // for other non-HideInInspector fields

        Entity entity = (Entity)target;

        if (GUILayout.Button("Can Be Damaged : " + entity.b_CanBeDamaged))
            entity.b_CanBeDamaged = !entity.b_CanBeDamaged;

        //Show damage options if this entity can be damaged.
        if (entity.b_CanBeDamaged)
        {
            GUILayout.Label("----------Damage-----------");
            entity.m_DamagedObject = EditorGUILayout.ObjectField("Damaged Object", entity.m_DamagedObject, entity.m_DamagedObject.GetType()) as GameObject;
        }
    }
}
#endif