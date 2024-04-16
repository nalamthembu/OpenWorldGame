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
                StimulateSense(visualStimuli);
            }
        }

        public bool HasPerceptionOf(AIPerceptionEnum type) => m_Perception.Contains(type);


        private void OnValidate()
        {
            if (HasPerceptionOf(AIPerceptionEnum.Sight) && m_FieldOfView == null)
                m_FieldOfView = gameObject.AddComponent<FieldOfView>();
        }

        public void StimulateSense(AISenseStimuli stimuli)
        {
            foreach (var sense in m_Perception)
            {
                switch (sense)
                {
                    case AIPerceptionEnum.Sight:

                        // Send out event on what you saw.
                         
                        break;

                    case AIPerceptionEnum.Hearing:
                        break;

                    case AIPerceptionEnum.Touch:
                        break;
                }
            }
        }
    }
}