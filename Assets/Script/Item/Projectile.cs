using UnityEngine;

[
    RequireComponent
    (
        typeof(BoxCollider)
    )
]
public class Projectile : MonoBehaviour
{
    [SerializeField] protected float hitForce = 5;

    protected float damageOnImpact;

    protected Vector3 initialPosition;


    private void OnCollisionEnter(Collision collision)
    {
        Debug.DrawRay(collision.contacts[0].point, -collision.contacts[0].normal, Color.red, 10);

        if (this is Bullet bullet)
        {
            bullet.RichochetCount++;

            //Damage Logic
            if (collision.collider.TryGetComponent<Character>(out var character))
            {
                character.TakeDamage(damageOnImpact, DamageCause.Bullet);
            }

            if (collision.collider.TryGetComponent<Vehicle>(out var vehicle))
            {
                vehicle.securitySystem.NotifySecuritySystemOfDisturbance();

                SoundManager.Instance.PlayInGameSound("BulletFX_Hit_Vehicle", collision.contacts[0].point, true, 30.0F);
            }

            if (bullet.RichochetCount >= bullet.MaxRicochetCount)
            {
                //Return to Object pool
                ObjectPoolManager.Instance.ReturnGameObject(gameObject);
            }
        }
    }
}