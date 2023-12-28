using UnityEngine;
using System.Collections;

public interface ICollidable
{
    public abstract void GotShot(Vector3 hitPoint, Vector3 hitNormal);
    public abstract void GotHit(Vector3 hitPoint, Vector3 hitNormal);
    public abstract IEnumerator PlayParticleFX(ParticleSystem particle, Vector3 hitPoint, Vector3 hitNormal, float timeBeforeRemoval);
}