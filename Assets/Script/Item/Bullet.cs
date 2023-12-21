using UnityEngine;

public class Bullet : Projectile
{
    [SerializeField] int maxRicochetCount = 2;

    public int MaxRicochetCount { get { return maxRicochetCount; } }
    public int RichochetCount { get; set; }

    private float maxRange;

    readonly float timeOut = 1;

    private float currentTime;

    new private Rigidbody rigidbody;

    public void InitialiseBullet(float thrustForce, float damageOnImpact, float maxRange)
    {
        this.maxRange = maxRange;
        this.damageOnImpact = damageOnImpact;

        if (rigidbody is null)
        {
            rigidbody = gameObject.AddComponent<Rigidbody>();

            rigidbody.mass = 0.05F;

            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            rigidbody.AddForce(transform.forward * thrustForce / 10, ForceMode.Impulse);
        }
    }

    private void Update()
    {
        currentTime += Time.deltaTime;

        if (currentTime < timeOut)
            return;

        if ((initialPosition - transform.position).sqrMagnitude >= maxRange * maxRange)
        {
            gameObject.SetActive(false);

            currentTime = 0;
        }
    }
}