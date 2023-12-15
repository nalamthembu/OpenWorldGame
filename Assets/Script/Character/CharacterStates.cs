using UnityEngine;

//Central class for character states
public class OnFootState : ICharacterState
{
    CharacterStateMachine stateMachine;

    bool IsPlayer;

    public OnFootState(CharacterStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public void HandleInput() { }

    public void OnStateEnter() { }

    public void OnStateExit()
    {
        return;
    }

    public void UpdateState()
    {
        if (stateMachine.Character is PlayerController)
        {
            if (PlayerInput.Instance.EnterVehicle)
            {
                if (HasFoundVehicle())
                {
                    stateMachine.TransitionToState(stateMachine.inVehicleState);
                }
            }
        }
    }

    protected bool HasFoundVehicle()
    {
        //Get the first car we find.
        Collider vehicleCollider = Physics.OverlapSphere(stateMachine.Character.transform.position, 5.0F, stateMachine.VehicleLayer)[0];

        Vehicle v = vehicleCollider.GetComponent<Vehicle>();

        stateMachine.Character.SetVehicle(v);

        if (v is null)
            return false;

        return true;
    }
}

public class InVehicleState : ICharacterState
{
    CharacterStateMachine stateMachine;

    bool IsPlayer;

    public InVehicleState(CharacterStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public void HandleInput()
    {
    }

    public void OnStateEnter()
    {
    }

    public void OnStateExit()
    {

    }

    public void UpdateState()
    {
    }
}