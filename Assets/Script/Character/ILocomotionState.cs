public interface ILocomotionState
{
    void OnStateEnter();
    void OnStateUpdate();
    void OnStateCheck();
    void OnAnimate();
    void OnStateEnd();
}