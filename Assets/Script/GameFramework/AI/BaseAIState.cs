namespace OWFramework.AI
{
    public abstract class BaseAIState
    {
        // What happens just before we enter the state.
        public abstract void OnEnter(BaseAIStateMachine machine);

        // What happens just before we exit this state.
        public abstract void OnExit(BaseAIStateMachine machine);

        // What happens during each 'Tick' or frame we are in the state.
        public abstract void OnUpdate(BaseAIStateMachine machine);

        // What happens during each check for a need to switch states.
        public abstract void OnCheckTransition(BaseAIStateMachine machine);

        // Draw any needed Gizmos.
        public virtual void OnDrawGizmos() { }
    }
}