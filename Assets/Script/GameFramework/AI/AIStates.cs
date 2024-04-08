public abstract class AIState
{
    // What happens just before we enter the state.
    public abstract void OnEnter(AIStateMachine machine);

    // What happens just before we exit this state.
    public abstract void OnExit(AIStateMachine machine);

    // What happens during each 'Tick' or frame we are in the state.
    public abstract void OnUpdate(AIStateMachine machine);

    // What happens during each check for a need to switch states.
    public abstract void OnCheckTransition(AIStateMachine machine);
}

// Don't do anything
public class AIIdleState : AIState
{
    public override void OnCheckTransition(AIStateMachine machine)
    {

    }

    public override void OnEnter(AIStateMachine machine)
    {

    }

    public override void OnExit(AIStateMachine machine)
    {

    }

    public override void OnUpdate(AIStateMachine machine)
    {

    }
}