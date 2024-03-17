public class GeneralAudioAmbienceComponent : BaseAudioAmbience
{
    private void OnEnable()
    {
        AreaAudioAmbienceComponent.OnEnteredAreaAmbienceZone += OnCameraEnteredAreaAmbienceZone;
        AreaAudioAmbienceComponent.OnExitedAreaAmbienceZone += OnCameraExitedAreaAmbienceZone;
    }

    private void OnDisable()
    {
        AreaAudioAmbienceComponent.OnEnteredAreaAmbienceZone -= OnCameraEnteredAreaAmbienceZone;
        AreaAudioAmbienceComponent.OnExitedAreaAmbienceZone -= OnCameraExitedAreaAmbienceZone;
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(FadeIn());
    }

    private void OnCameraEnteredAreaAmbienceZone()
    {
        StartCoroutine(FadeIn());
        m_IsActive = true;
    }
    private void OnCameraExitedAreaAmbienceZone()
    {
        StartCoroutine(FadeOut());
        m_IsActive = false;
    }

}
