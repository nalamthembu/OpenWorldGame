using UnityEngine;

namespace OWFramework.AI
{
    public class GangMemberStateMachine : BaseAIStateMachine
    {
        // States
        [SerializeField] private AIIdle IdleState;
        [SerializeField] private AIWander WanderState;

        protected override void Start()
        {
            base.Start();

            SwitchState(AIStateEnum.Idle);
        }

        public override void SwitchState(AIStateEnum newState)
        {
            base.SwitchState(newState);

            m_CurrentState?.OnExit(this);

            m_CurrentState = newState switch
            {
                AIStateEnum.Idle => IdleState,
                AIStateEnum.Wandering => WanderState,
                _ => IdleState
            };

            m_CurrentState.OnEnter(this);

            m_CurrentState.OnUpdate(this);
        }
    }
}