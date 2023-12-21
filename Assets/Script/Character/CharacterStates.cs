using UnityEngine;

//Central class for character states
public class OnFootState : ICharacterState
{
    CharacterStateMachine stateMachine;

    bool IsMovingTowardVehicle;

    bool IsAttemptingToEnterVehicle;

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
        if (stateMachine.Character is PlayerController player)
        {

            if (PlayerInput.Instance.EnterVehicle && !IsMovingTowardVehicle)
            {
                if (HasFoundVehicle())
                {
                    HUDManager.instance.GetNotifications().ShowNotification("Dear game tester, at the moment you can't enter vehicles, I'm working out some bugs but either the next iteration or the one after, you should be able to at least enter a vehicle.", 10);
                }
            }

            /*
            if (IsMovingTowardVehicle)
            {
                player.Animator.applyRootMotion = false;
                player.Animator.SetFloat(GameStrings.CURRENT_SPEED, player.NavMeshAgent.velocity.magnitude);
                player.Animator.SetFloat(GameStrings.INPUT_MAGNITUDE, player.NavMeshAgent.velocity.normalized.magnitude);
                player.Animator.SetFloat(GameStrings.TARGET_ROTATION, Mathf.DeltaAngle(player.transform.eulerAngles.y, player.CurrentVehicle.DriversSeat.GetEntryPoint().eulerAngles.y));
                //player.transform.LookAt(player.CurrentVehicle.DriversSeat.GetEntryPoint(), player.transform.up);

                if (player.NavMeshAgent.remainingDistance <= 0)
                {
                    IsAttemptingToEnterVehicle = true;

                    if (IsAttemptingToEnterVehicle)
                    {
                        player.Animator.applyRootMotion = true;

                        player.NavMeshAgent.enabled = false;

                        player.Animator.CrossFadeInFixedTime("Enter_Veh_Stage_01", 0.25F);
                    }
                }
            }
            */
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

    public override string ToString()
    {
        return "OnFoot";
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

    public override string ToString()
    {
        return "InVehicle";
    }
}