using UnityEngine;
using Controller = PlayerController;
using System;

public class PlayerStates 
{ 
/*
    * This should be empty, 
    * I'm using this file to store all the states 
    * instead of having multiple files.
*/
}

public abstract class PlayerState
{
    public abstract void OnEnter(PlayerStateMachine stateMachine);
    public abstract void OnUpdate(PlayerStateMachine stateMachine);
    public abstract void OnExit(PlayerStateMachine stateMachine);
    public abstract void OnCheckStateSwitch(PlayerStateMachine stateMachine);
}

public class IdleState : PlayerState
{
    float idleRandomiseTimer;

    public override void OnCheckStateSwitch(PlayerStateMachine stateMachine)
    {
        if (stateMachine.InputMagnitude > 0)
        {
            stateMachine.ResetFeet();

            stateMachine.RotatePlayerToFaceCameraDirection();

            if (Controller.Instance && Controller.Instance.RunHeld)
                stateMachine.SwitchState(stateMachine.runState);
            else
                stateMachine.SwitchState(stateMachine.walkState);
        }
        else
        {
            if (Controller.Instance && Controller.Instance.IsCrouched)
            {
                stateMachine.SwitchState(stateMachine.crouchState);
            }
        }
    }

    public override void OnEnter(PlayerStateMachine stateMachine)
    {
        idleRandomiseTimer = 0;
    }

    public override void OnExit(PlayerStateMachine stateMachine)
    {
        //Don't need to do anything.
    }

    public override void OnUpdate(PlayerStateMachine stateMachine)
    {
        HandleRandomIdleAnimation(stateMachine);

        stateMachine.RotatePlayerToFaceCameraDirection();

        OnCheckStateSwitch(stateMachine);
    }

    private void HandleRandomIdleAnimation(PlayerStateMachine stateMachine)
    {
        idleRandomiseTimer += Time.deltaTime;

        if (idleRandomiseTimer >= stateMachine.RandomiseIdleTimer)
        {
            stateMachine.RandomiseIdleAnimation();

            idleRandomiseTimer = 0;
        }
    }
}

public class StopState : PlayerState
{
    public override void OnEnter(PlayerStateMachine stateMachine)
    {
        //Do feet check
        stateMachine.SetFeet();
    }

    public override void OnExit(PlayerStateMachine stateMachine)
    {
        //Don't need to do anything here.
    }

    //TODO : Do I need to rotate the player?
    public override void OnUpdate(PlayerStateMachine stateMachine)
    {
        OnCheckStateSwitch(stateMachine);
    }

    public override void OnCheckStateSwitch(PlayerStateMachine stateMachine)
    {
        if (Controller.Instance)
        {
            //if the player suddenly gives input while stopping.
            if (stateMachine.InputMagnitude > 0)
            {
                //switch to relevant state.
                if (Controller.Instance.RunHeld)
                    stateMachine.SwitchState(stateMachine.runState);
                else
                    stateMachine.SwitchState(stateMachine.walkState);
            }
            else
            {
                //otherwise just go back to idle.
                stateMachine.SwitchState(stateMachine.idleState);
            }
        }
    }
}

public class WalkState : PlayerState
{
    public override void OnEnter(PlayerStateMachine stateMachine)
    {
        //Make sure feet check is reset.
        stateMachine.ResetFeet();
    }

    public override void OnExit(PlayerStateMachine stateMachine)
    {
        //Don't need to do anything.
    }

    public override void OnUpdate(PlayerStateMachine stateMachine)
    {
        stateMachine.RotatePlayerToFaceCameraDirection();

        OnCheckStateSwitch(stateMachine);
    }
    
    public override void OnCheckStateSwitch(PlayerStateMachine stateMachine)
    {
        if (Controller.Instance)
        {
            //Check if player is trying to run.
            if (Controller.Instance.RunHeld)
                stateMachine.SwitchState(stateMachine.runState);

            //Check if player has stopped giving input
            if (stateMachine.InputMagnitude <= 0)
                stateMachine.SwitchState(stateMachine.stopState);
        }
    }
}

public class RunState : PlayerState
{
    public override void OnEnter(PlayerStateMachine stateMachine)
    {
        //Make sure foot is reset
        stateMachine.ResetFeet();
    }

    public override void OnExit(PlayerStateMachine stateMachine)
    {
        //Don't need to do anything.
    }

    public override void OnUpdate(PlayerStateMachine stateMachine)
    {
        stateMachine.RotatePlayerToFaceCameraDirection();
        OnCheckStateSwitch(stateMachine);
    }

    public override void OnCheckStateSwitch(PlayerStateMachine stateMachine)
    {
        if (Controller.Instance)
        {
            //Check if player has stopped trying to run.
            if (!Controller.Instance.RunHeld)
                stateMachine.SwitchState(stateMachine.walkState);

            //Check if player has stopped giving input
            if (stateMachine.InputMagnitude <= 0)
                stateMachine.SwitchState(stateMachine.stopState);
        }
    }
}

public class CrouchState : PlayerState
{
    public static event Action OnPlayerCrouch;

    public override void OnCheckStateSwitch(PlayerStateMachine stateMachine)
    {
        if (Controller.Instance)
        {
            //go prone if the player is trying to.
            if (Controller.Instance.IsProne)
                stateMachine.SwitchState(stateMachine.proneState);
            //TODO : Check for player falling.


            //If the player stops crouching
            if (!Controller.Instance.IsCrouched)
            {
                if (stateMachine.InputMagnitude > 0)
                {
                    if (!Controller.Instance.RunHeld)
                        stateMachine.SwitchState(stateMachine.runState);
                    else
                        stateMachine.SwitchState(stateMachine.walkState);
                }
                else
                    stateMachine.SwitchState(stateMachine.idleState);
            }
        }
    }

    //Send out event notification.
    public override void OnEnter(PlayerStateMachine stateMachine)
    {
        //RESET FEET
        stateMachine.ResetFeet();
        OnPlayerCrouch?.Invoke();
    }

    public override void OnExit(PlayerStateMachine stateMachine)
    {
        //Don't do anything.
    }
    
    public override void OnUpdate(PlayerStateMachine stateMachine)
    {
        stateMachine.RotatePlayerToFaceCameraDirection();

        if (stateMachine.InputMagnitude <= 0)
            stateMachine.SetFeet();
        else
            stateMachine.ResetFeet();

        OnCheckStateSwitch(stateMachine);
    }
    
}

public class ProneState : PlayerState
{
    public static event Action OnPlayerProne;

    public override void OnCheckStateSwitch(PlayerStateMachine stateMachine)
    {
        if (Controller.Instance)
        {
            //crouch if the player has requested to stop being prone.
            if (!Controller.Instance.IsProne)
                stateMachine.SwitchState(stateMachine.crouchState);
            //TODO : Check for player falling.
        }
    }

    //Send out event notification.
    public override void OnEnter(PlayerStateMachine stateMachine)
    {
        //RESET FEET
        stateMachine.ResetFeet();
        OnPlayerProne?.Invoke();
    }

    public override void OnExit(PlayerStateMachine stateMachine)
    {
        //Don't need to do anything.
    }

    public override void OnUpdate(PlayerStateMachine stateMachine)
    {
        stateMachine.RotatePlayerToFaceCameraDirection(true);

        OnCheckStateSwitch(stateMachine);
    }

}

public class RagdollState : PlayerState
{
    public override void OnCheckStateSwitch(PlayerStateMachine stateMachine)
    {
        //TODO : Check if we've settled or if we've hit the ground.
    }

    public override void OnEnter(PlayerStateMachine stateMachine)
    {
        //TODO : Enable Ragdoll

        //TODO : Play sound from player (screaming)
    }

    public override void OnExit(PlayerStateMachine stateMachine)
    {
        //TODO : Disable Ragdoll

        //TODO : Play sound from player (reacting to fall)
    }

    public override void OnUpdate(PlayerStateMachine stateMachine)
    {
        OnCheckStateSwitch(stateMachine);
    }
}