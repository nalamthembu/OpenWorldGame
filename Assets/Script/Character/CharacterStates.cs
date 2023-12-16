using UnityEngine;

//Central class for character states
public class OnFootState : ICharacterState
{
    CharacterStateMachine stateMachine;

    public OnFootState(CharacterStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public void OnAnimate()
    {
        if (stateMachine.Character is PlayerController player)
        {
            if (PlayerInput.Instance.EquipLongArm && player.WeaponInventory.TryEquipWeapon(WeaponType.LONG_ARM))
            {
                player.Animator.SetBool(GameStrings.IS_RIFLE_EQUIPPED, true);
                player.Animator.CrossFadeInFixedTime(GameStrings.EQUIP_LONG_ARM, 0.25F, 1);
                player.Animator.CrossFadeInFixedTime(GameStrings.EMPTY_STATE, 0.25F, 0);
            }

            if (!player.WeaponInventory.HasWeaponEquipped && !player.Animator.IsCurrentStateName(GameStrings.EMPTY_STATE, 1))
            {
                player.Animator.CrossFadeInFixedTime(GameStrings.EMPTY_STATE, 0.25F, 1);
            }
        }
    }

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

    public void OnAnimate()
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