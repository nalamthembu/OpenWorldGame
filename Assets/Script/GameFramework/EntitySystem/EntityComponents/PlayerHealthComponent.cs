using System;
using UnityEngine;

public class PlayerHealthComponent : HealthComponent
{
    public static event Action OnPlayerDeath;

    public static event Action<float, float> OnHealthChange;

    public static event Action OnTakeDamage;

    public override void TakeDamage(float fDamage, GameObject Instigator, GameObject DamageCauser, bool IsHeadshot)
    {
        base.TakeDamage(fDamage, Instigator, DamageCauser);

        OnTakeDamage?.Invoke();

        OnHealthChange?.Invoke(m_Health, m_Armor);

        if (IsDead)
            OnPlayerDeath?.Invoke();
    }

    protected override void Start()
    {
        OnHealthChange?.Invoke(m_Health, m_Armor);
    }

    public override void AddHealth(float amount)
    {
        base.AddHealth(amount);

        OnHealthChange?.Invoke(m_Health, m_Armor);
    }

    public override void AddArmour(float amount)
    {
        base.AddArmour(amount);

        OnHealthChange?.Invoke(m_Health, m_Armor);
    }
}