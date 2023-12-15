using UnityEngine;

public class Gun : Weapon
{
    [SerializeField] protected Transform m_LeftHandIKTransform;
    public Transform LeftHandIK { get { return m_LeftHandIKTransform; } }

    private void Update()
    {
        if (m_IsPickedUp)
        {
            if (m_IsEquipped)
            {
                //if owner is not aiming.
                if (!m_Owner.IsAiming)
                {
                    transform.SetLocalPositionAndRotation(
                        WeaponData.holsteredVector.position,
                        Quaternion.Euler(WeaponData.holsteredVector.rotation)
                        );
                }

                //If owner is aiming.
                if (m_Owner.IsAiming)
                {

                }
            }
        }
    }

    //Collision
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 2)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayInGameSound
                    (
                        WeaponData.objectSoundData.CollisionSoundNames,
                        transform.position,
                        true
                    );

            }
        }
    }
}