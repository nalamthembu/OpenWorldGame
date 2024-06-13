using UnityEditorInternal;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
using UnityEngine.AI;

namespace OWFramework.AI
{
    public abstract class BaseAIState
    {
        // What happens just before we enter the state.
        public abstract void OnEnter(BaseAIStateMachine machine);

        // What happens just before we exit this state.
        public abstract void OnExit(BaseAIStateMachine machine);

        // What happens during each 'Tick' or frame we are in the state.
        public abstract void OnUpdate(BaseAIStateMachine machine);

        // What happens during each check for a need to switch states.
        public abstract void OnCheckTransition(BaseAIStateMachine machine);

        // Draw any needed Gizmos.
        public virtual void OnDrawGizmos() { }

        public virtual void NPCReactToDanger(Vector3 sourceOfDanger, BaseAIStateMachine machine)
        {
            // Switch Mood to Scared
            machine.SetMood(AIMood.Scared);
            machine.SwitchState(AIStateEnum.Shock);
            machine.DangerLocation = sourceOfDanger;
        }

        public virtual void OnAnimateIK() { }

        // Get Information about the Danger Location and this AI
        protected void GetDangerLocInfo(
            BaseAIStateMachine machine,
            Vector3 sampleDir, out float dotProduct,
            out Vector3 dirToDanger)
        {
            Vector3 a = machine.transform.position;
            Vector3 b = machine.DangerLocation;
            dirToDanger = (a - b).normalized;
            dotProduct = Vector3.Dot(sampleDir, dirToDanger);
        }

        // Get Information about a specified location and this AI
        protected void GetLocationMetrics(
            BaseAIStateMachine machine, 
            Vector3 sampleDir, Vector3 
            samplePosition, out float dot)
        {
            Vector3 a = machine.transform.position;
            Vector3 b = samplePosition;
            Vector3 dirToSamplePos = (a - b).normalized;
            dot = Vector3.Dot(sampleDir, dirToSamplePos);
        }

        // Find a random safe position away from dangerPos
        protected Vector3 GetRandomSafePosition(BaseAIStateMachine machine, Vector3 dangerPos, float minDistance, float maxDistance, int MAX_ITERATIONS = 50)
        {
            for (int i = 0; i < MAX_ITERATIONS; i++)
            {
                Vector3 randomDirection = Random.insideUnitSphere * maxDistance;
                randomDirection += machine.transform.position; // Use the current position as the origin for the search

                if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, maxDistance, NavMesh.AllAreas))
                {
                    float distance = Vector3.Distance(hit.position, dangerPos);
                    if (distance >= minDistance)
                    {
                        return hit.position;
                    }
                }
            }
            return Vector3.zero; // Return Vector3.zero if no valid position is found
        }
    }
}