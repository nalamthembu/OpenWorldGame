using UnityEngine;

public class BaseCharacterStateMachine : MonoBehaviour
{
    [SerializeField] protected float randomiseIdleTimer = 5;
    [SerializeField] protected int IdleAnimationCount = 3;
    private float m_IdleRandomiseTimer = 0;

    protected BaseCharacterWeaponHandler m_WeaponHandler;
    protected BaseCharacter m_Character;
    protected Animator m_Animator;
    protected CharacterSpeech m_CharacterSpeech;

    public BaseCharacter Character { get { return m_Character; } }
    public bool IsRunning { get; protected set; }
    public bool IsCrouchedOrProne { get; protected set; }
    public float PlayerSpeed { get; protected set; }
    public Animator Animator { get { return m_Animator; } }
    public float InputMagnitude { get; protected set; }
    public float Rotation { get; protected set; }
    public void RandomiseIdleAnimation() => m_Animator.SetInteger(GameStrings.IDLE_INDEX, Random.Range(0, IdleAnimationCount));
    public BaseCharacterWeaponHandler WeaponHandler => m_WeaponHandler;
    public CharacterSpeech CharacterSpeech => m_CharacterSpeech;

    protected virtual void Update()
    {
        HandleRandomIdleAnimation();
        SetAnimationValues();
    }
    protected virtual void Awake()
    {
        m_Character = GetComponent<BaseCharacter>();
        m_Animator = GetComponent<Animator>();
        m_WeaponHandler = GetComponent<BaseCharacterWeaponHandler>();
        m_CharacterSpeech = GetComponent<CharacterSpeech>();
    }
    protected virtual void SetAnimationValues() { }
    private void HandleRandomIdleAnimation()
    {
        m_IdleRandomiseTimer += Time.deltaTime;
        if (m_IdleRandomiseTimer >= randomiseIdleTimer)
        {
            RandomiseIdleAnimation();
            m_IdleRandomiseTimer = 0;
        }
    }
}