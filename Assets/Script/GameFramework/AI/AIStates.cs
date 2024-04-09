using OWFramework.AI;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.AI;

namespace OWFramework
{
    public enum AIStateEnum
    {
        Idle,
        Wandering
    };

    [System.Serializable]
    public class AIIdle : BaseAIState
    {
        [SerializeField] Vector2 IdleInterval = new(5, 10);

        float calculatedInterval;
        float idleTimer = 0;

        public override void OnCheckTransition(BaseAIStateMachine machine)
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

        public override void OnEnter(BaseAIStateMachine machine)
        {
            calculatedInterval = CalculateInterval();

            // Stop the character from moving.
            machine.SetNavigationEnabled(false);
        }

        public override void OnExit(BaseAIStateMachine machine)
        { }

        private float CalculateInterval() => Random.Range(IdleInterval.x, IdleInterval.y);

        public override void OnUpdate(BaseAIStateMachine machine)
        { }
    }

    [System.Serializable]
    public class AIWander : BaseAIState
    {
        [SerializeField] float maxDistance;
        [SerializeField] float minDistanceFromTarget;
        [SerializeField] float WanderDuration;

        //Private Members
        Vector3 calculatedRandomPosition;
        Vector3 centre;
        Transform aiTransform;
        float wanderTimer;

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
            if (aiTransform == null)
                aiTransform = machine.transform;

            centre = machine.Agent.transform.position;

            // Enable Movement
            machine.SetNavigationEnabled(true);

            // Calculate New Destination
            CalculateRandomPosition();
            GoToCalculatedPosition(machine.Agent);
        }

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

    // Chase State (Target is running)
     
    // Investigate State (Investigate a noise or something you saw)

    // Find Cover (Caused by High Danger, like getting shot at)

    // Run and Gun (Trait of a fearless AI)

    // Fire At Target (Blind fire if Danger is high)

    // Conversation State (Talk to another AI)

    // Flee (No! F##K this, I'm out of here!)
}