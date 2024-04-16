using System;
using UnityEngine;

namespace OWFramework.AI
{
    public class AISenseVisualStimuli : AISenseStimuli 
    {
        internal enum StimuliColliderShape
        {
            NotSet,
            Box,
            Capsule,
            Sphere,
            Mesh,
        };

        [SerializeField] private StimuliColliderShape m_StimuliColliderShape;
        private Collider m_StimuliCollider;

                                //<Who Saw It?,        What they Saw>
        public static event Action<AIPerceptionSensor, AISenseVisualStimuli> OnSeenBySensor;
        public void OnSeen(AIPerceptionSensor sensor) => OnSeenBySensor?.Invoke(sensor, this);

        private void OnValidate()
        {
            if (m_StimuliCollider == null)
            {
                m_StimuliCollider = m_StimuliColliderShape switch
                {
                    StimuliColliderShape.Mesh => gameObject.GetComponent<MeshCollider>(),
                    StimuliColliderShape.Capsule => gameObject.AddComponent<CapsuleCollider>(),
                    StimuliColliderShape.Box => gameObject.AddComponent<BoxCollider>(),
                    StimuliColliderShape.Sphere => gameObject.AddComponent<SphereCollider>(),
                };

                m_StimuliCollider.isTrigger = true;
            }
        }
    }
}