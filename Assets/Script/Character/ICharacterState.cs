public interface ICharacterState
{
    void OnStateEnter();
    void OnStateExit();
    void HandleInput();
    void UpdateState();
}
