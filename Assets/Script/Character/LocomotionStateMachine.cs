using UnityEngine;

public class LocomotionStateMachine : MonoBehaviour
{
    public Character Character { get; private set; }

    [SerializeField] LocomotionDebug debug;

    public Animator Animator { get; private set; }

    [SerializeField] private int m_IdleAnimationCount;
    public int IdleAnimationCount { get { return m_IdleAnimationCount; } }

    //States
    public IdleState idleState;
    public WalkState walkState;
    public RunState runState;
    public JumpState jumpState;
    public LandState landState;
    public RollState rollState;
    public InAirState inAirState;
    public StopState stopState;

    //Crouch States (tba)

    //CurrentState
    private ILocomotionState currentState;

    //Foot
    Transform m_LeftFoot;
    Transform m_RightFoot;

    void InitialiseStates()
    {
        idleState = new(this);
        walkState = new(this, Character.WalkSpeed);
        runState = new(this, Character.RunSpeed);
        stopState = new(this, 0); 
        inAirState = new(this);
        jumpState = new(this);
        landState = new(this);
        rollState = new(this);
    }

    private void Awake()
    {
        Character = GetComponent<Character>();
        Animator = GetComponent<Animator>();
    }

    private void InitialiseVars()
    {
        m_LeftFoot = Character.Animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        m_RightFoot = Character.Animator.GetBoneTransform(HumanBodyBones.RightFoot);
    }


    private void Start()
    {
        InitialiseStates();

        InitialiseVars();

        //Initialise with default state
        TransitionTo(idleState);
    }

    private void Update()
    {
        if (currentState != null)
        {
            currentState.OnStateUpdate();
            currentState.OnAnimate();
            currentState.OnStateCheck();
        }
    }

    public void TransitionTo(ILocomotionState state)
    {
        // Exit the current state
        if (currentState != null)
            currentState.OnStateEnd();

        // Set the new state
        currentState = state;

        // Enter the new state
        currentState.OnStateEnter();

        //Debugging
        debug.SetState(state);
    }

    public void ResetFeet()
    {
        Character.Animator.SetBool(GameStrings.LU, false);
        Character.Animator.SetBool(GameStrings.RU, false);
    }

    public void CheckFeet(out string foot)
    {
        bool lu;
        bool ru;

        //Which foot is higher.
        lu = m_LeftFoot.position.y > m_RightFoot.position.y;
        ru = m_LeftFoot.position.y < m_RightFoot.position.y;

        if (lu)
            foot = "LU";
        else
            foot = "RU";

        Character.Animator.SetBool(GameStrings.LU, lu);
        Character.Animator.SetBool(GameStrings.RU, ru);
    }


    public void PlayFootStepSound()
    {
        //TO-DO : Check surface 
        Surface detectedSurface = default;

        if (WorldManager.Instance != null)
        {
            switch (currentState)
            {
                default:
                case WalkState:
                   
                    if (detectedSurface.walkSoundID is null)
                    {
                        if (WorldManager.Instance.SurfaceData.TryGetSurface("DEFAULT_SURFACE", out Surface surface))
                        {
                            SoundManager.Instance.PlayInGameSound(surface.walkSoundID, transform.position, true);
                        }
                    }

                    break;

                case RunState:

                    if (detectedSurface.runSoundID is null)
                    {
                        if (WorldManager.Instance.SurfaceData.TryGetSurface("DEFAULT_SURFACE", out Surface surface))
                        {
                            SoundManager.Instance.PlayInGameSound(surface.runSoundID, transform.position, true);
                        }
                    }

                    break;
            }
        }

    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (debug.debugLocomotionState)
        {
            UnityEditor.Handles.Label(transform.position + (Vector3)debug.labelPosition, "Current Locomotion State : " + debug.currentState.ToString());
        }
#endif
    }
}

#region DEBUG
[System.Serializable]
public struct LocomotionDebug
{
    public LocomotionState currentState;

    public bool debugLocomotionState;

    public Vector2 labelPosition;

    public void SetState(ILocomotionState currentState)
    {
        switch (currentState)
        {
            case IdleState: this.currentState = LocomotionState.IDLE; break;
            case WalkState : this.currentState = LocomotionState.WALK; break;
            case RunState: this.currentState = LocomotionState.RUN; break;
            case StopState: this.currentState = LocomotionState.STOP; break;
            case JumpState: this.currentState = LocomotionState.JUMP; break;
            case InAirState: this.currentState = LocomotionState.IN_AIR; break;
            case LandState: this.currentState = LocomotionState.LAND; break;
            case RollState: this.currentState = LocomotionState.ROLL; break;
        }
    }
}

public enum LocomotionState
{
    IDLE,
    WALK,
    RUN,
    STOP,
    JUMP,
    IN_AIR,
    LAND,
    ROLL
}

#endregion