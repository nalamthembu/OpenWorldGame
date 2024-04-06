using System.Collections.Generic;
using UnityEngine;

public class BulletShellCollisionHandler : MonoBehaviour
{
    [SerializeField] string m_CollisionSoundID;

    private ParticleSystem m_ParticleSystem;
    public List<ParticleCollisionEvent> collisionEvents;

    private void Awake()
    {
        m_ParticleSystem = GetComponent<ParticleSystem>();
        collisionEvents = new();
    }

    private void OnParticleCollision(GameObject other)
    {
        // TODO : Detect Surfaces

        m_ParticleSystem.GetCollisionEvents(other, collisionEvents);

        foreach (ParticleCollisionEvent collisionEvent in collisionEvents)
        {
            if (collisionEvent.velocity.sqrMagnitude >= 5 * 5)
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayInGameSound(m_CollisionSoundID, transform.position, true, 0.5F);

                    break;
                }
            }
        }
    }
}