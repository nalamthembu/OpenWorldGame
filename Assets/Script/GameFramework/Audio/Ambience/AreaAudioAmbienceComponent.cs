using System;
using UnityEngine;

public class AreaAudioAmbienceComponent : BaseAudioAmbience
{
    protected Collider m_Collider;

    //Events
    public static Action OnEnteredAreaAmbienceZone;
    public static Action OnExitedAreaAmbienceZone;

    private void Awake() => InitialiseCollider();

    private void InitialiseCollider()
    {
        m_Collider = GetComponent<Collider>();
        m_Collider.isTrigger = true;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        //When the camera enters this zone.
        if (other.GetComponent<ThirdPersonCamera>())
        {
            StartCoroutine(FadeIn());

            OnEnteredAreaAmbienceZone?.Invoke();
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        //When the camera leaves this zone.
        if (other.GetComponent<ThirdPersonCamera>())
        {
            StartCoroutine(FadeOut());

            OnExitedAreaAmbienceZone?.Invoke();
        }
    }
}