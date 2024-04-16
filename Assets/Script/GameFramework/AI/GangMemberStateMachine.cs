using UnityEngine;

namespace OWFramework.AI
{
    public class GangMemberStateMachine : BaseAIStateMachine
    {
        // States
        [SerializeField] private AIIdle IdleState;
        [SerializeField] private AIWander WanderState;
        [SerializeField] private AIChase ChaseState;
        [SerializeField] private AIFollow FollowState;
        [SerializeField] private AIAimAtTarget AimAtTarget;

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
                AIStateEnum.Chase => ChaseState,
                AIStateEnum.Follow => FollowState,
                AIStateEnum.AimAtTarget => AimAtTarget,
                AIStateEnum.FireAtTarget => AimAtTarget,
                _ => IdleState
            };

            m_CurrentState.OnEnter(this);
        }
    }
}