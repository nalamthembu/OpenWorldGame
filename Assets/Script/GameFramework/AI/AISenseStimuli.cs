using UnityEngine;

namespace OWFramework.AI
{
    public enum ThreatType
    {
        NotAThreat,
        IsAThreat,
    };

    public class AISenseStimuli: MonoBehaviour
    {
        [SerializeField] protected ThreatType m_ThreatType = ThreatType.NotAThreat;
        public ThreatType ThreatType => m_ThreatType;
    }
}