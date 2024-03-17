using UnityEngine;

[CreateAssetMenu(fileName = "VFXData", menuName = "Game/VFX/Visual Effects Data")]
public class VFXData :ScriptableObject
{
    [SerializeField] VisualEffect[] effects;

    public VisualEffect GetVisualEffect(string id)
    {
        foreach(var effect in effects)
        {
            if (effect.GetID()== id)
            {
                return effect;
            }
        }

        Debug.LogError($"Could not find visual effect with ID : {id}.");

        return null;
    }
}

[System.Serializable]
public class VisualEffect
{
    [SerializeField] string id;
    [SerializeField] GameObject m_prefab;
    public GameObject GetPrefab() => m_prefab;
    public string GetID () => id;
}