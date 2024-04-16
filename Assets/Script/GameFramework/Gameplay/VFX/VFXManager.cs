using UnityEngine;

public class VFXManager : MonoBehaviour
{
    [SerializeField] VFXData data;

    public static VFXManager Instance;

    [SerializeField] bool m_debugSlowMo = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.P))
            m_debugSlowMo = !m_debugSlowMo;
        Time.timeScale = m_debugSlowMo ? 0.5f : 1;
        */
    }

    public void SpawnVisualEffect(string id, Vector3 position, Quaternion rotation)
    {
        if (data != null)
        {
            var vfx = data.GetVisualEffect(id);

            GameObject vfxGO = Instantiate(vfx.GetPrefab(), position, rotation);

            Destroy(vfxGO, 5);
        }
    }
}