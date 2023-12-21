using UnityEngine;

[RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
public class Weapon : MonoBehaviour, IWeapon
{
    protected SphereCollider m_Collider;

    protected Rigidbody m_Rigidbody;

    protected bool m_IsPickedUp;

    protected bool m_IsEquipped;

    protected Character m_Owner;

    [SerializeField] protected WeaponData m_WeaponData;

    public bool IsPickedUp { get { return m_IsPickedUp; } }

    public WeaponData WeaponData { get { return m_WeaponData; } }

    protected virtual void Awake() => InitialiseWeapon();
   
    private void OnValidate()
    {
        if (m_Collider is null)
            m_Collider = GetComponent<SphereCollider>();
        else if (m_Collider != null && !m_Collider.isTrigger)
            m_Collider.isTrigger = true;

        if (m_Rigidbody is null)
            m_Rigidbody = GetComponent<Rigidbody>();
    }

    public void SetEquipStatus(bool value)
    {
        m_IsEquipped = value;
        gameObject.SetActive(true);
    }

    private void InitialiseWeapon()
    {
        m_Collider = GetComponent<SphereCollider>();

        m_Collider.isTrigger = true;

        m_Rigidbody = GetComponent<Rigidbody>();
    }

    public virtual void Fire() => Debug.LogError("This is not implemented");
    
    //Collision
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 2)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayInGameSound
                    (
                        WeaponData.objectSoundData.CollisionSoundID,
                        transform.position,
                        true
                    );

            }
        }
    }

    //Pickup logic.
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            if (player.TryGetComponent<WeaponInventory>(out var inventory))
            {
                //If adding the weapon was successful.
                if (inventory.AddWeapon(this))
                {
                    m_Collider.enabled = false;

                    m_Rigidbody.isKinematic = true;

                    m_IsPickedUp = true;

                    //Set owner.
                    m_Owner = player;
                }
            }
        }
    }
}