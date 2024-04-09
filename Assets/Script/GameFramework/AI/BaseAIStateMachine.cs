using UnityEngine;
using UnityEngine.AI;

namespace OWFramework.AI
{
    public enum AIMood
    {
        Relaxed,
        Alert,
        Angry,
        Scared,
        Fearless
    };

    public class BaseAIStateMachine : MonoBehaviour
    { 
        protected BaseAIState m_CurrentState;

        protected BlackBoardComponent m_Blackboard = new();

        [SerializeField] protected AIMood m_Mood;

        [SerializeField] protected float m_RunSpeed;
        [SerializeField] protected float m_WalkSpeed;

        public float WalkSpeed { get { return m_WalkSpeed; } }
        public float RunSpeed { get { return m_RunSpeed; } }
        public AIMood GetMood() => m_Mood;

        public NavMeshAgent Agent { get; protected set; }

        protected virtual void Awake() => Agent = GetComponent<NavMeshAgent>();
        
        protected virtual void Start() => InitialiseBlackboard();

        protected virtual void InitialiseBlackboard()
        {
            // Get Player Position
            m_Blackboard.SetValue("m_PlayerTransform", PlayerCharacter.Instance.transform);

            // Player Last Known Position
            m_Blackboard.SetValue("m_LastKnowPlayerPosition", Vector3.zero);
        }

        public virtual void SwitchState(AIStateEnum newState) { }

        protected virtual void Update()
        {
            // Update the current state and always check if we need to switch states. 
            if (m_CurrentState != null)
            {
                m_CurrentState.OnUpdate(this);
                m_CurrentState.OnCheckTransition(this);
            }
        }

        protected virtual void OnDrawGizmos() => m_CurrentState?.OnDrawGizmos();

        public void SetNavigationEnabled(bool enable) => Agent.isStopped = !enable;
        
    }
}