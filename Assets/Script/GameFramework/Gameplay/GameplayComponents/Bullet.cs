using UnityEngine;

public class Bullet : Projectile
{
    [SerializeField] protected float m_HitForce = 1000.0F;

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        //Add force to rigidbody that got hit.
        if (collision.collider.TryGetComponent<Rigidbody>(out var affectedRigidbody))
            affectedRigidbody.AddForce(-collision.contacts[0].normal * m_HitForce, ForceMode.Impulse);
        
        RemoveEntityFromScene();
    }
}