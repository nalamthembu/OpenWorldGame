using OWFramework.AI;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.AI;

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

            // Determine Speed Based on Mood
            machine.Agent.speed = machine.GetMood() switch
            {
                AIMood.Relaxed => machine.WalkSpeed, // No worries, man.
                AIMood.Fearless => machine.RunSpeed, // Fight 
                AIMood.Scared => machine.RunSpeed, // Flight
                AIMood.Alert => machine.RunSpeed / 2, // Just go for a light jog.
                _ => machine.WalkSpeed
            };

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
        public override void OnCheckTransition(BaseAIStateMachine machine)
        {
            // TODO : Switch to Flee State

            if (!machine.Animator.GetCurrentAnimatorStateInfo(0).IsName("React_Shock"))
            {
                machine.SwitchState(AIStateEnum.Idle);
            }
        }

        public override void OnEnter(BaseAIStateMachine machine)
        {
            // Stop Moving
            machine.SetNavigationEnabled(false);

            // Trigger Shock Animation
            machine.Animator.CrossFadeInFixedTime("React_Shock", 0.25F, 0);

            // Express shock
            machine.CharacterSpeech.DoReaction(ReactionType.Frightened);
        }

        public override void OnExit(BaseAIStateMachine machine) {/*DO NOTHING*/}

        public override void OnUpdate(BaseAIStateMachine machine) {/*DO NOTHING*/}
    }

    // Conversation State (Talk to another AI)

    // Flee (No! F##K this, I'm out of here!)
}