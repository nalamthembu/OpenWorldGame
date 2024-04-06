using System;
using UnityEngine;
using RootMotion.Dynamics;

/// <summary>
/// This is a health component and can be attached to any Entity.
/// This can be put on anything that needs 'life'.
/// </summary>
public class HealthComponent : MonoBehaviour, IEntityComponent
{
    [SerializeField][Range(0, 100)] protected float m_Health = 100;
    [SerializeField][Range(0, 100)] protected float m_Armor = 0;
    [SerializeField][Range(0, 100)] protected float m_MaxFallDamage = 100;
    [SerializeField][Range(0, 5)] protected float m_FallDamageMultiplier = 1.25F;
    [SerializeField] BehaviourFall m_FallBehaviour;

    #region Delegates & Events
    // DELEGATES
    public delegate void OnHealthChangedDelegate(float health, float armor);
    public delegate void OnBeginFallDelegate(float initialHeight);
    public delegate void OnEndFallDelegate(float initialHeight, Vector3 impactPoint);
    public delegate void OnEntityDeathDelegate(bool fromHeadshot);
    public delegate void OnEntityTakeDamageDelegate(PainLevel painLevel);
    // EVENTS
    public event OnHealthChangedDelegate OnHealthChanged;
    public event OnBeginFallDelegate OnBeginFall;
    public event OnEndFallDelegate OnEndFall;
    public event OnEntityDeathDelegate OnEntityDeath;
    public event OnEntityTakeDamageDelegate OnEntityTakeDamage;
    #endregion

    GameObject m_LastKnownDamageCauser;

    //FLAGS
    public bool IsDead { get; private set; }
    public bool CharacterIsFalling {get; private set; }
    //

    protected Entity m_thisEntity;
    public BaseCharacter Character { get { return m_thisEntity as BaseCharacter; } }

    protected virtual void Awake()
    {
        m_thisEntity = GetComponent<Entity>();

        if (m_FallBehaviour == null)
        {
            Debug.LogWarning($"Be careful, Fall Behaviour on {gameObject.name} is NULL.");
        }
        else
        {
            m_FallBehaviour.OnPostMuscleCollision += OnCharacterStoppedFalling;
            m_FallBehaviour.OnPostActivate += OnCharacterStartedFalling;
        }
    }

    protected virtual void Start() { /* child classes use this. */ }

    #region Fall Damage

    private float m_InitialHeight;

    public void OnCharacterStartedFalling()
    {
        if (m_Health <= 0)
            return;

        var RagdollTransform = m_FallBehaviour.puppetMaster.targetRoot;

        float avgHeight = 0;

        Vector3 centre = RagdollTransform.position + transform.up * 1.25F;

        Vector3[] origins = new Vector3[]
        {
            centre,
            centre + transform.forward,
            centre + -transform.forward,
            centre + -transform.right,
            centre + transform.right
        };

        for (int i = 0; i < origins.Length; i++)
        {
            if (Physics.Raycast(origins[i], Vector3.down, out var hit, 100, m_FallBehaviour.raycastLayers, QueryTriggerInteraction.Ignore))
            {
                avgHeight += hit.distance;

                Debug.DrawLine(origins[i], hit.point, Color.green);
            }
        }

        avgHeight /= origins.Length;

        m_InitialHeight = avgHeight;
        
        CharacterIsFalling = true;
        OnBeginFall?.Invoke(m_InitialHeight);
    }

    public void OnCharacterStoppedFalling(MuscleCollision collision)
    {
        if (!CharacterIsFalling || m_Health <= 0 || collision.collision.impulse.magnitude < 3)
            return;

        print($"impulse : {collision.collision.impulse.magnitude}");

        if (m_InitialHeight > 1.25F && SoundManager.Instance != null)
            SoundManager.Instance.PlayInGameSound("CharFX_HighImpactThud", collision.collision.contacts[0].point, true, 3);

        print(m_InitialHeight);
        
        float damage = collision.collision.impulse.magnitude * m_FallDamageMultiplier;
        damage = Mathf.Clamp(damage, 0, m_MaxFallDamage);
        TakeDamage(damage, gameObject, collision.collision.gameObject);
        m_InitialHeight *= 0;
        OnEndFall?.Invoke(m_InitialHeight, collision.collision.contacts[0].point);
        CharacterIsFalling = false;
    }

    #endregion

    #region Cheats/Debug Options
    [ContextMenu("Give Infinite Health")]
    public void GiveInfiniteHealth()
    {
        m_Health = float.PositiveInfinity;
        Debug.Log($"[Infinity Health #ON]:[{m_thisEntity.name}].");
    }

    [ContextMenu("Disable Infinite Health")]
    public void DisableInfiniteHealth()
    {
        m_Health = 100;
        OnHealthChanged?.Invoke(m_Health, m_Armor);
        Debug.Log($"[Infinity Health #OFF]:[{m_thisEntity.name}].");
    }
    #endregion

    #region General Health

    public virtual void AddHealth(float amount)
    {
        amount = Mathf.Abs(amount);

        m_Health += amount;

        m_Health = m_Health > 100 ? 100 : m_Health;

        OnHealthChanged?.Invoke(m_Health, m_Armor);
    }

    public virtual void AddArmour(float amount)
    {
        amount = Mathf.Abs(amount);

        m_Armor += amount;

        m_Armor = m_Armor > 100 ? 100 : m_Armor;

        OnHealthChanged?.Invoke(m_Health, m_Armor);
    }

    public PainLevel CalculatePainLevel(float damage, float currentHealth)
    {
        double healthRatio = (double)currentHealth / 100; // Assuming max health is 100 for simplicity

        if (currentHealth - damage <= 0)
            return PainLevel.Death;

        if (healthRatio > 0.7)
        {
            if (damage < 30)
                return PainLevel.Low;
            else if (damage < 60)
                return PainLevel.Medium;
            else
                return PainLevel.High;
        }
        else if (healthRatio > 0.4)
        {
            if (damage < 20)
                return PainLevel.Low;
            else if (damage < 40)
                return PainLevel.Medium;
            else
                return PainLevel.High;
        }
        else
        {
            if (damage < 10)
                return PainLevel.Low;
            else if (damage < 20)
                return PainLevel.Medium;
            else
                return PainLevel.High;
        }
    }

    public virtual void TakeDamage(float fDamage, GameObject Instigator, GameObject DamageCauser, bool IsHeadshot = false)
    {
        if (IsDead)
            return;

        BaseCharacter character = (BaseCharacter)m_thisEntity;

        OnEntityTakeDamage?.Invoke(CalculatePainLevel(fDamage, m_Health));

        if (m_Armor > 0)
        {
            m_Armor -= fDamage;

            if (m_Armor <= 0)
                m_Armor = 0;

            OnHealthChanged?.Invoke(m_Health, m_Armor);

            return;
        }

        if (m_Health > 0)
        {
            m_Health -= fDamage;

            if (m_Health <= 0)
            {
                IsDead = true;

                m_Health = 0;

                OnHealthChanged?.Invoke(m_Health, m_Armor);

                //Activate Ragdoll
                character.PuppetMaster.Kill();

                OnEntityDeath?.Invoke(IsHeadshot);
            }
        }
    }

    #endregion
}