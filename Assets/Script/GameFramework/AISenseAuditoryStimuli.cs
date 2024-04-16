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

        readonly List<AIPerceptionSensor> m_ActivelyStimulatedSensors = new();

        private void Update() => ProcessConstantStimulation();

        private void ProcessConstantStimulation()
        {
            if (m_ActivelyStimulating)
            {
                var colliders = Physics.OverlapSphere(transform.position, m_AudibleDistance, m_AIPerceptionSensorMask, QueryTriggerInteraction.Ignore);

                foreach (var collider in colliders)
                {
                    if (collider.TryGetComponent<AIPerceptionSensor>(out var sensor) && !m_ActivelyStimulatedSensors.Contains(sensor))
                        m_ActivelyStimulatedSensors.Add(sensor);
                    else
                    {
                        foreach (AIPerceptionSensor existingSensors in m_ActivelyStimulatedSensors)
                        {
                            existingSensors.StimulateSense(this);
                        }
                    }
                }
            }
            else
            {
                if (m_ActivelyStimulatedSensors.Count > 0)
                {
                    m_ActivelyStimulatedSensors.Clear();
                }
            }
        }

        public void SetActiveStimulation(bool active) => m_ActivelyStimulating = active;

        private void OnCollisionEnter(Collision collision)
        {
            if (m_ActivateOnInpact)
            {
                var colliders = Physics.OverlapSphere(transform.position, m_AudibleDistance, m_AIPerceptionSensorMask, QueryTriggerInteraction.Ignore);

                if (colliders.Length > 0)
                {
                    foreach (var collider in colliders)
                    {
                        if (collider.TryGetComponent<AIPerceptionSensor>(out var sensor))
                        {
                            sensor.StimulateSense(this);
                        }
                    }
                }
            }
        }
    }
}