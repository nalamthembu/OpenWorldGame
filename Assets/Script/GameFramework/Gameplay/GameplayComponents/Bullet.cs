using UnityEngine;

public class Bullet : Projectile
{
    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        //Play Hit sound
        //TODO : CHECK WHAT TYPE OF SURFACE THIS BULLET HIT.
        if (SoundManager.Instance)
        {
            SoundManager.Instance.PlayInGameSound("BulletFX_Hit_Concrete", collision.contacts[0].point, true);
            SoundManager.Instance.PlayInGameSound("BulletFX_Hit_Ricochet", collision.contacts[0].point, true);
        }

        RemoveEntityFromScene();
    }
}