using UnityEngine;

public class CharacterStateMachine : MonoBehaviour
{
    private ICharacter characterController;
    public Character Character { get; private set; }

    public Animator Animator { get { return Character.Animator; } }

    [SerializeField] DebugCharacterState debug;

    [SerializeField] LayerMask m_VehicleLayer;

    public LayerMask VehicleLayer { get { return m_VehicleLayer; } }

    //States
    public OnFootState onFootState;
    public InVehicleState inVehicleState;

    private ICharacterState currentState;

    private void Awake()
    {
        characterController = GetComponent<ICharacter>();
        Character = GetComponent<Character>();
    }

    void Start()
    {
        InitialiseStates();
        // Initialize with the default state
        TransitionToState(onFootState);
    }

    void InitialiseStates()
    {
        onFootState = new(this);
        inVehicleState = new(this);
    }

    void Update()
    {
        // Check for input and update the current state
        currentState.OnAnimate();
        currentState.UpdateState();
    }

    public void TransitionToState(ICharacterState state)
    {
        // Exit the current state
        if (currentState != null)
            currentState.OnStateExit();

        // Set the new state
        currentState = state;

        // Enter the new state
        currentState.OnStateEnter();

        //Debugging
        debug.SetState(state);
    }

#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        if (debug.debugCharacterState)
        {
            UnityEditor.Handles.Label(transform.position + (Vector3) debug.labelPosition, "Current Character State : " + debug.currentState.ToString());
        }
    }
#endif

    // Other methods...
}



[System.Serializable]
public struct DebugCharacterState
{
    public enum CharacterState
    {
        ON_FOOT,
        IN_VEHICLE
    }

    public bool debugCharacterState;

    public Vector2 labelPosition;

    public CharacterState currentState;
    public void SetState(ICharacterState currentState)
    {
        switch (currentState)
        {
            case OnFootState: this.currentState = CharacterState.ON_FOOT; break;
            case InVehicleState: this.currentState = CharacterState.IN_VEHICLE; break;
        }
    }
}
