using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Random = UnityEngine.Random;

/// <summary>
/// This is a health component and can be attached to any Entity.
/// This can be put on anything that needs 'life'.
/// </summary>
public class HealthComponent : MonoBehaviour, IEntityComponent
{
    [SerializeField][Range(0, 100)] protected float m_Health = 100;
    [SerializeField][Range(0, 100)] protected float m_Armor = 0;

    [HideInInspector] public bool m_RandomiseHealth;
    [HideInInspector] public Vector2 randomHealthValues;
    [HideInInspector] public Vector2 randomArmorValues;

    public bool IsDead { get; private set; }

    //<this entity, Instigator, DamageCauser>
    public static event Action<Entity, Entity, Entity> OnEntityDead;

    protected Entity m_thisEntity;

    protected virtual void Awake() => m_thisEntity = GetComponent<Entity>();

    protected virtual void Start() { /* child classes use this. */ }

    private void OnValidate()
    {
        if (m_Health <= 0)
        {
            if (TryGetComponent<Entity>(out var attachedEntity))
                OnEntityDead(attachedEntity, null, null);
            else
                Debug.LogError("This component is not attached to an Entity.");

            IsDead = true;

            enabled = false;

            return;
        }
    }

    public virtual void AddHealth(float amount)
    {
        amount = Mathf.Abs(amount);

        m_Health += amount;

        m_Health = m_Health > 100 ? 100 : m_Health;
    }

    public virtual void AddArmour(float amount)
    {
        amount = Mathf.Abs(amount);

        m_Armor += amount;

        m_Armor = m_Armor > 100 ? 100 : m_Armor;
    }

    public virtual void TakeDamage(float fDamage, Entity Instigator, Entity DamageCauser)
    {
        if (IsDead)
            return;
        else
        {
            if (m_Armor > 0)
            {
                m_Armor -= fDamage;

                if (m_Armor <= 0)
                    m_Armor = 0;

                return;
            }

            if (m_Health > 0)
            {
                m_Health -= fDamage;

                if (m_Health <= 0)
                {
                    IsDead = true;

                    m_Health = 0;

                    //Activate Ragdoll
                    if (m_thisEntity is BaseCharacter character && character.PuppetMaster != null)
                        character.PuppetMaster.Kill();

                    if (TryGetComponent<Entity>(out var attachedEntity))
                        OnEntityDead(attachedEntity, Instigator, DamageCauser);
                }
            }
        }
    }
    
    public void RandomiseValues()
    {
        m_Health = Random.Range(randomHealthValues.x, randomHealthValues.y);
        m_Armor = Random.Range(randomArmorValues.x, randomArmorValues.y);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(HealthComponent))]
public class RandomScript_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // for other non-HideInInspector fields

        HealthComponent healthComp = (HealthComponent)target;

        // draw checkbox for the bool
        healthComp.m_RandomiseHealth = EditorGUILayout.Toggle("Randomise Health Values", healthComp.m_RandomiseHealth);
        if (healthComp.m_RandomiseHealth) // if bool is true, show other fields
        {
            if (healthComp.randomHealthValues.magnitude <= 0)
            {
                healthComp.randomHealthValues.x = Random.Range(25, 50);
                healthComp.randomHealthValues.y = Random.Range(50, 100);
            }

            healthComp.randomArmorValues = EditorGUILayout.Vector2Field("Random Armor Values", healthComp.randomArmorValues);
            healthComp.randomHealthValues = EditorGUILayout.Vector2Field("Random Health Values", healthComp.randomHealthValues);

            if (GUILayout.Button("Randomise"))
            {
                healthComp.RandomiseValues();
            }
        }
    }
}
#endif