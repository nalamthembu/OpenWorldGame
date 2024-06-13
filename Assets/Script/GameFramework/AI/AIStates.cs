using OWFramework.AI;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.AI;
using UnityEngine.Profiling;

namespace OWFramework
{
    public enum AIStateEnum
    {
        Idle,
        Wandering,
        Shock,
        Flee
    };

    // Idle State (Stand around and do nothing)

    [System.Serializable]
    public class AIIdle : BaseAIState
    {
        [SerializeField] bool       m_ReactToDistubance;
        [SerializeField] bool       m_DoNotWander = false;
        [SerializeField] Vector2    IdleInterval = new(5, 10);

        private Transform   transform;
        float               calculatedInterval;
        float               idleTimer = 0;
        BaseAIStateMachine  stateMachine;

        public override void OnCheckTransition(BaseAIStateMachine machine)
        {
            if (!m_DoNotWander)
            {
                idleTimer += Time.deltaTime;

                if (idleTimer >= calculatedInterval)
                {
                    idleTimer = 0;
                    calculatedInterval = CalculateInterval();

                    //Switch to Wandering State
                    machine.SwitchState(AIStateEnum.Wandering);
                }
            }
        }

        public override void OnEnter(BaseAIStateMachine machine)
        {
            calculatedInterval = CalculateInterval();

            // SET MACHINE
            if (!stateMachine)
                stateMachine = machine;

            // Stop the character from moving.
            machine.SetNavigationEnabled(false);

            // React to disturbance - sub to events
            if (m_ReactToDistubance)
            {
                machine.PerceptionSensor.OnAuditoryContactMade += OnHeardSomething;
                machine.PerceptionSensor.OnPhysicalContactMade += OnPhysicalContactMade;
                machine.PerceptionSensor.OnVisualContactMade += OnVisualContactMade;
                Debug.Log("Subbed to all sensory inputs!");
            }

            transform = machine.transform;
        }

        private void OnHeardSomething(AISenseAuditoryStimuli stimuli)
        {
            Debug.Log("Heard something!");

            switch (stateMachine.NPCType)
            {
                case NPCType.Civvie:

                    if (stimuli.ThreatType == ThreatType.IsAThreat)
                    {
                        NPCReactToDanger(stimuli.transform.position, stateMachine);
                        Debug.Log("Heard a threat!");
                    }
                    break;


                    // TODO : Have Hostiles react to threats by charging at them.

                    // TODO : Have Hostiles react to non-threats by investigating.
            }
        }

        private void OnPhysicalContactMade() { }

        private void OnVisualContactMade(AISenseVisualStimuli stimuli) { }

        public override void OnExit(BaseAIStateMachine machine)
        {
            // Enable the character to move.
            machine.SetNavigationEnabled(true);

            // Unsub from events
            if (m_ReactToDistubance)
            {
                machine.PerceptionSensor.OnAuditoryContactMade -= OnHeardSomething;
                machine.PerceptionSensor.OnPhysicalContactMade -= OnPhysicalContactMade;
                machine.PerceptionSensor.OnVisualContactMade -= OnVisualContactMade;
            }
        }

        private float CalculateInterval() => Random.Range(IdleInterval.x, IdleInterval.y);
        public override void OnUpdate(BaseAIStateMachine machine) { }
    }

    // Wander State (Walk/Run around aimlessly)

    [System.Serializable]
    public class AIWander : BaseAIState
    {
        [SerializeField] float maxDistance;
        [SerializeField] float minDistanceFromTarget;
        [SerializeField] float WanderDuration;
        [SerializeField] bool  m_ReactToDistubance;

        //Private Members
        Vector3             calculatedRandomPosition;
        Vector3             centre; // This is used during random point calculations.
        Transform           transform; // This is the AI Attached to this State Machine.
        float               wanderTimer;
        BaseAIStateMachine  stateMachine;

        public override void OnCheckTransition(BaseAIStateMachine machine)
        {
            wanderTimer += Time.deltaTime;

            if (wanderTimer >= WanderDuration)
            {
                wanderTimer = 0;
                machine.SwitchState(AIStateEnum.Idle);
            }
        }

        public override void OnExit(BaseAIStateMachine machine)
        { }

        public override void OnEnter(BaseAIStateMachine machine)
        {
            if (transform == null)
                transform = machine.transform;

            centre = machine.Agent.transform.position;

            // Enable Movement
            machine.SetNavigationEnabled(true);

            // Calculate New Destination
            CalculateRandomPosition();
            GoToCalculatedPosition(machine.Agent);

            // React to disturbance - sub to events
            if (m_ReactToDistubance)
            {
                machine.PerceptionSensor.OnAuditoryContactMade += OnHeardSomething;
                machine.PerceptionSensor.OnPhysicalContactMade += OnPhysicalContactMade;
                machine.PerceptionSensor.OnVisualContactMade += OnVisualContactMade;
            }
        }

        private void OnHeardSomething(AISenseAuditoryStimuli stimuli)
        {
            switch (stateMachine.NPCType)
            {
                case NPCType.Civvie:

                    if (stimuli.ThreatType == ThreatType.IsAThreat)
                        NPCReactToDanger(stimuli.transform.position, stateMachine);
                    break;

                    // TODO : Have Hostiles react to threats by charging at them.

                    // TODO : Have Hostiles react to non-threats by investigating.
            }
        }

        private void OnPhysicalContactMade() { }

        private void OnVisualContactMade(AISenseVisualStimuli stimuli) { }

        public override void OnUpdate(BaseAIStateMachine machine)
        {
            if (machine.Agent == null) return;

            // Keep finding a new random location to go to.
            if (machine.Agent.remainingDistance <= minDistanceFromTarget || machine.Agent.destination == null)
            {
                CalculateRandomPosition();
                GoToCalculatedPosition(machine.Agent);
            }
        }

        public void SetRandomParameters(Vector3 centre, float maxDistance)
        {
            this.centre = centre;
            this.maxDistance = maxDistance;
        }

        private void CalculateRandomPosition()
        {
            int maxIterations = 100;

            for (int i = 0; i < maxIterations; i++)
            {
                calculatedRandomPosition = centre + (Vector3)Random.insideUnitCircle * maxDistance;

                // Check if its on a Nav Mesh.
                if (NavMesh.SamplePosition(calculatedRandomPosition, out var hit, maxDistance, NavMesh.AllAreas))
                    calculatedRandomPosition = hit.position;
            }
        }

        private void GoToCalculatedPosition(NavMeshAgent agent)
        {
            if (agent != null)
            {
                agent.SetDestination(calculatedRandomPosition);
            }
        }

        public override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(calculatedRandomPosition, 10);
        }
    }

    // Shock State (Heard or saw something threatening)
    [System.Serializable]
    public class AIShock : BaseAIState
    {
        [SerializeField] float m_ShockDuration = 3;

        float m_ShockTimer = 0;

        public override void OnCheckTransition(BaseAIStateMachine machine)
        {
            // TODO : Switch to Flee State

            m_ShockTimer += Time.deltaTime;

            if (m_ShockTimer >= m_ShockDuration)
            {
                machine.SwitchState(AIStateEnum.Flee);
            }
        }

        public override void OnEnter(BaseAIStateMachine machine)
        {
            // Reset Shock Timer
            m_ShockTimer = 0;

            // Stop Moving
            machine.SetNavigationEnabled(false);

            // Trigger Shock Animation
            machine.Animator.CrossFadeInFixedTime("React_Shock", 0.25F, 2);

            // Express shock
            machine.CharacterSpeech.DoReaction(ReactionType.Frightened);
        }

        public override void OnExit(BaseAIStateMachine machine) {/*DO NOTHING*/}

        public override void OnUpdate(BaseAIStateMachine machine)
        {
            GetDangerLocInfo(machine, machine.transform.right, out var dot, out _);
            machine.Animator.SetFloat("DANGER_DOT", dot);
        }
    }

    // Conversation State (Talk to another AI)

    // Flee (No! F##K this, I'm out of here!)
    [System.Serializable]
    public class AIFlee : BaseAIState
    {
        [SerializeField] float m_MinDist = 10;
        [SerializeField] float m_MaxDist = 25;
        [SerializeField] float m_DistanceBeforeSwitching = 2.5F;

        Vector3 m_SafePosition;

        public override void OnCheckTransition(BaseAIStateMachine machine)
        {
            if (machine.Agent)
            {
                // We're close enough to switch states
                if (machine.Agent.remainingDistance <= m_DistanceBeforeSwitching)
                {
                    machine.SwitchState(AIStateEnum.Idle);
                }
            }
        }

        public override void OnEnter(BaseAIStateMachine machine)
        {
            // Set mood if it isn't correct
            if (machine.GetMood() != AIMood.Scared) machine.SetMood(AIMood.Scared);

            m_SafePosition = GetRandomSafePosition(machine, machine.DangerLocation, m_MinDist, m_MaxDist);

            machine.SetNavigationEnabled(true);

            machine.GoToPosition(m_SafePosition);
        }

        public override void OnExit(BaseAIStateMachine machine) { }

        public override void OnUpdate(BaseAIStateMachine machine) { }
    }
}