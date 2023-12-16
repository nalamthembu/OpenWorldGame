using UnityEngine;
using System.Collections;

//Central Class for Locomotion States

public class IdleState : ILocomotionState
{
    LocomotionStateMachine stateMachine;

    float m_IdleIndex;

    int m_CurrentIdleIndex;

    float m_SeekTime = 0;

    const int MAX_TIME = 10;

    public IdleState(LocomotionStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public void OnStateEnter()
    {
        stateMachine.CheckFeet(out _);
        
        stateMachine.Character.TargetSpeed = 0;
    }

    public void OnStateUpdate()
    {
        m_SeekTime += Time.deltaTime;

        if (m_SeekTime >= MAX_TIME)
        {
            m_CurrentIdleIndex = Random.Range(0, stateMachine.IdleAnimationCount);

            m_SeekTime = 0;
        }

        m_IdleIndex = Mathf.Lerp(m_IdleIndex, m_CurrentIdleIndex, Time.deltaTime);

        return;
    }

    public void OnStateCheck()
    {

        if (stateMachine.Character is PlayerController player)
        {
            if (PlayerInput.Instance.InputMagnitude > 0)
            {
                stateMachine.TransitionTo(PlayerInput.Instance.IsRunning ? stateMachine.runState : stateMachine.walkState);
            }
        }

        if (!stateMachine.Character.IsGrounded())
        {
            stateMachine.TransitionTo(stateMachine.inAirState);
        }
    }

    public void OnStateEnd()
    {
        return;
    }

    public void OnAnimate()
    {
        stateMachine.Character.Animator.SetFloat(GameStrings.IDLE_INDEX, m_IdleIndex);

        stateMachine.ResetFeet();
    }
}

public class WalkState : ILocomotionState
{
    LocomotionStateMachine stateMachine;

    public WalkState(LocomotionStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public void OnStateEnter()
    {
        stateMachine.ResetFeet();
        stateMachine.Character.TargetSpeed = stateMachine.Character.WalkSpeed;
    }

    public void OnStateUpdate()
    {
        if (stateMachine.Character is PlayerController player)
        {
            player.MoveCharacter();
            player.RotateCharacter();
        }
    }

    public void OnStateCheck()
    {
        if (stateMachine.Character is PlayerController player)
        {
            if (PlayerInput.Instance.InputMagnitude > 0)
            {
                if (PlayerInput.Instance.IsRunning)
                {
                    stateMachine.TransitionTo(stateMachine.runState);
                }

                //If I jump go to jump state
                if (PlayerInput.Instance.IsJumping && player.IsGrounded())
                {
                    stateMachine.TransitionTo(stateMachine.jumpState);
                }
            }
            else
            {
                stateMachine.TransitionTo(stateMachine.stopState);
            }
        }

        if (!stateMachine.Character.IsGrounded())
        {
            stateMachine.TransitionTo(stateMachine.inAirState);
        }
    }

    public void OnStateEnd()
    {
        stateMachine.ResetFeet();
    }

    public void OnAnimate()
    {
        if (stateMachine.Character is PlayerController player)
        {
            stateMachine.Animator.SetFloat(GameStrings.INPUT_MAGNITUDE, PlayerInput.Instance.InputMagnitude);
            stateMachine.Animator.SetFloat(GameStrings.TARGET_ROTATION, player.TargetRotation);
            stateMachine.Animator.SetFloat(GameStrings.CURRENT_SPEED, player.CurrentSpeed);
        }
    }
}

public class RunState : ILocomotionState
{
    LocomotionStateMachine stateMachine;

    public RunState(LocomotionStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public void OnStateEnter()
    {
        stateMachine.Character.TargetSpeed = stateMachine.Character.RunSpeed;
    }

    public void OnStateUpdate()
    {
        if (stateMachine.Character is PlayerController player)
        {
            player.MoveCharacter();
            player.RotateCharacter();
        }
    }

    public void OnStateCheck()
    {
        if (stateMachine.Character is PlayerController player)
        {
            if (PlayerInput.Instance.InputMagnitude > 0)
            {
                if (!PlayerInput.Instance.IsRunning)
                {
                    stateMachine.TransitionTo(stateMachine.walkState);
                }

                //If I jump go to jump state
                if (PlayerInput.Instance.IsJumping && player.IsGrounded())
                {
                    stateMachine.TransitionTo(stateMachine.jumpState);
                }
            }
            else
            {
                stateMachine.TransitionTo(stateMachine.stopState);
            }
        }

        if (!stateMachine.Character.IsGrounded())
        {
            stateMachine.TransitionTo(stateMachine.inAirState);
        }
    }

    public void OnStateEnd()
    {
        return;
    }

    public void OnAnimate()
    {
        if (stateMachine.Character is PlayerController player)
        {
            stateMachine.Animator.SetFloat(GameStrings.INPUT_MAGNITUDE, PlayerInput.Instance.InputMagnitude);
            stateMachine.Animator.SetFloat(GameStrings.TARGET_ROTATION, player.TargetRotation);
            stateMachine.Animator.SetFloat(GameStrings.CURRENT_SPEED, player.CurrentSpeed);
        }
    }
}

public class StopState : ILocomotionState
{
    LocomotionStateMachine stateMachine;
    public StopState(LocomotionStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public void OnStateEnter()
    {
        stateMachine.CheckFeet(out _);
        stateMachine.Character.TargetSpeed = 0;
    }

    public void OnStateUpdate()
    {
        if (stateMachine.Character is PlayerController player)
        {
            player.MoveCharacter();
           // player.RotateCharacter();
        }
    }

    public void OnStateCheck()
    {
        if (stateMachine.Character.CurrentSpeed <= 0)
        {
            stateMachine.TransitionTo(stateMachine.idleState);
        }

        if (!stateMachine.Character.IsGrounded())
        {
            //TO-DO : Should there be a state for falling?? (Like, complete loss of air control)
            stateMachine.TransitionTo(stateMachine.inAirState);
        }
    }

    public void OnStateEnd()
    {
        stateMachine.ResetFeet();
    }

    public void OnAnimate()
    {
        if (stateMachine.Character is PlayerController player)
        {
            stateMachine.Animator.SetFloat(GameStrings.INPUT_MAGNITUDE, PlayerInput.Instance.InputMagnitude);
            stateMachine.Animator.SetFloat(GameStrings.TARGET_ROTATION, player.TargetRotation);
            stateMachine.Animator.SetFloat(GameStrings.CURRENT_SPEED, player.CurrentSpeed);
        }
    }
}

public class JumpState : ILocomotionState
{
    LocomotionStateMachine stateMachine;

    string footThatTouchedTheGroundFirst;

    public JumpState(LocomotionStateMachine locomotionStateMachine)
    {
        this.stateMachine = locomotionStateMachine;
    }

    public void OnAnimate()
    {
        if (stateMachine.Character is PlayerController player)
        {
            bool leftFootHitFirst = footThatTouchedTheGroundFirst == GameStrings.LU;

            string animStateToSwitchTo = leftFootHitFirst ? GameStrings.JUMP_LU : GameStrings.JUMP_RU;

            player.Animator.CrossFade(animStateToSwitchTo, 0.01F);

            stateMachine.Animator.SetFloat(GameStrings.INPUT_MAGNITUDE, PlayerInput.Instance.InputMagnitude);
            stateMachine.Animator.SetFloat(GameStrings.TARGET_ROTATION, player.TargetRotation);
            stateMachine.Animator.SetFloat(GameStrings.CURRENT_SPEED, player.CurrentSpeed);
        }
    }

    public void OnStateCheck()
    {
        if (!stateMachine.Character.IsGrounded())
        {
            stateMachine.TransitionTo(stateMachine.inAirState);
        }
    }

    public void OnStateEnd()
    {
        stateMachine.ResetFeet();
    }

    public void OnStateEnter()
    {
        if (stateMachine.Character is PlayerController player)
        {
            player.IsJumping = true;
        }
        stateMachine.CheckFeet(out _);
    }

    public void OnStateUpdate()
    {
        if (stateMachine.Character is PlayerController player)
        {
            player.MoveCharacter();
            player.RotateCharacter();
        }
    }
}

public class InAirState : ILocomotionState
{
    LocomotionStateMachine stateMachine;

    public InAirState(LocomotionStateMachine locomotionStateMachine)
    {
        this.stateMachine = locomotionStateMachine;
    }

    public void OnAnimate()
    {
        /*
        if (stateMachine.Character is PlayerController player)
        {
            if (!player.Animator.IsCurrentStateName(GameStrings.FALLING))
            player.Animator.CrossFade(GameStrings.FALLING, 0.01F);
        }
        */
    }

    public void OnStateCheck()
    {
        if (stateMachine.Character.IsGrounded())
        {
            stateMachine.TransitionTo(stateMachine.landState);
        }
    }

    public void OnStateEnd()
    {
        return;
    }

    public void OnStateEnter()
    {
        
    }

    public void OnStateUpdate()
    {
        if (stateMachine.Character is PlayerController player)
        {
            player.MoveCharacter();
            player.RotateCharacter();
        }
    }
}

public class LandState : ILocomotionState
{
    LocomotionStateMachine stateMachine;

    string footThatTouchedTheGroundFirst = string.Empty;

    public LandState(LocomotionStateMachine locomotionStateMachine)
    {
        this.stateMachine = locomotionStateMachine;
    }

    public void OnAnimate()
    {
        if (stateMachine.Character is PlayerController player)
        {
            if (player.Animator.IsCurrentStateName(GameStrings.FALLING))
            {
                bool leftFootHitFirst = footThatTouchedTheGroundFirst == GameStrings.LU;

                string animStateToSwitchTo = leftFootHitFirst ? GameStrings.LAND_LU : GameStrings.LAND_RU;

                player.Animator.CrossFade(animStateToSwitchTo, 0.01F);
            }

            stateMachine.Animator.SetFloat(GameStrings.INPUT_MAGNITUDE, PlayerInput.Instance.InputMagnitude);
            stateMachine.Animator.SetFloat(GameStrings.TARGET_ROTATION, player.TargetRotation);
            stateMachine.Animator.SetFloat(GameStrings.CURRENT_SPEED, player.CurrentSpeed);
        }
    }

    public void OnStateCheck()
    {
        if (PlayerInput.Instance.InputMagnitude > 0)
        {
            if (PlayerInput.Instance.IsRunning)
            {
                stateMachine.TransitionTo(stateMachine.runState);
            }

            if (!PlayerInput.Instance.IsRunning)
            {
                stateMachine.TransitionTo(stateMachine.walkState);
            }
        }
        else
        {
            stateMachine.TransitionTo(stateMachine.idleState);
        }
    }

    public void OnStateEnd()
    {
        stateMachine.ResetFeet();
    }

    public void OnStateEnter()
    {
        stateMachine.CheckFeet(out footThatTouchedTheGroundFirst);
    }

    public void OnStateUpdate()
    {
        if (stateMachine.Character is PlayerController player)
        {
            if (PlayerInput.Instance.InputMagnitude > 0)
            {
                //Rotate but don't move.
                player.RotateCharacter();
            }
        }
    }
}

public class RollState : ILocomotionState
{
    LocomotionStateMachine stateMachine;

    public RollState(LocomotionStateMachine locomotionStateMachine)
    {
        this.stateMachine = locomotionStateMachine;
    }

    public void OnAnimate()
    {
        throw new System.NotImplementedException();
    }

    public void OnStateCheck()
    {
        throw new System.NotImplementedException();
    }

    public void OnStateEnd()
    {
        throw new System.NotImplementedException();
    }

    public void OnStateEnter()
    {
        throw new System.NotImplementedException();
    }

    public void OnStateUpdate()
    {
        throw new System.NotImplementedException();
    }
}
