using UnityEngine;
using System.Collections.Generic;

namespace OWFramework.AI
{
    public enum AIPerceptionEnum
    {
        Sight,
        Hearing,
        Touch
    };

    public class AIPerceptionSensor : MonoBehaviour
    {
        [SerializeField] private List<AIPerceptionEnum> m_Perception; //What can they sense?

        private FieldOfView m_FieldOfView;

        #region Stimulation Events

        // Sight
        public delegate void OnVisualContact(AISenseVisualStimuli visualStimuli);
        public event OnVisualContact OnVisualContactMade;

        // Hearing
        public delegate void OnAuditoryContact(AISenseAuditoryStimuli auditoryStimuli);
        public event OnAuditoryContact OnAuditoryContactMade;

        // Touch
        public delegate void OnPhysicalContact(); // TODO : Physical Contact
        public event OnPhysicalContact OnPhysicalContactMade;

        #endregion

        private void Awake()
        {
            if (!m_FieldOfView && HasPerceptionOf(AIPerceptionEnum.Sight))
                m_FieldOfView = GetComponent<FieldOfView>();
        }

        private void Update()
        {
            // Process Sight
            if (m_FieldOfView != null &&
                m_FieldOfView.HasVisibleTarget(out var target) &&
                target.TryGetComponent<AISenseVisualStimuli>(out var visualStimuli))
            {
                if (m_FieldOfView.NoticeAmount >= 1)
                    StimulateSense(visualStimuli);
            }
        }

        public bool HasPerceptionOf(AIPerceptionEnum type) => m_Perception.Contains(type);
        public void StimulateSense(AISenseStimuli stimuli)
        {
            switch (stimuli)
            {
                case AISenseVisualStimuli:
                    if (m_Perception.Contains(AIPerceptionEnum.Sight))
                    {
                        OnVisualContactMade?.Invoke(stimuli as AISenseVisualStimuli);
                        print("ai_saw_something");
                    }
                    break;

                case AISenseAuditoryStimuli:
                    if (m_Perception.Contains(AIPerceptionEnum.Hearing))
                    {
                        OnAuditoryContactMade?.Invoke(stimuli as AISenseAuditoryStimuli);
                        print("ai_heared_something");
                    }
                    break;

                    // TODO : Touch
            }
        }
    }
}