using UnityEngine;

public class ShieldPickup : BasePickup
{
    [Header("----------General-----------")]
    [Tooltip("What random amount of armor should this give, what is the range?")]
    [SerializeField] Vector2 m_ArmorAmountRange = new(10, 50);

    private float m_Armor;

    public override void Initialise()
    {
        base.Initialise();
        m_Armor = Mathf.Ceil(Random.Range(m_ArmorAmountRange.x, m_ArmorAmountRange.y));
        transform.position = new()
        {
            x = transform.position.x,
            y = 2.5F,
            z = transform.position.z
        };
    }

    protected override void PlayerPickupSound()
    {
        //TODO : Play armor pickup sound....

        //TODO : Pick up particle?

    }

    protected override void DoPlayerPickUp()
    {
        base.DoPlayerPickUp();

        if (PlayerCharacter.Instance != null)
        {
            if (PlayerCharacter.Instance.TryGetComponent<PlayerHealthComponent>(out var armour))
            {
                armour.AddArmour(m_Armor);
            }
        }
    }
}
