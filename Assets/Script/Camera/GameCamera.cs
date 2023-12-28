using UnityEngine;

public class GameCamera : MonoBehaviour
{
    public ThirdPersonCamera m_ThirdPersonCamera;

    public DebugCamera m_DebugCamera;

    public static GameCamera Instance;

    private void Awake()
    {
        if (Instance is null)
            Instance = this;
        else
            Destroy(gameObject);

        m_ThirdPersonCamera.Initialise(transform, 60);
        m_DebugCamera.Initialise(transform, 50);
    }

    private void Update()
    {
        if (m_ThirdPersonCamera.m_IsActive)
            m_ThirdPersonCamera.Update();

        if (m_DebugCamera.m_IsActive)
            m_DebugCamera.Update();
    }

    private void LateUpdate()
    {
        if (m_ThirdPersonCamera.m_IsActive)
            m_ThirdPersonCamera.LateUpdate();

        if (m_DebugCamera.m_IsActive)
            m_DebugCamera.LateUpdate();
    }
}
