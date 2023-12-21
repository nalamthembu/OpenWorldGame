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

    const int ASSIGNED_SPEED = 0;
    
    public IdleState(LocomotionStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public void OnStateEnter()
    {
        stateMachine.Character.TargetSpeed = ASSIGNED_SPEED;
    }

    public void OnStateUpdate()
    {
        if (GameStateMachine.Instance.currentState is GameStatePaused)
            return;

        m_SeekTime += Time.deltaTime;

        if (m_SeekTime >= MAX_TIME)
        {
            m_CurrentIdleIndex = Random.Range(0, stateMachine.IdleAnimationCount);

            m_SeekTime = 0;
        }

        m_IdleIndex = Mathf.Lerp(m_IdleIndex, m_CurrentIdleIndex, Time.deltaTime);

        if (stateMachine.Character is PlayerController player)
        {
            if (player.WeaponInventory.HasWeaponEquipped)
            {
                if (player.IsAiming)
                {
                    player.CalculateRotation();
                    player.RotateCharacter(0.15F);
                }
            }
        }
    }

    public void OnStateCheck()
    {

        if (stateMachine.Character is PlayerController player)
        {
            if (PlayerInput.Instance.InputMagnitude > 0)
            { 
                stateMachine.TransitionTo(PlayerInput.Instance.IsRunning ? stateMachine.runState : stateMachine.walkState);
            }

            if (Mathf.Floor(player.Animator.velocity.magnitude) > 0.1)
            {
                stateMachine.CheckFeet(out _);
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

        if (stateMachine.Character is PlayerController player)
        {
            bool IsAiming = player.WeaponInventory.HasWeaponEquipped && PlayerInput.Instance.IsAiming;
            player.SetAiming(IsAiming);
            stateMachine.Animator.SetBool(GameStrings.IS_AIMING, IsAiming);
            stateMachine.Animator.SetFloat(GameStrings.INPUT_X, 0, 0.5F, Time.deltaTime);
            stateMachine.Animator.SetFloat(GameStrings.INPUT_Y, 0, 0.5F, Time.deltaTime);

            if (IsAiming)
            {
                switch (player.WeaponInventory.CurrentWeapon)
                {
                    case TwoHandedWeapon:
                        stateMachine.Animator.SetFloat(GameStrings.CAMERA_X, CameraController.Instance.GetCameraPitchYaw().x);
                        break;
                }
            }
        }
    }
}

public class WalkState : ILocomotionState
{
    LocomotionStateMachine stateMachine;
    readonly float assignedSpeed;

    public WalkState(LocomotionStateMachine stateMachine, float assignedSpeed)
    {
        this.stateMachine = stateMachine;
        this.assignedSpeed = assignedSpeed;
    }

    public void OnStateEnter()
    {
        stateMachine.ResetFeet();
        stateMachine.Character.TargetSpeed = assignedSpeed;

        if (stateMachine.Character is PlayerController player)
        {
            player.CalculateRotation();
        }
    }

    public void OnStateUpdate()
    {
        if (stateMachine.Character is PlayerController player)
        {
            player.CalculateSpeed();

            if (player.Animator.IsCurrentStateTag("Loop", 0) || player.Animator.IsCurrentStateTag("Loop", 1))
            {
                player.CalculateRotation();
                player.RotateCharacter();
                stateMachine.ResetFeet();
            }

            if (player.WeaponInventory.HasWeaponEquipped)
            {
                if (player.IsAiming)
                {
                    player.CalculateRotation();
                    player.RotateCharacter(0.15F);

                    switch (player.WeaponInventory.CurrentWeapon)
                    {
                        case TwoHandedWeapon:
                            stateMachine.Animator.SetFloat(GameStrings.CAMERA_X, CameraController.Instance.GetCameraPitchYaw().x);
                            break;
                    }
                }
            }
        }
    }

    public void OnStateCheck()
    {
        if (stateMachine.Character is PlayerController player)
        {
            if (Mathf.Floor(PlayerInput.Instance.InputMagnitude) > 0)
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
            bool IsAiming = player.WeaponInventory.HasWeaponEquipped && PlayerInput.Instance.IsAiming;
            player.SetAiming(IsAiming);
            stateMachine.Animator.SetBool(GameStrings.IS_AIMING, IsAiming);
            stateMachine.Animator.SetFloat(GameStrings.INPUT_MAGNITUDE, PlayerInput.Instance.InputMagnitude);
            stateMachine.Animator.SetFloat(GameStrings.TARGET_ROTATION, player.GetAngle());
            stateMachine.Animator.SetFloat(GameStrings.CURRENT_SPEED, player.CurrentSpeed);
            stateMachine.Animator.SetFloat(GameStrings.INPUT_X, PlayerInput.Instance.InputDir.x, 0.1F, Time.deltaTime);
            stateMachine.Animator.SetFloat(GameStrings.INPUT_Y, PlayerInput.Instance.InputDir.y, 0.1F, Time.deltaTime);

            if (IsAiming)
            {
                switch (player.WeaponInventory.CurrentWeapon)
                {
                    case TwoHandedWeapon:
                        stateMachine.Animator.SetFloat(GameStrings.CAMERA_X, CameraController.Instance.GetCameraPitchYaw().x);
                        break;
                }
            }
        }
    }
}

public class RunState : ILocomotionState
{
    readonly LocomotionStateMachine stateMachine;
    readonly float assignedSpeed;

    public RunState(LocomotionStateMachine stateMachine, float assignedSpeed)
    {
        this.stateMachine = stateMachine;
        this.assignedSpeed = assignedSpeed;
    }

    public void OnStateEnter()
    {
        stateMachine.ResetFeet();

        if (stateMachine.Character is PlayerController player)
        {
            player.CalculateRotation();
            player.TargetSpeed = assignedSpeed;
        }
    }

    public void OnStateUpdate()
    {
        if (stateMachine.Character is PlayerController player)
        {
            player.CalculateSpeed();

            if (player.Animator.IsCurrentStateTag("Loop", 0) || player.Animator.IsCurrentStateTag("Loop", 1))
            {
                player.CalculateRotation();
                player.RotateCharacter();
                stateMachine.ResetFeet();
            }

            if (player.WeaponInventory.HasWeaponEquipped)
            {
                if (player.IsAiming)
                {
                    player.CalculateRotation();
                    player.RotateCharacter(0.15F);

                    switch (player.WeaponInventory.CurrentWeapon)
                    {
                        case TwoHandedWeapon:
                            stateMachine.Animator.SetFloat(GameStrings.CAMERA_X, CameraController.Instance.GetCameraPitchYaw().x);
                            break;
                    }
                }
            }
        }
    }

    public void OnStateCheck()
    {
        if (stateMachine.Character is PlayerController player)
        {
            if (Mathf.Floor(PlayerInput.Instance.InputMagnitude) > 0)
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
        stateMachine.ResetFeet();
    }

    public void OnAnimate()
    {
        if (stateMachine.Character is PlayerController player)
        {
            bool IsAiming = player.WeaponInventory.HasWeaponEquipped && PlayerInput.Instance.IsAiming;
            player.SetAiming(IsAiming);
            stateMachine.Animator.SetBool(GameStrings.IS_AIMING, IsAiming);
            stateMachine.Animator.SetFloat(GameStrings.INPUT_MAGNITUDE, PlayerInput.Instance.InputMagnitude);
            stateMachine.Animator.SetFloat(GameStrings.TARGET_ROTATION, player.GetAngle());
            stateMachine.Animator.SetFloat(GameStrings.CURRENT_SPEED, player.CurrentSpeed);
            stateMachine.Animator.SetFloat(GameStrings.INPUT_X, PlayerInput.Instance.InputDir.x, 0.1F, Time.deltaTime);
            stateMachine.Animator.SetFloat(GameStrings.INPUT_Y, PlayerInput.Instance.InputDir.y, 0.1F, Time.deltaTime);

            if (IsAiming)
            {
                switch (player.WeaponInventory.CurrentWeapon)
                {
                    case TwoHandedWeapon:
                        stateMachine.Animator.SetFloat(GameStrings.CAMERA_X, CameraController.Instance.GetCameraPitchYaw().x);
                        break;
                }
            }
        }
    }
}

public class StopState : ILocomotionState
{
    readonly LocomotionStateMachine stateMachine;
    readonly float assignedSpeed;

    float inputXOnEnter;
    float inputYOnEnter;

    public StopState(LocomotionStateMachine stateMachine, float assignedSpeed)
    {
        this.stateMachine = stateMachine;
        this.assignedSpeed = assignedSpeed;
    }

    public void OnStateEnter()
    {
        stateMachine.CheckFeet(out _);
        stateMachine.Character.TargetSpeed = assignedSpeed;

        inputXOnEnter = stateMachine.Animator.GetFloat(GameStrings.INPUT_X);
        inputYOnEnter = stateMachine.Animator.GetFloat(GameStrings.INPUT_Y);
    }

    public void OnStateUpdate()
    {
        if (stateMachine.Character is PlayerController player)
        {
            player.CalculateSpeed();
        }
    }

    public void OnStateCheck()
    {
        if (Mathf.Floor(stateMachine.Character.CurrentSpeed) <= 0)
        {
            stateMachine.TransitionTo(stateMachine.idleState);
        }
        else
        if (stateMachine.Character is PlayerController)
        {
            if (PlayerInput.Instance.InputMagnitude <= 0)
            {
                stateMachine.TransitionTo(stateMachine.idleState);

            }
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
            bool IsAiming = player.WeaponInventory.HasWeaponEquipped && PlayerInput.Instance.IsAiming;
            player.SetAiming(IsAiming);
            stateMachine.Animator.SetBool(GameStrings.IS_AIMING, IsAiming);
            stateMachine.Animator.SetFloat(GameStrings.INPUT_MAGNITUDE, PlayerInput.Instance.InputMagnitude);
            stateMachine.Animator.SetFloat(GameStrings.TARGET_ROTATION, player.TargetRotation);
            stateMachine.Animator.SetFloat(GameStrings.CURRENT_SPEED, player.CurrentSpeed);
            stateMachine.Animator.SetFloat(GameStrings.INPUT_X, inputXOnEnter);
            stateMachine.Animator.SetFloat(GameStrings.INPUT_Y, inputYOnEnter);

            if (IsAiming)
            {
                switch (player.WeaponInventory.CurrentWeapon)
                {
                    case TwoHandedWeapon:
                        stateMachine.Animator.SetFloat(GameStrings.CAMERA_X, -CameraController.Instance.GetCameraPitchYaw().x);
                        break;
                }
            }
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
