using UnityEngine;

public class BaseCharacterStateMachine: MonoBehaviour
{
    [SerializeField] protected float randomiseIdleTimer = 5;
    [SerializeField] protected int IdleAnimationCount = 3;

    protected BaseCharacter m_Character;
    protected Animator m_Animator;

    public BaseCharacter Character { get { return m_Character; } }
    public bool IsRunning { get; protected set; }
    public bool IsCrouchedOrProne { get; protected set; }
    public float PlayerSpeed { get; protected set; }
    public Animator Animator { get { return m_Animator; } }
    public float InputMagnitude { get; protected set; }
    public float Rotation { get; protected set; }
    public float RandomiseIdleTimer { get { return randomiseIdleTimer; } }
}