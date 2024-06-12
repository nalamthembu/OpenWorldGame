using UnityEngine;
using System.Collections.Generic;

namespace OWFramework.AI
{
    public class AISenseAuditoryStimuli : AISenseStimuli
    {
        [SerializeField] bool       m_ActivateOnInpact;
        [SerializeField] float      m_AudibleDistance = 10.0F;
        [SerializeField] LayerMask  m_AIPerceptionSensorMask;
        [SerializeField] bool       m_ActivelyStimulating;
        [SerializeField] float      m_StimulationCooldownTime = 1;      // If AI's already heard this once, lets not overstimulate.

        float m_StimulationCooldownRemaining;

        private void Update()
        {
            if (m_StimulationCooldownRemaining <= 0)
            {
                ProcessConstantStimulation();
                m_StimulationCooldownRemaining = 0;
            }
            else
            {
                if (m_StimulationCooldownRemaining > 0)
                    m_StimulationCooldownRemaining -= Time.deltaTime;
            }
        }

        public void StimulateNearbyAI()
        {
            if (m_StimulationCooldownRemaining > 0)
                return;

            var colliders = Physics.OverlapSphere(transform.position, m_AudibleDistance, m_AIPerceptionSensorMask);

            Debug.Log($"Found : {colliders.Length} colliders");

            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent<AIPerceptionSensor>(out var sensor))
                {
                    sensor.StimulateSense(this);
                }
            }

            if (m_StimulationCooldownRemaining <= 0)
                m_StimulationCooldownRemaining += m_StimulationCooldownTime;
        }

        private void ProcessConstantStimulation()
        {
            if (m_ActivelyStimulating)
            {
                StimulateNearbyAI();
            }
        }

        public void SetActiveStimulation(bool active) => m_ActivelyStimulating = active;

        private void OnCollisionEnter(Collision collision)
        {
            if (m_ActivateOnInpact)
                StimulateNearbyAI();
        }
    }
}