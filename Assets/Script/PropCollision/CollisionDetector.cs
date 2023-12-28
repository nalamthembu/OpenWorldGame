using System.Collections;
using UnityEngine;

public class CollisionDetector : MonoBehaviour, ICollidable
{
    [SerializeField] string m_ParticleSystemObjectPoolID;

    [SerializeField] GameObject m_DestroyedSubObject;

    [SerializeField] string m_CollisionSoundID, m_BulletHitSoundID;

    [SerializeField] bool useSoundSweetener, IsDestructable;

    public void GotHit(Vector3 hitPoint, Vector3 hitNormal)
    {
        //IF WE HAVE AN ASSIGNED PARTICLE SYSTEM
        if (m_ParticleSystemObjectPoolID != string.Empty)
        {
            if (ObjectPoolManager.Instance.TryGetPool(m_ParticleSystemObjectPoolID, out Pool pool))
            {
                if (pool.GetGameObject().TryGetComponent<ParticleSystem>(out var particle))
                {
                    StartCoroutine(PlayParticleFX(particle, hitPoint, hitNormal, 5.0F));
                }
            }
        }

        if (m_DestroyedSubObject != null)
        {
            m_DestroyedSubObject.SetActive(true);
        }

        if (m_CollisionSoundID != string.Empty)
        {
            SoundManager.Instance.PlayInGameSound(m_CollisionSoundID, hitPoint, true, 5.0F);
        }
    }

    public void GotShot(Vector3 hitPoint, Vector3 hitNormal)
    {
        //IF WE HAVE AN ASSIGNED PARTICLE SYSTEM
        if (m_ParticleSystemObjectPoolID != string.Empty)
        {
            if (ObjectPoolManager.Instance.TryGetPool(m_ParticleSystemObjectPoolID, out Pool pool))
            {
                if (pool.GetGameObject().TryGetComponent<ParticleSystem>(out var particle))
                {
                    StartCoroutine(PlayParticleFX(particle, hitPoint, hitNormal, 5.0F));
                }
            }
        }

        if (IsDestructable)
        {
            if (m_DestroyedSubObject != null)
            {
                Destroy(m_DestroyedSubObject, 30.0F);
                gameObject.SetActive(false);
                m_DestroyedSubObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        if (m_BulletHitSoundID != string.Empty)
        {
            SoundManager.Instance.PlayInGameSound(m_BulletHitSoundID, hitPoint, true, 5.0F);

            if (useSoundSweetener)
            {
                SoundManager.Instance.PlayInGameSound("Generic_Sub_Sweeteneer", hitPoint, true, 1.25F);
            }
        }
    }

    public IEnumerator PlayParticleFX(ParticleSystem particle, Vector3 hitPoint, Vector3 hitNormal, float timeBeforeRemoval)
    {
        particle.gameObject.transform.position = hitPoint;
        particle.gameObject.transform.forward = -hitNormal;
        particle.Play();

        float currentTime = 0;

        while(currentTime < timeBeforeRemoval)
        {
            currentTime += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        particle.Stop();

        particle.gameObject.SetActive(false);
    }
}
