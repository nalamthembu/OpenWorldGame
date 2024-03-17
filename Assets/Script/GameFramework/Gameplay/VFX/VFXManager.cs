using UnityEngine;

public class VFXManager : MonoBehaviour
{
    [SerializeField] VFXData data;

    public static VFXManager Instance;

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