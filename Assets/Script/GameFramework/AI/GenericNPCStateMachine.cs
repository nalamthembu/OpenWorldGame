using UnityEngine;

namespace OWFramework.AI
{
    public class GenericNPCStateMachine : BaseAIStateMachine
    {
        [SerializeField] AIIdle m_IdleState = new();
        [SerializeField] AIShock m_ShockState = new();

        protected override void Awake()
        {
            base.Awake();

            SwitchState(AIStateEnum.Idle);
        }

        public override void SwitchState(AIStateEnum newState)
        {
            base.SwitchState(newState);

            m_CurrentState?.OnExit(this);

            switch (newState)
            {
                case AIStateEnum.Idle:
                    m_CurrentState = m_IdleState; break;

                case AIStateEnum.Shock:
                    m_CurrentState = m_ShockState; break;
            }

            m_CurrentState.OnEnter(this);
        }
    }
}